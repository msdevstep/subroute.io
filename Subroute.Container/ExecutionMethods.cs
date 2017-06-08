using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.ServiceBus.Messaging;
using Subroute.Common;
using Subroute.Core.Exceptions;
using Subroute.Core.Execution;
using Subroute.Core.Utilities;
using Autofac;
using Subroute.Core.Models.Compiler;
using Subroute.Core.Nuget;
using Subroute.Core;
using Subroute.Core.Data;

namespace Subroute.Container
{
    /// <summary>
    /// Contains methods wired up to the web jobs framework.
    /// </summary>
    public class ExecutionMethods
    {
        /// <summary>
        /// Primary entry point for container, accepts in coming requests over the service bus
        /// and executes users code to create a response and republish the response back over
        /// the service bus.
        /// </summary>
        /// <param name="message">Incoming message containing request details.</param>
        /// <param name="response">Outgoing response that publishes the response details to the service bus.</param>
        /// <returns>Empty task with async context.</returns>
        public static async Task ExecuteAsync([ServiceBusTrigger("%Subroute.ServiceBus.RequestTopicName%", "%Subroute.ServiceBus.RequestSubscriptionName%")]BrokeredMessage message, [ServiceBus("%Subroute.ServiceBus.ResponseTopicName%")] ICollector<BrokeredMessage> response)
        {
            await ExecuteInternalAsync(message, response).TraceTimeAsync("Total Execution");
        }

        private static async Task ExecuteInternalAsync(BrokeredMessage message, ICollector<BrokeredMessage> response)
        {
            // We'll always need a response message, so create it now and populate it below.
            var responseMessage = new BrokeredMessage();

            // We'll create the app domain in the outer scope so we can unload it when we are finished (if it was created).
            AppDomain sandboxDomain = null;

            try
            {
                var requestId = (int) message.Properties["RequestId"];

                // Set correlation id of response message using the correlation ID of the request message.
                responseMessage.CorrelationId = message.CorrelationId;
                responseMessage.Properties["RequestId"] = requestId;

                // The request will also load the associated route, so we'll use that feature
                // to reduce the number of SQL calls we make.
                var request = await Program.RequestRepository.GetRequestByIdAsync(requestId).TraceTimeAsync("Load Request");
                var route = request.Route;
                var routeSettings = route.RouteSettings.ToArray();
                var routePackages = route.RoutePackages.ToArray();

                // Trace the incoming request URI.
                Trace.TraceInformation("Trace 'Request Uri' - {0}", request.Uri);

                try
                {
                    var ev = new Evidence();
                    ev.AddHostEvidence(new Zone(SecurityZone.Internet));

                    var assemblyType = typeof(ExecutionSandbox);
                    var assemblyPath = Path.GetDirectoryName(assemblyType.Assembly.Location);
                    var sandboxPermissionSet = TraceUtility.TraceTime("Create Sandbox Permission Set", 
                        () => SecurityManager.GetStandardSandbox(ev));

                    // Exit with an error code if for some reason we can't get the sandbox permission set.
                    if (sandboxPermissionSet == null)
                        throw new EntryPointException("Unable to load the sandbox environment, please contact Subroute.io for help with this error.");

                    TraceUtility.TraceTime("Reconfigure Appropriate Permission Sets", () =>
                    {
                        // Remove access to UI components since we are in a headless environment.
                        sandboxPermissionSet.RemovePermission(typeof(UIPermission));

                        // Remove access to the File System Dialog since we are headless.
                        sandboxPermissionSet.RemovePermission(typeof(FileDialogPermission));

                        // Add the ability to use reflection for invocation and serialization.
                        sandboxPermissionSet.AddPermission(new ReflectionPermission(PermissionState.Unrestricted));

                        // Add the ability to make web requests.
                        sandboxPermissionSet.AddPermission(new WebPermission(PermissionState.Unrestricted));

                        // Add the ability to use the XmlSerializer and the DataContractSerializer.
                        sandboxPermissionSet.AddPermission(new SecurityPermission(SecurityPermissionFlag.SerializationFormatter));

                        // Add permission to access the nuget package directory so that assemblies can be loaded.
                        sandboxPermissionSet.AddPermission(new FileIOPermission(FileIOPermissionAccess.PathDiscovery | FileIOPermissionAccess.Read, Settings.NugetPackageDirectory));
                    });

                    // We'll create a new folder to hold an empty config file we create, and by
                    // doing this, it prevents the user from gaining access to our configuration
                    // file and the settings within, such as connection strings, infrastructure
                    // and other sensitive information we don't want them to have. Plus it will
                    // allow us to change any configuration settings that are specific to their
                    // application domain, such as default settings and other infrastructure.
                    // We must ensure that we have at least the root configuration XML tag in
                    // the configuration file we create or various dependencies will fail
                    // such as XmlSerializer and DataContractSerializer.
                    var directories = TraceUtility.TraceTime("Setup Filesystem", 
                        () => SetupFilesystem(route, routeSettings));

                    // We'll add one last permission to allow the user access to their own private folder.
                    TraceUtility.TraceTime("Set Permission to Read Directory", 
                        () => sandboxPermissionSet.AddPermission(new FileIOPermission(FileIOPermissionAccess.Read, new[] { directories.RootDirectory })));

                    TraceUtility.TraceTime("Create AppDomain", () =>
                    {
                        var appDomainSetup = new AppDomainSetup { ApplicationBase = assemblyPath, ConfigurationFile = directories.ConfigFile };
                        sandboxDomain = AppDomain.CreateDomain("Sandboxed", ev, appDomainSetup, sandboxPermissionSet);
                    });

                    // The ExecutionSandbox is a MarshalByRef type that allows us to dynamically
                    // load their assemblies via a byte array and execute methods inside of
                    // their app domain from our full-trust app domain. It's the bridge that
                    // crosses the app domain boundary.
                    var executionSandbox = TraceUtility.TraceTime("Create ExecutionSandbox Instance", 
                        () => (ExecutionSandbox)sandboxDomain.CreateInstance(
                        assemblyType.Assembly.FullName,
                        assemblyType.FullName,
                        false,
                        BindingFlags.Public | BindingFlags.Instance,
                        null, null, null, null)
                        .Unwrap());

                    // Prepare packages by locating the proper assemblies for the current framework and ensure
                    // they have been downloaded to the packages folder and return their paths.
                    executionSandbox.SetReferences(await PreparePackagesAsync(routePackages).TraceTimeAsync("Prepare Packages"));

                    // To properly load assemblies into the dynamic partial trust assembly, we have to override
                    // the AssemblyResolve method which is only called when an assembly load attempt is made
                    // and fails. We can't use a closure here because to do that, the entire class ExecutionMethods
                    // would have to be serailized across the app domain boundry. So we'll add a string array property
                    // to the ExecutionSandbox class so we can access the references from in the app domain boundry.
                    // Just remember this event is executed inside the partial trust domain.
                    sandboxDomain.AssemblyResolve += (sender, args) =>
                    {
                        var name = new AssemblyName(args.Name);
                        var path = ExecutionSandbox.References.FirstOrDefault(r => Path.GetFileNameWithoutExtension(r) == name.Name);

                        return path == null ? null : Assembly.LoadFrom(path);
                    };

                    // Build the ExecutionRequest object that represents the incoming request
                    // which holds the payload, headers, method, etc. The class is serialized
                    // so it can cross the app domain boundary. So it's serialized in our 
                    // full-trust host app domain, and deserialized and reinstantiated in
                    // the sandbox app domain.
                    var uri = new Uri(request.Uri, UriKind.Absolute);
                    var executionRequest = TraceUtility.TraceTime("Create RouteRequest Instance", 
                        () => new RouteRequest(uri, request.Method)
                        {
                            IpAddress = request.IpAddress,
                            Headers = HeaderHelpers.DeserializeHeaders(request.RequestHeaders),
                            Body = request.RequestPayload
                        });

                    try
                    {
                        // The ExecutionSandbox we'll attempt to locate the best method to execute
                        // based on the incoming request method (GET, POST, DELETE, etc.) and
                        // will pass the ExecutionRequest we created above. In return, we receive
                        // an instance of ExecutionResponse that has been serialized like the request
                        // and deserialized in our full-trust host domain.
                        var executionResponse = TraceUtility.TraceTime("Load and Execute Request", 
                            () => executionSandbox.Execute(route.Assembly, executionRequest));

                        // We'll use the data that comes back from the response to fill out the 
                        // remainder of the database request record which will return the status
                        // code, message, payload, and headers. Then we update the database.
                        request.CompletedOn = DateTimeOffset.UtcNow;
                        request.StatusCode = (int)executionResponse.StatusCode;
                        request.StatusMessage = executionResponse.StatusMessage;
                        request.ResponsePayload = executionResponse.Body;
                        request.ResponseHeaders = RouteResponse.SerializeHeaders(executionResponse.Headers);

                        await Program.RequestRepository.UpdateRequestAsync(request).TraceTimeAsync("Update Request Record");

                        // We'll pass back a small bit of data indiciating to the subscribers of
                        // the response topic listening for our specific correlation ID that indicates
                        // the route code was executed successfully and to handle it as such.
                        responseMessage.Properties["Result"] = (int)ExecutionResult.Success;
                        responseMessage.Properties["Message"] = "Completed Successfully";

                        // Create the response message and send it on its way.
                        response.Add(responseMessage);
                    }
                    catch (TargetInvocationException invokationException)
                    {
                        // These exceptions can occur when we encounter a permission exception where 
                        // the user doesn't have permission to execute a particular block of code.
                        if (invokationException.InnerException is SecurityException securityException)
                            throw new RoutePermissionException(GetPermissionErrorMessage(securityException), invokationException);

                        // Check for BadRequestException, we need to wrap it with the core exception.
                        // These exceptions can occur when query string parsing fails, and since the
                        // user's code doesn't have access to the core exceptions, we'll need to wrap
                        // it instead manually.
                        if (invokationException.InnerException is Common.BadRequestException badRequestException)
                            throw new Core.Exceptions.BadRequestException(badRequestException.Message, badRequestException);

                        // Otherwise it is most likely a custom user exception.
                        throw new CodeException(invokationException.InnerException?.Message ?? "Route raised a custom exception.", invokationException.InnerException);
                    }
                    catch (EntryPointException entryPointException)
                    {
                        // These exceptions occur when an entry point could not be located.
                        // Since we don't have a reference to core in the common library.
                        // We'll instead wrap this exception in a core 
                        // exception to apply a status code.
                        throw new RouteEntryPointException(entryPointException.Message, entryPointException);
                    }
                    catch (SecurityException securityException)
                    {
                        // These exceptions can occur when we encounter a permission exception where 
                        // the user doesn't have permission to execute a particular block of code.
                        throw new RoutePermissionException(GetPermissionErrorMessage(securityException), securityException);
                    }
                    catch (Common.BadRequestException badRequestException)
                    {
                        // These exceptions can occur when query string parsing fails, and since the
                        // user's code doesn't have access to the core exceptions, we'll need to wrap
                        // it instead manually.
                        throw new Core.Exceptions.BadRequestException(badRequestException.Message, badRequestException);
                    }
                    catch (AggregateException asyncException) // Captures async and task exceptions.
                    {
                        // These exceptions occur when an entry point could not be located.
                        // Since we don't have a reference to core in the common library.
                        // We'll instead wrap this exception in a core 
                        // exception to apply a status code.
                        if (asyncException.InnerException is EntryPointException entryPointException)
                            throw new RouteEntryPointException(entryPointException.Message, entryPointException);

                        // These exceptions can occur when we encounter a permission exception where 
                        // the user doesn't have permission to execute a particular block of code.
                        if (asyncException.InnerException is SecurityException securityException)
                            throw new RoutePermissionException(GetPermissionErrorMessage(securityException), securityException);

                        // These exceptions can occur when query string parsing fails, and since the
                        // user's code doesn't have access to the core exceptions, we'll need to wrap
                        // it instead manually.
                        if (asyncException.InnerException is SecurityException badRequestException)
                            throw new Core.Exceptions.BadRequestException(badRequestException.Message, badRequestException);

                        // These are all other exceptions that occur during the execution of
                        // a route. These exceptions are raised by the users code.
                        throw new RouteException(asyncException.InnerException?.Message ?? asyncException.Message, asyncException.InnerException);
                    }
                    catch (Exception routeException)
                    {
                        // These are all other exceptions that occur during the execution of
                        // a route. These exceptions are raised by the users code.
                        throw new RouteException(routeException.Message, routeException);
                    }
                }
                catch (Exception appDomainException)
                {
                    // This exception relates to exceptions configuring the AppDomain and we'll still notify the
                    // user, we just won't give them specific information that could reveal our infrastructure
                    // unless an IStatusCodeException was thrown, meaning it's a public exception.
                    var statusCode = 500;
                    var statusMessage = "An unexpected exception has occurred. Please contact Subroute.io regarding this error.";
                    var statusCodeException = appDomainException as IStatusCodeException;
                    string stackTrace = null;

                    if (statusCodeException != null)
                    {
                        statusCode = (int)statusCodeException.StatusCode;
                        statusMessage = appDomainException.Message;

                        if (appDomainException is CodeException)
                            stackTrace = appDomainException.ToString();
                    }

                    request.CompletedOn = DateTimeOffset.UtcNow;
                    request.StatusCode = statusCode;
                    request.ResponsePayload = PayloadHelpers.CreateErrorPayload(statusMessage, stackTrace);
                    request.ResponseHeaders = HeaderHelpers.GetDefaultHeaders();

                    await Program.RequestRepository.UpdateRequestAsync(request).TraceTimeAsync("Update Request Record (Error)");

                    responseMessage.Properties["Result"] = (int)ExecutionResult.Failed;
                    responseMessage.Properties["Message"] = appDomainException.Message;

                    // Create the response message and send it on its way.
                    response.Add(responseMessage);
                }
            }
            catch (Exception fatalException)
            {
                // These exceptions are absolutely fatal. We'll have to notify the waiting thread
                // via the service bus message, because we're unable to load a related request.
                responseMessage.Properties["Result"] = (int)ExecutionResult.Fatal;
                responseMessage.Properties["Message"] = fatalException.Message;

                // Create the response message and send it on its way.
                response.Add(responseMessage);
            }
            finally
            {
                // Unload the users app domain to recover all memory used by it.
                if (sandboxDomain != null)
                    TraceUtility.TraceTime("Unload AppDomain", 
                        () => AppDomain.Unload(sandboxDomain));
            }
        }

        private static ExecutionDirectories SetupFilesystem(Route route, RouteSetting[] settings)
        {
            var tempDirectory = Path.GetTempPath();
            var userConfigDirectory = Path.Combine(tempDirectory, route.Uri);
            var userConfigFilePath = Path.Combine(userConfigDirectory, "app.config");

            Directory.CreateDirectory(userConfigDirectory);
            
            var configFile = settings.Aggregate(@"<?xml version=""1.0"" encoding=""utf-8"" ?><configuration><appSettings>",
                (current, setting) => current + $"<add key=\"{setting.Name}\" value=\"{setting.Value}\" />{Environment.NewLine}",
                result => $"{result}</appSettings></configuration>");

            File.WriteAllText(userConfigFilePath, configFile);

            return new ExecutionDirectories
            {
                ConfigFile = userConfigFilePath,
                RootDirectory = userConfigDirectory
            };
        }

        private static async Task<string[]> PreparePackagesAsync(RoutePackage[] routePackages)
        {
            // We'll need an instance of the nuget service to download and prepare packages.
            var nugetService = Program.Container.Resolve<INugetService>();

            // Iterate over each route package and ensure it has been downloaded.
            foreach (var package in routePackages)
                await nugetService.DownloadPackageAsync(package);

            // Load all references to be loaded into the new app domain.
            var references = routePackages
                .Select(Dependency.FromRoutePackage)
                .SelectMany(d => nugetService.GetPackageReferences(d))
                .Select(pr => pr.AssemblyPath)
                .ToArray();

            return references;
        }

        private static string GetPermissionErrorMessage(SecurityException exception)
        {
            switch (exception.PermissionType.Name)
            {
                case nameof(FileIOPermission):
                    return "Subroute.io execution environment prevents file system access. Please use other available storage functionality.";
                case nameof(EnvironmentPermission):
                    return "Subroute.io execution environment prevents access to system and user environment variables.";
                case nameof(StorePermission):
                    return "Subroute.io execution environment prevents access to key and certificate stores.";
                case nameof(RegistryPermission):
                    return "Subroute.io execution environment prevents access to the system registry.";
                case nameof(SocketPermission):
                    return "Subroute.io execution environment does not allow establishing a connection to or opening a socket.";
                case nameof(UIPermission):
                    return "Subroute.io execution environment does not allow creating or accessing user interface components.";
                case nameof(FileDialogPermission):
                    return "Subroute.io execution environment does not allow opening or interacting with a file dialog.";
                case nameof(ConfigurationPermission):
                    return "Subroute.io execution environment does not allow access to configuration settings.";
                case nameof(EventLogPermission):
                    return "Subroute.io execution environment does not allow access to the system event log.";
                case nameof(NetworkInformationPermission):
                    return "Subroute.io execution environment does not allow access to information regarding internal network infrastructure.";
                default:
                    return "Subroute.io execution environment does not allow file, registry, and certain reflection statements from executing.";
            }
        }
    }
}
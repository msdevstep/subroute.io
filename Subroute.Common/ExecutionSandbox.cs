using System;
using System.Linq;
using System.Net;
using System.Reflection;
using Subroute.Common.Extensions;
using System.Threading.Tasks;

namespace Subroute.Common
{
    /// <summary>
    /// ExecutionSandbox is necessary to allow loading the dynamic assembly via an assembly with a known location. ExecutionSandbox supports MashalByRefObject which
    /// allows crossing the AppDomain boundry so that the correct method may be called for the HTTP verb. Anything executed inside this method is safe
    /// and within the sandbox environment.
    /// </summary>
    public class ExecutionSandbox : MarshalByRefObject
    {
        /// <summary>
        /// Loads the assembly from the byte array and executes the best matched method based on the incoming request.
        /// </summary>
        /// <param name="assemblyBytes">Byte array containing the assembly to be loaded.</param>
        /// <param name="request"><see cref="RouteRequest"/> object containing all data for the current request.</param>
        /// <returns><see cref="RouteResponse"/> object containing the response for the current request.</returns>
        public RouteResponse Execute(byte[] assemblyBytes, string[] dependencies, RouteRequest request)
        {
            // Run the private async method synchronously.
            var task = ExecuteAsync(assemblyBytes, dependencies, request);
            task.Wait();

            return task.Result;
        }

        private async Task<RouteResponse> ExecuteAsync(byte[] assemblyBytes, string[] dependencies, RouteRequest request)
        {
            // Load all dependent assemblies.
            foreach (var dependency in dependencies)
                Assembly.LoadFile(dependency);

            var assembly = Assembly.Load(assemblyBytes);
            var baseController = typeof (BaseController);
            var classType = assembly.GetTypes().FirstOrDefault(t => t.InheritsFrom(baseController));

            if (classType == null)
                throw new EntryPointException($"Could not locate any types that inherit from '{baseController.Name}'.");
            
            // Locate a public instance method that begins with the http method type.
            var methodType = classType.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Where(m => !m.Name.Equals("GetHashCode") && !m.Name.Equals("GetType"))
                .FirstOrDefault(m => m.Name.StartsWith(request.Method, StringComparison.CurrentCultureIgnoreCase));
            
            if (methodType == null)
                throw new EntryPointException($"No method was found for http verb '{request.Method}'.");

            // For async methods we need to ensure that the method is marked async. Otherwise we will fail
            // with an exception. We could still run it, but we don't want to promote bad code.
            if ((methodType.ReturnType == typeof(Task) || methodType.ReturnType.InheritsFrom(typeof(Task))) && !methodType.IsAsyncMethod())
                throw new EntryPointException($"Found method named '{methodType.Name}' which returns Task, however the method must be marked as async.");

            // Create an instance of controller and execute the method.
            var instance = classType.CreateInstance();
            var result = methodType.Invoke(instance, new object[] { request });

            // Depending on the result, we can determine if a method is asynchronous or not
            // and treat it as such by waiting for it to complete before returning the result.
            switch (result)
            {
                // Standard synchronous return type, return it straight away.
                case RouteResponse resultTyped:
                    return resultTyped;

                // Typed async result, wait for the result then return it.
                case Task<RouteResponse> resultTaskTyped:
                    return await resultTaskTyped;

                // Untyped async result, wait for completion then return a no content result.
                case Task resultTask:
                    await resultTask;

                    return RouteResponse.NoContent;

                // For void methods, we'll return a no content result, otherwise it's an unknown type so throw.
                default:
                    // Void is a valid type, return an empty result.
                    if (methodType.ReturnType == typeof(void))
                        return RouteResponse.NoContent;

                    // All other types should produce an exception because we aren't sure what they want us to do with it.
                    throw new EntryPointException($"Found method named '{methodType.Name}', but has an invalid return type. Valid types are void, RouteResponse, Task, and Task<RouteResponse>.");
            }
        }
    }
}

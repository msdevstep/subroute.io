using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Permissions;
using System.Threading;
using System.Threading.Tasks;
using Subroute.Common.Extensions;

namespace Subroute.Common
{
    /// <summary>
    /// ExecutionSandbox is necessary to allow loading the dynamic assembly via an assembly with a known location. ExecutionSandbox supports MashalByRefObject which
    /// allows crossing the AppDomain boundry so that the correct method may be called for the HTTP verb. Anything executed inside this method is safe
    /// and within the sandbox environment.
    /// </summary>
    public class ExecutionSandbox : MarshalByRefObject
    {
        public RouteResponse Execute(byte[] assemblyBytes, RouteRequest request)
        {
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

            // Create an instance of controller.
            var instance = classType.CreateInstance();

            // Void methods are treated as a no content result, so we'll create the ExecutionResponse object 
            // ourselves with NoContent status code. We'll also invoke the message to ensure it's processed.
            if (methodType.ReturnType == typeof(void))
            {
                methodType.Invoke(instance, new object[] { request });

                return new RouteResponse(HttpStatusCode.NoContent)
                {
                    Body = new byte[0]
                };
            }
                
            // Methods that directly return an ExecutionResponse are considered bare-metal responses, so we'll just pass it along.
            return (RouteResponse) methodType.Invoke(instance, new object[] { request });
        }
    }
}

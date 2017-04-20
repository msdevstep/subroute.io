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

            // Create an instance of controller and execute the method.
            var instance = classType.CreateInstance();
            var result = methodType.Invoke(instance, new object[] { request });

            // Determine how to handle the result of the user method.
            switch (result)
            {
                case RouteResponse resultTyped:
                    return resultTyped;
                case Task<RouteResponse> resultTaskTyped:
                    resultTaskTyped.Wait();

                    return resultTaskTyped.Result;
                case Task resultTask:
                    resultTask.Wait();

                    return RouteResponse.NoContent;
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

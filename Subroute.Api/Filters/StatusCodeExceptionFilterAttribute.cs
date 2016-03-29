using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http.Filters;
using Subroute.Core.Exceptions;

namespace Subroute.Api.Filters
{
    public class StatusCodeExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            if (actionExecutedContext.Exception is IStatusCodeException)
            {
                var statusCodeException = actionExecutedContext.Exception as IStatusCodeException;

                if (statusCodeException is ValidationException)
                {
                    var validationException = statusCodeException as ValidationException;
                    actionExecutedContext.Response = actionExecutedContext.Request.CreateResponse(validationException.StatusCode, validationException.ValidationResult);
                }
                else
                {
                    if (statusCodeException.IsPublic)
                        actionExecutedContext.Response = actionExecutedContext.Request.CreateErrorResponse(statusCodeException.StatusCode, actionExecutedContext.Exception.Message, actionExecutedContext.Exception);
                    else
                        actionExecutedContext.Response = actionExecutedContext.Request.CreateErrorResponse(statusCodeException.StatusCode, actionExecutedContext.Exception);
                }
            }
        }
    }
}
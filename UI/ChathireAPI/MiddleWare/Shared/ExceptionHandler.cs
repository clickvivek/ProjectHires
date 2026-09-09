using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Middleware.Shared
{
    public interface IExceptionHandler
    {
        Task HandleException(HttpContext context);
    }
    public class ExceptionHandler : IExceptionHandler
    {
        public static ILogger logger;

        public async Task HandleException(HttpContext context)
        {
            var error = context.Features.Get<IExceptionHandlerFeature>();
            if (error != null && !(error.Error is ArgumentException))
            {
                //ToDo MOve 500 to const
                context.Response.StatusCode = 500; // or another Status accordingly to Exception Type
                context.Response.ContentType = "application/json";
                var ex = error.Error;
                Type _type = ex.GetType();

                string _message = ex.Message;
                //ToDo MOve 500 to const
                long _code = 500;
                logger.LogError(ex,"Error Occured and Captured by Generic Handler");
                
                _message = "Error Occured , Please contact customer support. Error number :" + _code;
                await context.Response.WriteAsync(new Error()
                {
                    Id = _code.ToString(),
                    Message = ex.StackTrace
                }.ToString(), Encoding.UTF8);
            }
        }
    }
}

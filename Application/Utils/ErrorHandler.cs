using Domain.Entities.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Application.Utils
{
    public static class ErrorHandler
    {
        public static void UseCustomErrors(this IApplicationBuilder app, IHostEnvironment environment)
        {
            app.Use(WriteProductionResponse);
        }

        private static Task WriteProductionResponse(HttpContext httpContext, Func<Task> next)
            => WriteResponse(httpContext, includeDetails: true);

        private static async Task WriteResponse(HttpContext httpContext, bool includeDetails)
        {
            // Try and retrieve the error from the ExceptionHandler middleware
            var exceptionDetails = httpContext.Features.Get<IExceptionHandlerFeature>();
            var ex = exceptionDetails?.Error;

            if (ex != null)
            {
                httpContext.Response.ContentType = "application/problem+json";

                var globalErrorResponse = new BaseAPIErrorResponse
                {
                    Title = "Internal Server Error",
                    Message = ex.Message.Replace("exception:", "").Replace("One or more errors occurred. (", "").Replace(")", ""),
                    StatusCode = 500
                };

                var stream = httpContext.Response.Body;
                await JsonSerializer.SerializeAsync(stream, globalErrorResponse);
            }
        }
    }
}

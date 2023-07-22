using Domain.Entities.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace Application.Utils
{
    public static class ErrorHandler
    {
        public static void UseCustomExceptionHandler(this IApplicationBuilder app)
        {
            app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(async context =>
                {
                    context.Response.StatusCode = 500;
                    context.Response.ContentType = "application/json";

                    var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
                    var exception = exceptionHandlerPathFeature.Error;

                    var errors = new List<ErrorResponse>();
                    var error = new ErrorResponse()
                    {
                        Field = exception.Source,
                        Message = exception.Message.Replace("exception:", "").Replace("One or more errors occurred. (", "").Replace(")", ""),
                        Detail = exception.InnerException
                    };
                    errors.Add(error);

                    var errorResponse = new ErrorResponseModel()
                    {
                        StatusCode = 500,
                        Errors = errors
                    };

                    await context.Response.WriteAsync(JsonConvert.SerializeObject(errorResponse));
                });
            });
        }
    }
}

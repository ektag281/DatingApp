using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using DatingApp.Api.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DatingApp.Api.Middileware
{
    public class ExceptionMiddileware
    {
        private readonly RequestDelegate requestDelegate;
        private readonly ILogger<ExceptionMiddileware> logger;
        private readonly IHostEnvironment enviroment;

        public ExceptionMiddileware(RequestDelegate requestDelegate,
                                    ILogger<ExceptionMiddileware> logger,
                                    IHostEnvironment enviroment)
        {
            this.requestDelegate = requestDelegate;
            this.logger = logger;
            this.enviroment = enviroment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try{
                await requestDelegate(context);
            }
            catch(Exception ex)
            {
                logger.LogError(ex,ex.Message);
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int) HttpStatusCode.InternalServerError;

                var response = enviroment.IsDevelopment() 
                                ? new ApiException(context.Response.StatusCode,ex.Message,ex.StackTrace?.ToString())
                                : new ApiException(context.Response.StatusCode,"Internal Server Error");

                var options = new JsonSerializerOptions{ PropertyNamingPolicy = JsonNamingPolicy.CamelCase} ;
                var json = JsonSerializer.Serialize(response,options);
                await context.Response.WriteAsync(json);
            }
        }


    }
}
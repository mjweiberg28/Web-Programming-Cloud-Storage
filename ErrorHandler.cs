using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using System;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WebProgramming
{
    public class ErrorHandler
    {

        public static void HandleError(IApplicationBuilder errorApp)
        {
            errorApp.Run(async context =>
            {
                var errorFeature = context.Features.Get<IExceptionHandlerFeature>();
                var exception = errorFeature.Error;

                var problemDetails = new ProblemDetails
                {
                    Instance = Guid.NewGuid().ToString()
                };

                if (exception is BadHttpRequestException badHttpRequestException)
                {
                    problemDetails.Title = "Invalid request";
                    problemDetails.Status = (int)typeof(BadHttpRequestException).GetProperty("StatusCode",
                        BindingFlags.NonPublic | BindingFlags.Instance).GetValue(badHttpRequestException);
                    problemDetails.Detail = badHttpRequestException.Message;
                }
                else
                {
                    problemDetails.Title = "An unexpected error occurred!";
                    problemDetails.Status = 500;
                    problemDetails.Detail = exception.ToString();
                }

                context.Response.StatusCode = problemDetails.Status.Value;
                context.Response.WriteJson(problemDetails, "application/problem+json");
                await Task.Delay(0);
            });
        }
    }

    internal static class HttpExtensions
    {
        private static readonly JsonSerializer Serializer = new JsonSerializer
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        public static void WriteJson<T>(this HttpResponse response, T obj, string contentType = null)
        {
            response.ContentType = contentType ?? "application/json";
            using (var writer = new HttpResponseStreamWriter(response.Body, Encoding.UTF8))
            {
                using (var jsonWriter = new JsonTextWriter(writer))
                {
                    jsonWriter.CloseOutput = false;
                    jsonWriter.AutoCompleteOnClose = false;

                    Serializer.Serialize(jsonWriter, obj);
                }
            }
        }
    }
}

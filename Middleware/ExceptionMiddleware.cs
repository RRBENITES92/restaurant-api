using System.Net;
using System.Text.Json;
using RestaurantApi.Responses;
using RestaurantApi.Exceptions;

namespace RestaurantApi.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            int statusCode = 500;
            string message = "Internal server error";

            if (ex is AppException appEx)
            {
                statusCode = appEx.StatusCode;
                message = appEx.Message;
            }

            context.Response.StatusCode = statusCode;

            var response = new
            {
                Success = false,
                Message = message,
                Data = (object?)null
            };

            await context.Response.WriteAsJsonAsync(response);
        }
    }
}
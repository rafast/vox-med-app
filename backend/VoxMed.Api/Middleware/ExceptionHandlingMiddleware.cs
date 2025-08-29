using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace VoxMed.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (InvalidOperationException ex)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            context.Response.ContentType = "application/json";
            var result = JsonSerializer.Serialize(new { error = ex.Message });
            await context.Response.WriteAsync(result);
        }
        catch (KeyNotFoundException ex)
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            context.Response.ContentType = "application/json";
            var result = JsonSerializer.Serialize(new { error = ex.Message });
            await context.Response.WriteAsync(result);
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";
            var result = JsonSerializer.Serialize(new { error = "An unexpected error occurred.", details = ex.Message });
            await context.Response.WriteAsync(result);
        }
    }
}

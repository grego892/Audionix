using System.Net;
using Serilog;

public class LocalhostRestrictionMiddleware
{
    private readonly RequestDelegate _next;

    public LocalhostRestrictionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var blockedPaths = new[] { "/Account/Register", "/Account/ConfirmEmail", "/Account/RegisterConfirmation" };

        if (context.Connection.RemoteIpAddress != null && !IPAddress.IsLoopback(context.Connection.RemoteIpAddress))
        {
            foreach (var path in blockedPaths)
            {
                if (context.Request.Path.StartsWithSegments(path))
                {
                    Log.Information($"--- LocalhostRestrictionMiddleware.cs -- Invoke - Request to {path} not from localhost.  Status403Forbidden");
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return;
                }
            }
        }

        await _next(context);
    }

}

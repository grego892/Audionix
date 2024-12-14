using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Serilog;

namespace Audionix.Middleware
{
    public class LocalhostRestrictionMiddleware
    {
        private readonly RequestDelegate _next;

        public LocalhostRestrictionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var remoteIpAddress = context.Connection.RemoteIpAddress.MapToIPv4();
            Log.Information($"--- LocalhostRestrictionMiddleware -- InvokeAsync() - Remote IP Address: {remoteIpAddress.MapToIPv4()}");
            if (remoteIpAddress == null || !IsLocalhost(remoteIpAddress))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("Access denied. Contact administrator.");
                return;
            }

            await _next(context);
        }

        private bool IsLocalhost(System.Net.IPAddress remoteIpAddress)
        {
            return remoteIpAddress.Equals(System.Net.IPAddress.Loopback) || remoteIpAddress.Equals(System.Net.IPAddress.IPv6Loopback);
        }
    }
}

using System.Security.Claims;

namespace API.Services
{
    public class UserLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public UserLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var user = context.User;

            var userId = user?.FindFirst("UserId")?.Value ?? "Anonymous";
            var email = user?.FindFirst(ClaimTypes.Email)?.Value ?? "Anonymous";
            var role = user?.FindFirst(ClaimTypes.Role)?.Value ?? "None";

            // Serilog'a özel "contextual log" ekle
            using (Serilog.Context.LogContext.PushProperty("UserId", userId))
            using (Serilog.Context.LogContext.PushProperty("Email", email))
            using (Serilog.Context.LogContext.PushProperty("Role", role))
            {
                await _next(context);
            }
        }
    }

}

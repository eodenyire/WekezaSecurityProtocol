using Microsoft.AspNetCore.Http;
using System.Text.Json;
using WekezaSecurityProtocol.Models;

namespace WekezaSecurityProtocol.Middleware
{
    /// <summary>
    /// Salama Routing Middleware - intercepts requests and routes to shadow or standard services
    /// based on session mode detected in JWT token
    /// </summary>
    public class SalamaRoutingMiddleware
    {
        private readonly RequestDelegate _next;

        public SalamaRoutingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Extract session mode from JWT token
            var sessionMode = ExtractSessionMode(context);

            // Add session mode to HttpContext for downstream services
            context.Items["SessionMode"] = sessionMode;
            context.Items["IsShadowMode"] = sessionMode == SessionMode.Shadow;

            // Add custom header for internal routing
            if (sessionMode == SessionMode.Shadow)
            {
                context.Request.Headers["X-Session-Mode"] = "SHADOW";
            }

            // Continue to next middleware
            await _next(context);
        }

        /// <summary>
        /// Extract session mode from JWT token claims
        /// </summary>
        private SessionMode ExtractSessionMode(HttpContext context)
        {
            // Check Authorization header
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader))
            {
                return SessionMode.Standard;
            }

            // Extract Bearer token
            var token = authHeader.StartsWith("Bearer ") 
                ? authHeader.Substring("Bearer ".Length).Trim() 
                : authHeader;

            // Parse JWT token and check for "shadow" scope
            // In production, use proper JWT validation
            var claims = ParseJwtClaims(token);
            if (claims != null && claims.ContainsKey("scope"))
            {
                var scope = claims["scope"];
                if (scope.Contains("shadow", StringComparison.OrdinalIgnoreCase))
                {
                    return SessionMode.Shadow;
                }
            }

            return SessionMode.Standard;
        }

        /// <summary>
        /// Simple JWT claims parser (use proper JWT library in production)
        /// </summary>
        private Dictionary<string, string>? ParseJwtClaims(string token)
        {
            try
            {
                var parts = token.Split('.');
                if (parts.Length != 3)
                {
                    return null;
                }

                // Decode payload (second part)
                var payload = parts[1];
                var jsonBytes = Convert.FromBase64String(PadBase64(payload));
                var json = System.Text.Encoding.UTF8.GetString(jsonBytes);
                
                return JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Pad base64 string for proper decoding
        /// </summary>
        private string PadBase64(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: return base64 + "==";
                case 3: return base64 + "=";
                default: return base64;
            }
        }
    }

    /// <summary>
    /// Extension method to register middleware
    /// </summary>
    public static class SalamaRoutingMiddlewareExtensions
    {
        public static IApplicationBuilder UseSalamaRouting(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SalamaRoutingMiddleware>();
        }
    }
}

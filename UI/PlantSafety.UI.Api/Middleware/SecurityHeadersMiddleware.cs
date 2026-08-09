// ============================================================================
//  Project:        PlantSafety.UI.Api
//  Module:         Middleware
//  File:           SecurityHeadersMiddleware.cs
//  Created:        2026-08-09
//  Description:    Phase 3.9 (Security & Compliance) deliverable. This host
//                  has no authentication/authorization concept at all yet
//                  (flagged separately -- a full auth system is a product
//                  decision, not something to invent unasked inside a
//                  platform-modernization pass). What's safe and valuable
//                  to add without that decision: the standard baseline
//                  security response headers every HTTP API should send
//                  regardless of auth model.
// ============================================================================
namespace PlantSafety.UI.Api.Middleware;

/// <summary>Adds baseline security response headers to every response.</summary>
public sealed class SecurityHeadersMiddleware(RequestDelegate next)
{
    /// <summary>Invokes the middleware.</summary>
    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(() =>
        {
            context.Response.Headers["X-Content-Type-Options"] = "nosniff";
            context.Response.Headers["X-Frame-Options"] = "DENY";
            context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            context.Response.Headers["X-Permitted-Cross-Domain-Policies"] = "none";
            return Task.CompletedTask;
        });

        await next(context);
    }
}

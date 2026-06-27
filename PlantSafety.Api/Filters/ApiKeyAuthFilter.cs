// ============================================================================
// File: ApiKeyAuthFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-12
// Last Updated: 2026-06-12
// Description: Action filter that enforces API key authentication for endpoints
//              serving machine-to-machine or webhook consumer clients.
// ============================================================================
//
// Onboarding Notes:
// - Apply [ServiceFilter(typeof(ApiKeyAuthFilter))] to controller actions or controllers
//   that require API key authentication in addition to (or instead of) JWT bearer auth.
// - The API key is read from the X-API-Key request header and validated against keys
//   stored in Azure Key Vault (accessed via IConfiguration, which reads from Key Vault
//   via the Secret Store CSI driver). Keys are NEVER stored in appsettings.json.
// - Webhook subscription endpoints in the Integrations domain use this filter.
// - Primary human-user authentication uses JWT Bearer tokens via Azure AD (not this filter).
//   This filter is for service-to-service or external system clients only.
// - See Architecture Specification §12.1 (Authentication Flow).
//
// Rationale:
// - Separates API key validation from JWT bearer token validation, enabling endpoint-level
//   granularity for which auth mechanism is applied.
//
// Future Extension Seams:
// - Support multiple valid API keys (key rotation) from a Key Vault secret list.
// - Add rate limiting by API key using IMemoryCache or Redis.
//
// ============================================================================

using Microsoft.AspNetCore.Mvc.Filters;
using PlantSafety.Api.Middleware;
using PlantSafety.Api.Models;

namespace PlantSafety.Api.Filters;

/// <summary>
/// Action filter that validates the X-API-Key header against the platform's configured
/// API key secret. Returns 401 Unauthorized if the key is absent or invalid.
/// </summary>
public sealed class ApiKeyAuthFilter : IAuthorizationFilter
{
    /// <summary>The HTTP header name carrying the API key.</summary>
    public const string ApiKeyHeader = "X-API-Key";

    private const string ApiKeyConfigSection = "ApiSecurity:ApiKey";

    private readonly IConfiguration _configuration;
    private readonly ILogger<ApiKeyAuthFilter> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="ApiKeyAuthFilter"/>.
    /// </summary>
    /// <param name="configuration">The configuration provider (backed by Azure Key Vault in production).</param>
    /// <param name="logger">Logger for recording authentication failures.</param>
    public ApiKeyAuthFilter(IConfiguration configuration, ILogger<ApiKeyAuthFilter> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Validates the X-API-Key header against the platform-configured API key.
    /// Sets a 401 Unauthorized result if the key is missing or does not match.
    /// </summary>
    /// <param name="context">The authorization filter context.</param>
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue(ApiKeyHeader, out var providedKey))
        {
            SetUnauthorizedResult(context, "API key header is missing.");
            return;
        }

        var expectedKey = _configuration[ApiKeyConfigSection];

        if (string.IsNullOrWhiteSpace(expectedKey) || !string.Equals(providedKey, expectedKey, StringComparison.Ordinal))
        {
            var correlationId = context.HttpContext.Items[CorrelationIdMiddleware.ItemsKey]?.ToString()
                                ?? context.HttpContext.TraceIdentifier;

            _logger.LogWarning(
                "Invalid API key presented. CorrelationId: {CorrelationId}",
                correlationId);

            SetUnauthorizedResult(context, "The provided API key is invalid.");
        }
    }

    private static void SetUnauthorizedResult(AuthorizationFilterContext context, string detail)
    {
        var correlationId = context.HttpContext.Items[CorrelationIdMiddleware.ItemsKey]?.ToString()
                            ?? context.HttpContext.TraceIdentifier;

        var problem = ProblemDetailsExtensions.UnauthorizedProblem(correlationId);
        context.Result = new ObjectResult(problem)
        {
            StatusCode = StatusCodes.Status401Unauthorized,
            ContentTypes = { "application/problem+json" }
        };
    }
}

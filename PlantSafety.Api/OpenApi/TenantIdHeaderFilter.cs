// ============================================================================
// File: TenantIdHeaderFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-13
// Last Updated: 2026-06-13
// Description: Swashbuckle IOperationFilter that documents the X-Tenant-ID
//              request header on all non-anonymous OpenAPI operations, clarifying
//              that the tenant is normally resolved from the JWT but can be
//              confirmed or overridden via the header for service-to-service calls.
// ============================================================================
//
// Onboarding Notes:
// - Registered in SwaggerExtensions.AddPlantSafetySwagger() via
//   options.OperationFilter<TenantIdHeaderFilter>().
// - The filter skips anonymous endpoints (those without [Authorize] metadata)
//   to avoid documenting the header on health-check endpoints where it is not
//   relevant.
// - Marked as optional (Required = false) because TenantResolutionMiddleware
//   falls back to the JWT tenantId claim if the header is absent.
//
// Rationale:
// - Without this filter, the X-Tenant-ID header is undocumented in the OpenAPI
//   spec, confusing service-to-service callers that need to set it explicitly.
//
// Future Extension Seams:
// - Upgrade to Required = true for service-to-service endpoints that cannot rely
//   on a JWT claim (API-key-authenticated paths).
//
// ============================================================================

using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using PlantSafety.Api.Constants;

namespace PlantSafety.Api.OpenApi;

/// <summary>
/// Swashbuckle operation filter that documents the <c>X-Tenant-ID</c> request header
/// on all authenticated (non-anonymous) OpenAPI operations.
/// </summary>
public sealed class TenantIdHeaderFilter : IOperationFilter
{
    /// <inheritdoc />
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var isAnonymous = context.MethodInfo
            .GetCustomAttributes(true)
            .OfType<AllowAnonymousAttribute>()
            .Any();

        if (isAnonymous)
            return;

        operation.Parameters ??= [];

        var alreadyDeclared = operation.Parameters.Any(
            p => string.Equals(p.Name, ApiHeaderConstants.TenantId, StringComparison.OrdinalIgnoreCase)
              && p.In == ParameterLocation.Header);

        if (alreadyDeclared)
            return;

        operation.Parameters.Add(new OpenApiParameter
        {
            Name        = ApiHeaderConstants.TenantId,
            In          = ParameterLocation.Header,
            Required    = false,
            Description = "Explicit tenant identifier. If omitted, the tenant is resolved from " +
                          "the JWT tenantId claim by TenantResolutionMiddleware. Required for " +
                          "service-to-service calls authenticated via API key rather than JWT.",
            Schema      = new OpenApiSchema { Type = JsonSchemaType.String, Format = "uuid" }
        });
    }
}

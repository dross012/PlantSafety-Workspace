// ============================================================================
// File: CorrelationIdOperationFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-13
// Last Updated: 2026-06-13
// Description: Swashbuckle IOperationFilter that adds the X-Correlation-ID
//              request header parameter and response header documentation to
//              every OpenAPI operation in the generated spec.
// ============================================================================
//
// Onboarding Notes:
// - Registered in SwaggerExtensions.AddPlantSafetySwagger() via
//   options.OperationFilter<CorrelationIdOperationFilter>().
// - Adds the X-Correlation-ID header as an optional request parameter so clients
//   know they may supply their own correlation ID; the middleware will use it if
//   present or generate one if absent.
// - Also documents the X-Correlation-ID response header on all 200 and 201
//   responses, which is always present regardless of whether the client supplied it.
// - The filter is idempotent: if a parameter with the same name already exists
//   on the operation (from a controller-level attribute), it is not duplicated.
//
// Rationale:
// - Without this filter, the generated OpenAPI spec would not document the
//   correlation ID contract, making it invisible to API consumers and test tooling.
//
// Future Extension Seams:
// - Add documentation for the Idempotency-Key header for POST operations
//   (extract to a separate IdempotencyKeyOperationFilter).
//
// ============================================================================

using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using PlantSafety.Api.Constants;

namespace PlantSafety.Api.OpenApi;

/// <summary>
/// Swashbuckle operation filter that documents the <c>X-Correlation-ID</c> request
/// and response headers on every generated OpenAPI operation.
/// </summary>
public sealed class CorrelationIdOperationFilter : IOperationFilter
{
    /// <inheritdoc />
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters ??= [];

        var alreadyDeclared = operation.Parameters.Any(
            p => p.Name.Equals(ApiHeaderConstants.CorrelationId, StringComparison.OrdinalIgnoreCase)
              && p.In == ParameterLocation.Header);

        if (!alreadyDeclared)
        {
            operation.Parameters.Add(new OpenApiParameter
            {
                Name        = ApiHeaderConstants.CorrelationId,
                In          = ParameterLocation.Header,
                Required    = false,
                Description = "Client-supplied correlation identifier that is echoed back in the " +
                              "response header. A UUID v4/v7 is generated server-side if omitted.",
                Schema      = new OpenApiSchema { Type = "string", Format = "uuid" }
            });
        }

        foreach (var (statusCode, response) in operation.Responses)
        {
            if (statusCode is not ("200" or "201" or "202" or "204"))
                continue;

            response.Headers ??= new Dictionary<string, OpenApiHeader>();

            if (!response.Headers.ContainsKey(ApiHeaderConstants.CorrelationId))
            {
                response.Headers[ApiHeaderConstants.CorrelationId] = new OpenApiHeader
                {
                    Description = "The correlation identifier echoed from the request, or a " +
                                  "server-generated UUID if none was supplied.",
                    Schema = new OpenApiSchema { Type = "string", Format = "uuid" }
                };
            }
        }
    }
}

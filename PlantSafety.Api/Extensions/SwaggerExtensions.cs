// ============================================================================
// File: SwaggerExtensions.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-12
// Last Updated: 2026-06-12
// Description: Swagger/OpenAPI configuration extensions for PlantSafety.Api,
//              registering versioned API documentation for all 16 bounded contexts.
// ============================================================================
//
// Onboarding Notes:
// - Configures Swashbuckle to produce versioned OpenAPI documents for V1 and V2 endpoints.
// - JWT Bearer authentication is added as a security definition so developers can
//   authenticate directly in the Swagger UI during development.
// - Each bounded context controller uses [Tags("ModuleName")] to group endpoints in the UI.
// - In production, Swagger UI is disabled — only the JSON spec endpoint remains available
//   for Azure API Management (APIM) import. This is enforced by checking IWebHostEnvironment.
// - See Architecture Specification §10.4.7 (Required Swagger Grouping).
//
// Rationale:
// - Versioned OpenAPI documentation enables APIM policy binding per API version and
//   allows front-end teams to generate type-safe API clients from the spec.
//
// Future Extension Seams:
// - Add XML comment documentation from PlantSafety.Api.xml to enrich the spec.
// - Configure response examples for each endpoint using Swashbuckle.AspNetCore.Annotations.
//
// ============================================================================

using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace PlantSafety.Api.Extensions;

/// <summary>
/// Extension methods for configuring Swagger/OpenAPI documentation generation in PlantSafety.Api.
/// </summary>
public static class SwaggerExtensions
{
    private const string SecurityDefinitionName = "Bearer";

    /// <summary>
    /// Registers Swashbuckle Swagger generation with versioned API documents and JWT security definition.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddPlantSafetySwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "PlantSafety API",
                Version = "v1",
                Description = "Enterprise industrial safety management API — PlantSafety Platform v3.0",
                Contact = new OpenApiContact
                {
                    Name = "PlantSafety Architecture Team",
                    Email = "architecture@plantsafety.internal"
                }
            });

            options.SwaggerDoc("v2", new OpenApiInfo
            {
                Title = "PlantSafety API",
                Version = "v2",
                Description = "PlantSafety Platform API v2 — Breaking changes and enhanced endpoints",
                Contact = new OpenApiContact
                {
                    Name = "PlantSafety Architecture Team",
                    Email = "architecture@plantsafety.internal"
                }
            });

            options.AddSecurityDefinition(SecurityDefinitionName, new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter your Azure AD JWT Bearer token. Example: Bearer {token}"
            });

            options.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecuritySchemeReference(SecurityDefinitionName),
                    []
                }
            });

            options.DocInclusionPredicate((docName, apiDesc) =>
            {
                if (!apiDesc.TryGetMethodInfo(out var methodInfo))
                    return false;

                var versions = methodInfo.DeclaringType?
                    .GetCustomAttributes(true)
                    .OfType<Asp.Versioning.ApiVersionAttribute>()
                    .SelectMany(attr => attr.Versions)
                    .Select(v => $"v{v.MajorVersion}")
                    .ToList() ?? [];

                return versions.Contains(docName);
            });

            options.EnableAnnotations();
        });

        return services;
    }

    /// <summary>
    /// Configures the Swagger middleware pipeline. Swagger UI is only enabled in non-production environments.
    /// </summary>
    /// <param name="app">The web application builder.</param>
    /// <param name="environment">The current web hosting environment.</param>
    /// <returns>The web application for chaining.</returns>
    public static WebApplication UsePlantSafetySwagger(this WebApplication app, IWebHostEnvironment environment)
    {
        app.UseSwagger(options =>
        {
            options.RouteTemplate = "openapi/{documentName}.json";
        });

        if (!environment.IsProduction())
        {
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/openapi/v1.json", "PlantSafety API V1");
                options.SwaggerEndpoint("/openapi/v2.json", "PlantSafety API V2");
                options.RoutePrefix = "swagger";
                options.DocumentTitle = "PlantSafety API Explorer";
            });
        }

        return app;
    }
}

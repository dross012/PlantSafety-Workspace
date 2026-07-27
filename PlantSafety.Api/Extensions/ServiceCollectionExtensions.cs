// ============================================================================
// File: ServiceCollectionExtensions.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-12
// Last Updated: 2026-06-12
// Description: Central DI registration extension for all PlantSafety.Api services,
//              including middleware, filters, health checks, versioning, and CORS.
// ============================================================================
//
// Onboarding Notes:
// - Called once from Program.cs: builder.Services.AddPlantSafetyApi(builder.Configuration).
// - Each bounded context registers its own DI extensions (e.g., AddAnalyticsApi()) from
//   within its own Extensions folder — they are wired here with a single call per module.
// - MediatR, FluentValidation, and Application layer services are registered in the
//   Application layer's DependencyInjection.cs (PlantSafety.Application), not here.
// - Infrastructure services (Serilog, OpenTelemetry, Prometheus, Dapper, MassTransit,
//   Redis) are registered in PlantSafety.Infrastructure's DependencyInjection.cs.
//   The Presentation layer MUST NOT reference Infrastructure library types directly,
//   except via the DI registration calls. See Architecture Specification §4.1.
// - API versioning uses URL-segment format: /api/v{version:apiVersion}/...
//   See Architecture Specification §4.8 (API Versioning).
//
// Rationale:
// - Centralising all Presentation layer DI in a single well-named extension method
//   keeps Program.cs minimal and makes the registration surface auditable.
//
// Future Extension Seams:
// - Add per-module feature flags (IFeatureManager) for gradual rollout of new bounded contexts.
// - Add Azure Application Configuration refresh registration for runtime config updates.
//
// ============================================================================

using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using PlantSafety.Api.Context;
using PlantSafety.Api.Filters;
using PlantSafety.Api.HealthChecks;
using PlantSafety.Application.Abstractions.Context;

namespace PlantSafety.Api.Extensions;

/// <summary>
/// Extension methods for registering all PlantSafety.Api Presentation layer services
/// into the ASP.NET Core DI container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all Presentation layer services required by PlantSafety.Api,
    /// including controllers, filters, health checks, API versioning, CORS, and module extensions.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddPlantSafetyApi(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddControllers(options =>
        {
            options.Filters.Add<ApiValidationFilter>();
        });

        services.AddScoped<ApiKeyAuthFilter>();

        services.AddPlantSafetyApiVersioning();
        services.AddPlantSafetyAuthentication(configuration);
        services.AddPlantSafetyAuthorization();
        services.AddPlantSafetyHealthChecks(configuration);
        services.AddPlantSafetyCors(configuration);
        services.AddPlantSafetyOutputCache();
        services.AddPlantSafetySwagger();

        services.AddEndpointsApiExplorer();
        services.AddHttpContextAccessor();
        services.AddScoped<IOperationContext, OperationContextAccessor>();
        services.AddScoped<RequestEnvelopeFactory>();

        return services;
    }

    private static IServiceCollection AddPlantSafetyApiVersioning(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        })
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        return services;
    }

    private static IServiceCollection AddPlantSafetyAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = configuration["AzureAd:Authority"];
                options.Audience = configuration["AzureAd:Audience"];
                options.TokenValidationParameters.ValidateLifetime = true;
                options.TokenValidationParameters.ClockSkew = TimeSpan.FromSeconds(30);
            });

        return services;
    }

    private static IServiceCollection AddPlantSafetyAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            // Platform-wide base policy — all authenticated users
            options.AddPolicy("RequireAuthenticatedUser", policy =>
                policy.RequireAuthenticatedUser());

            // Registers the four canonical Safety.Tier.* policies plus every
            // per-module *AuthorizationPolicies policy name discovered via
            // reflection. Previously declared but never invoked — see
            // AuthorizationPolicyExtensions.cs for the full explanation of the
            // defect this fixes.
            options.AddPlantSafetyPolicies();
        });

        return services;
    }

    private static IServiceCollection AddPlantSafetyHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>("database", tags: ["ready"])
            .AddCheck<RedisHealthCheck>("redis", tags: ["ready"])
            .AddCheck<ServiceBusHealthCheck>("servicebus", tags: ["ready"])
            .AddCheck<SchemaVersionHealthCheck>("schema-version", tags: ["ready"]);

        return services;
    }

    private static IServiceCollection AddPlantSafetyCors(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

        services.AddCors(options =>
        {
            options.AddPolicy("PlantSafetyDefault", policy =>
            {
                if (allowedOrigins.Length > 0)
                {
                    policy.WithOrigins(allowedOrigins)
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                }
                else
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                }
            });
        });

        return services;
    }

    private static IServiceCollection AddPlantSafetyOutputCache(this IServiceCollection services)
    {
        services.AddOutputCache(options =>
        {
            options.AddBasePolicy(builder => builder.Expire(TimeSpan.FromMinutes(1)));
        });

        return services;
    }
}

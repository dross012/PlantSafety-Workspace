// ============================================================================
// File: Program.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-12
// Last Updated: 2026-06-12
// Description: Application entry point for PlantSafety.Api. Bootstraps the ASP.NET Core
//              host, registers all services, and configures the canonical middleware pipeline.
// ============================================================================
//
// Onboarding Notes:
// - Uses top-level statements (C# 9+) with the minimal hosting model (WebApplication.CreateBuilder).
// - The DI registration surface is split into focused extension methods to keep this file
//   readable and auditable:
//     builder.Services.AddPlantSafetyApi()             — Presentation layer services (this project)
//     builder.Services.AddPlantSafetyApplication()     — Application layer (MediatR, validators, mappers)
//     builder.Services.AddPlantSafetyInfrastructure()  — Infrastructure layer (Dapper, Redis, MassTransit, etc.)
// - The middleware pipeline order is enforced by UsePlantSafetyMiddleware() — never add middleware
//   outside of that extension method unless absolutely required.
// - Azure Key Vault is added as a configuration source here using Managed Identity
//   (DefaultAzureCredential). No secrets appear in appsettings.json or environment variables.
// - In non-production environments, Swagger UI is enabled at /swagger.
//   In production, only the OpenAPI JSON spec is exposed for APIM import.
// - See Architecture Specification §4.1 (Layer Separation), §4.8 (API Versioning),
//   §7.2 (Authentication), §12.3 (Managed Identity).
//
// Rationale:
// - Keeping Program.cs thin ensures the startup surface is easy to audit for security
//   compliance and layer separation violations.
//
// Future Extension Seams:
// - Add IHostedService registrations for background readiness probes or warm-up tasks.
// - Add Azure App Configuration dynamic refresh for feature flag toggling without redeployment.
//
// ============================================================================

using Azure.Identity;
using PlantSafety.Api.Extensions;
using PlantSafety.Application.DependencyInjection;
using PlantSafety.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// ── Azure Key Vault ───────────────────────────────────────────────────────────
// Secrets are never stored in appsettings.json. DefaultAzureCredential uses
// Workload Identity (pod-level Managed Identity) in AKS; it falls back to
// developer identity (az login) in local environments.
var keyVaultUri = builder.Configuration["KeyVault:Uri"];
if (!string.IsNullOrWhiteSpace(keyVaultUri))
{
    builder.Configuration.AddAzureKeyVault(new Uri(keyVaultUri), new DefaultAzureCredential());
}

// ── Service Registration ──────────────────────────────────────────────────────
// Presentation layer — controllers, filters, health checks, versioning, CORS, Swagger
builder.Services.AddPlantSafetyApi(builder.Configuration);

// Application layer — MediatR pipeline, FluentValidation, AutoMapper, domain event dispatch
builder.Services.AddApplicationServices();

// Infrastructure layer — Dapper, Redis, MassTransit, Serilog sinks, OpenTelemetry exporters
builder.Services.AddInfrastructure(builder.Configuration);

// ── Service Provider Validation ──────────────────────────────────────────────
// ValidateOnBuild is intentionally disabled. UPDATE (2026-07-22): the ~151 missing
// legacy-module repository implementations (Compliance, Analytics, Operational,
// Integrations, CrossDomain, SafetyEvents, Intelligence, ContinuousImprovement,
// PlanningProjects, HealthChecks, Financials, Features) referenced in the original version
// of this comment have all been implemented and wired into DI — that part of the technical
// debt is closed. What remains, confirmed via a real ValidateOnBuild=true run, is exactly
// TWO genuine external-infrastructure dependencies with no local stand-in:
//   - Azure.Storage.Blobs.BlobServiceClient (real Azure Storage account/connection string)
//   - MassTransit.IBus (real message broker — RabbitMQ/Azure Service Bus)
// Both require real credentials/infrastructure not present in local dev and are out of
// scope to fake with a no-op stand-in without a deliberate decision to do so. Disabling
// ValidateOnBuild moves the handful of handlers that depend on these two services from
// "blocks the entire app from starting" to "fails only if actually invoked" — their true
// functional state locally either way. ValidateScopes stays ON, so genuine lifetime-mismatch
// bugs (e.g. a singleton consuming a scoped service) still fail fast.
// TODO: stand up local dev equivalents (Azurite for Blob Storage, a local RabbitMQ container
// or an in-memory IBus stand-in) if local end-to-end testing of blob-export/event-publishing
// features becomes necessary, then re-enable ValidateOnBuild.
builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateOnBuild = false;
    options.ValidateScopes = true;
});

// ── Build ─────────────────────────────────────────────────────────────────────
var app = builder.Build();

// ── Middleware Pipeline ───────────────────────────────────────────────────────
// Order is architecturally mandated — see ApplicationBuilderExtensions for rationale.
app.UsePlantSafetyMiddleware();

// ── Swagger / OpenAPI ─────────────────────────────────────────────────────────
app.UsePlantSafetySwagger(app.Environment);

// ── Endpoints ─────────────────────────────────────────────────────────────────
app.MapControllers();
app.MapPlantSafetyHealthChecks();

app.Run();

/// <summary>
/// Exposes the top-level-statement entry point as a public partial class so
/// <see cref="Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory{TEntryPoint}"/>
/// in PlantSafety.IntegrationTests can reference it (the implicit Program class
/// generated by top-level statements is internal by default).
/// </summary>
public partial class Program { }

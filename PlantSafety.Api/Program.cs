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

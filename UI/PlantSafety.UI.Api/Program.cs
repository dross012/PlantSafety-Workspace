// ============================================================================
//  Project:        PlantSafety.UI.Api
//  Module:         Host
//  File:           Program.cs
//  Created:        2026-08-09
//  Description:    Composition root for PlantSafety.UI's backing API. Wires
//                  every real in-memory Shared.*Domain repository/service
//                  (Path B/C) plus Enterprise.DataLake/Enterprise.Graph's
//                  provider seam (Path D), mounts Enterprise.ApiGateway's and
//                  Enterprise.Dashboard's and Enterprise.Integration's
//                  ALREADY-BUILT controllers as ApplicationParts (reused
//                  as-is, not re-implemented), registers a Guid strong-id
//                  JSON converter per strong-id type used across request/
//                  response DTOs, and enables permissive local-dev CORS for
//                  the MAUI Blazor Hybrid WebView client (loopback origin
//                  varies per platform).
// ============================================================================
using Enterprise.ApiGateway.Controllers;
using Enterprise.DataLake.Infrastructure.DependencyInjection;
using Enterprise.Integration.Adapters;
using Enterprise.Integration.Events;
using Enterprise.Integration.Mapping;

using Shared.Deterministic.Abstractions;
using Shared.Deterministic.Abstractions.Clock;
using Shared.ErrorHandling.Abstractions.Policy;
using Shared.Telemetry.Abstractions.Correlation;
using Shared.Telemetry.Abstractions.Events;
using Shared.Configuration.Abstractions.Manifest;
using Shared.Configuration.Abstractions.Secrets;
using Shared.Configuration.Abstractions.Validation;
using Shared.ModuleContracts.Abstractions.Events;

using PlantSafety.UI.Api.Events;

using Shared.EmployeeDomain.Abstractions.ValueObjects;
using Shared.EmployeeDomain.Infrastructure.DependencyInjection;
using Shared.FacilityDomain.Abstractions.Hierarchy;
using Shared.FacilityDomain.Abstractions.ValueObjects;
using Shared.FacilityDomain.Infrastructure.DependencyInjection;
using Shared.MachineDomain.Abstractions.ValueObjects;
using Shared.MachineDomain.Infrastructure.DependencyInjection;
using Shared.SafetyData.Abstractions.Models.Industries.Orchestration;
using Shared.SafetyData.Infrastructure.DependencyInjection;
using Shared.SafetyData.Infrastructure.Industries;

using Enterprise.RulesEngine.Abstractions.Ids;
using Enterprise.WorkflowOrchestration.Abstractions.Ids;
using Enterprise.Graph.Abstractions.Ids;

using PlantSafety.UI.Api.Json;
using PlantSafety.UI.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

// ---- Deterministic runtime (Phase 2.1): IClock/IRandomProvider, real by default; set
//      "DeterministicMode:IsEnabled=true" (config/env) to pin a fixed clock + seeded
//      randomness for integration test runs. ----
builder.Services.AddDeterministicRuntime(options => builder.Configuration.GetSection("DeterministicMode").Bind(options));

// ---- Real Shared.*Domain services (Path B/C) ----
builder.Services.AddFacilityDomain();
builder.Services.AddEmployeeDomain();
builder.Services.AddMachineDomain();

// ---- Enterprise.DataLake (Path D.2) ----
builder.Services.AddEnterpriseDataLake();

// ---- Enterprise.Graph read seam (Path D.1/D.3) ----
builder.Services.AddSingleton<InMemoryEnterpriseGraphProvider>();
builder.Services.AddSingleton<IEnterpriseGraphProvider>(sp => sp.GetRequiredService<InMemoryEnterpriseGraphProvider>());

// ---- Shared.SafetyData multi-facility tenant registry (Path C.8) ----
builder.Services.AddSingleton<ITenantFacilityRegistryV2, TenantFacilityRegistryV2>();

// ---- Shared.SafetyData Facility Profile + Compliance & Risk, incl. severity-aware scoring (Path C.10) ----
builder.Services.AddFacilityProfiles();
builder.Services.AddComplianceRisk();

// ---- Enterprise.Integration adapters (Path D.9) -- one no-op-mapping adapter per system as a demo default ----
builder.Services.AddSingleton<IReadOnlyDictionary<EnterpriseIntegrationSystem, IEnterpriseIntegrationAdapter>>(_ =>
    Enum.GetValues<EnterpriseIntegrationSystem>().ToDictionary(
        system => system,
        system => (IEnterpriseIntegrationAdapter)new EnterpriseIntegrationAdapter(system, Array.Empty<EnterpriseIntegrationMapping>())));

// ---- Controllers: this assembly + the already-built Enterprise.ApiGateway/Dashboard/Integration controllers ----
builder.Services
    .AddControllers()
    .AddApplicationPart(typeof(EnterpriseApiGatewayController).Assembly)
    .AddApplicationPart(typeof(Enterprise.Dashboard.Controllers.EnterpriseDashboardController).Assembly)
    .AddApplicationPart(typeof(Enterprise.Integration.Controllers.EnterpriseIntegrationController).Assembly)
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new GuidStrongIdJsonConverter<FacilityId>(FacilityId.From, id => id.Value));
        options.JsonSerializerOptions.Converters.Add(new GuidStrongIdJsonConverter<BuildingId>(BuildingId.From, id => id.Value));
        options.JsonSerializerOptions.Converters.Add(new GuidStrongIdJsonConverter<AreaId>(AreaId.From, id => id.Value));
        options.JsonSerializerOptions.Converters.Add(new GuidStrongIdJsonConverter<ZoneId>(ZoneId.From, id => id.Value));
        options.JsonSerializerOptions.Converters.Add(new GuidStrongIdJsonConverter<EmployeeId>(EmployeeId.From, id => id.Value));
        options.JsonSerializerOptions.Converters.Add(new GuidStrongIdJsonConverter<MachineId>(MachineId.From, id => id.Value));
        options.JsonSerializerOptions.Converters.Add(new GuidStrongIdJsonConverter<EnterpriseRuleId>(EnterpriseRuleId.From, id => id.Value));
        options.JsonSerializerOptions.Converters.Add(new GuidStrongIdJsonConverter<EnterpriseWorkflowId>(EnterpriseWorkflowId.From, id => id.Value));
        options.JsonSerializerOptions.Converters.Add(new GuidStrongIdJsonConverter<EnterpriseNodeId>(EnterpriseNodeId.From, id => id.Value));
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

builder.Services.AddOpenApi();

// ---- Local-dev CORS for the MAUI Blazor Hybrid WebView client (Section 10: UI services call this API by typed HttpClient) ----
const string uiCorsPolicy = "PlantSafetyUiLocalDev";
builder.Services.AddCors(options => options.AddPolicy(uiCorsPolicy, policy =>
    policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

// ---- Phase 2.3: deterministic error envelope + error policy engine, singleton so the
//      circuit breaker's state is shared across requests, not reset per-request. ----
builder.Services.AddSingleton(new CircuitBreaker(CircuitBreakerPolicy.Standard, new SystemClock()));
builder.Services.AddSingleton<IErrorPolicy>(sp => new StandardErrorPolicy(ErrorRetryPolicy.Standard, sp.GetRequiredService<CircuitBreaker>()));

// ---- Phase 2.4: structured, JSON-formatted logging + per-request correlation context. ----
builder.Logging.AddJsonConsole();
builder.Services.AddScoped<CorrelationContext>();
builder.Services.AddScoped<ICorrelationContext>(sp => sp.GetRequiredService<CorrelationContext>());
builder.Services.AddSingleton<ILogEventWriter, LogEventWriter>();

// ---- Phase 2.5: environment manifest, loaded and validated once at startup -- the host
//      refuses to start with invalid configuration rather than failing unpredictably at
//      first use. RuntimeLimits are applied to Kestrel/the request-timeout middleware
//      below; EnableOpenApi replaces the old app.Environment.IsDevelopment() branch, so
//      OpenAPI availability is environment-manifest-driven, not code-branched. ----
EnvironmentManifest environmentManifest = EnvironmentManifestLoader.Load(builder.Configuration);
IReadOnlyList<string> environmentManifestErrors = EnvironmentManifestValidator.Validate(environmentManifest);
if (environmentManifestErrors.Count > 0)
{
    throw new ConfigurationValidationException(environmentManifestErrors);
}

builder.Services.AddSingleton(environmentManifest);
builder.Services.AddRequestTimeouts(options => options.DefaultPolicy = new Microsoft.AspNetCore.Http.Timeouts.RequestTimeoutPolicy { Timeout = environmentManifest.RuntimeLimits.RequestTimeout });
builder.WebHost.ConfigureKestrel(kestrel => kestrel.Limits.MaxRequestBodySize = environmentManifest.RuntimeLimits.MaxRequestBodySizeBytes);

// ---- Phase 2.7: module boundary event gateway. Singleton -- subscriptions live for the
//      process lifetime. FacilitiesController publishes FacilityCreatedEvent; the
//      subscriber below (a different "module" from the publisher's point of view) reacts
//      by logging a structured event, without FacilitiesController referencing it at all. ----
builder.Services.AddSingleton<IModuleEventGateway, ModuleEventGateway>();

var app = builder.Build();

// ---- Phase 3.2: log that configuration loaded successfully, without ever touching
//      manifest.Secrets/ConnectionStrings values directly -- RedactedEnvironmentManifest
//      carries key names and enabled-flag names only. ----
{
    RedactedEnvironmentManifest redacted = SecretRedactor.Redact(environmentManifest);
    IClock startupClock = app.Services.GetRequiredService<IClock>();
    app.Services.GetRequiredService<ILogEventWriter>().Write(new LogEvent(
        EventId: "ENVIRONMENT_MANIFEST_LOADED",
        Category: LogCategory.Infrastructure,
        Severity: LogSeverity.Info,
        Message: $"Environment manifest loaded for \"{redacted.EnvironmentName}\": {redacted.EnabledFeatureFlags.Count} feature flag(s) enabled, {redacted.ConnectionStringKeys.Count} connection string(s), {redacted.SecretKeys.Count} secret(s) configured.",
        Payload: new Dictionary<string, object?>
        {
            ["EnabledFeatureFlags"] = redacted.EnabledFeatureFlags,
            ["ConnectionStringKeys"] = redacted.ConnectionStringKeys,
            ["SecretKeys"] = redacted.SecretKeys,
        },
        CorrelationId: "startup",
        Module: "Configuration",
        OccurredAtUtc: startupClock.UtcNow));
}

if (environmentManifest.IsFeatureEnabled("EnableOpenApi"))
{
    app.MapOpenApi();
}

app.Services.GetRequiredService<IModuleEventGateway>().Subscribe<FacilityCreatedEvent>((evt, _) =>
{
    app.Services.GetRequiredService<ILogEventWriter>().Write(new LogEvent(
        EventId: "FACILITY_CREATED",
        Category: LogCategory.Facility,
        Severity: LogSeverity.Info,
        Message: $"Facility \"{evt.Name}\" ({evt.FacilityId}) created.",
        Payload: new Dictionary<string, object?> { ["FacilityId"] = evt.FacilityId, ["Name"] = evt.Name },
        // This subscriber is wired once at startup (a singleton), outside any one
        // request's scope, so it has no ICorrelationContext to read from -- the event's
        // own FacilityId is the natural correlating key for this event-driven flow.
        CorrelationId: evt.FacilityId.ToString(),
        Module: "Facility",
        OccurredAtUtc: evt.OccurredAtUtc));
    return Task.CompletedTask;
});

app.UseRequestTimeouts();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseExceptionHandler(exceptionHandlerApp => exceptionHandlerApp.Run(context =>
    GlobalErrorHandler.HandleAsync(
        context,
        context.RequestServices.GetRequiredService<IClock>(),
        context.RequestServices.GetRequiredService<IErrorPolicy>(),
        context.RequestServices.GetRequiredService<CircuitBreaker>(),
        context.RequestServices.GetRequiredService<ILogEventWriter>())));
app.UseCors(uiCorsPolicy);
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

/// <summary>Marker partial for WebApplicationFactory-based integration test hosting.</summary>
public partial class Program;

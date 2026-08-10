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

using Shared.Authorization.Abstractions.Caller;
using Shared.Authorization.Abstractions.Identity;
using Shared.Authorization.Abstractions.Permissions;
using Shared.Authorization.Abstractions.Roles;
using Shared.Authorization.Abstractions.Tenant;
using Shared.Authorization.Infrastructure.Options;
using Shared.Deterministic.Abstractions;
using Shared.Deterministic.Abstractions.Clock;
using Shared.ErrorHandling.Abstractions.Policy;
using Shared.Telemetry.Abstractions.Correlation;
using Shared.Telemetry.Abstractions.Events;
using Shared.Configuration.Abstractions.Manifest;
using Shared.Configuration.Abstractions.Secrets;
using Shared.Configuration.Abstractions.Validation;
using Shared.ModuleContracts.Abstractions.Events;

using Shared.FacilityDomain.Infrastructure.Events;

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
using PlantSafety.UI.Api.Tenant;

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

// ---- Phase 3.5: enforce RuntimeLimits.MaxConcurrentRequests, defined since Phase 2.5 but
//      never actually applied anywhere until now. A global concurrency limiter (not a
//      per-client rate limiter -- there's no auth/client identity yet, see Phase 3.9's
//      flag) that rejects with 429 once MaxConcurrentRequests in-flight requests are
//      already being handled, rather than letting unbounded concurrency exhaust the host. ----
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = System.Threading.RateLimiting.PartitionedRateLimiter.Create<HttpContext, string>(_ =>
        System.Threading.RateLimiting.RateLimitPartition.GetConcurrencyLimiter("global", _ =>
            new System.Threading.RateLimiting.ConcurrencyLimiterOptions
            {
                PermitLimit = environmentManifest.RuntimeLimits.MaxConcurrentRequests,
                QueueLimit = 0,
            }));
});

// ---- Phase 2.7: module boundary event gateway. Singleton -- subscriptions live for the
//      process lifetime. FacilityService (Phase 4: moved out of FacilitiesController, since
//      publishing a domain event is handler-level orchestration, not controller logic)
//      publishes FacilityCreatedEvent; the subscriber below (a different "module" from the
//      publisher's point of view) reacts by logging a structured event, without
//      FacilityService referencing it at all. ----
builder.Services.AddSingleton<IModuleEventGateway, ModuleEventGateway>();

// ---- Phase 9: Shared.Authorization's deterministic permission resolver. Stateless, pure --
//      registered as a singleton like every other stateless resolver in this host. No
//      authentication scheme is registered (see WorkflowEngineController.cs's own header
//      comment) -- this only makes the real, existing role/permission model resolvable and
//      observable; it does not itself enforce anything at the API boundary. ----
builder.Services.AddSingleton<IPermissionResolver, EnterprisePermissionResolver>();

// ---- Phase 10: deterministic, header-based API-key authentication. The registered-key map is
//      bound once from configuration ("ApiKeyAuthentication:RegisteredKeys") and defaults to
//      empty -- unless an operator actually configures keys, this reproduces today's exact
//      behavior (every request resolves to CallerContext.SystemDefault). CallerContextProvider
//      is request-scoped (Set once by ApiKeyAuthenticationMiddleware, read by controllers),
//      mirroring CorrelationContext's own scoped registration above. No authentication scheme,
//      no [Authorize]/UseAuthorization() reliance -- see ApiKeyAuthenticationMiddleware.cs's own
//      header comment for why. ----
var apiKeyAuthenticationOptions = new ApiKeyAuthenticationOptions();
builder.Configuration.GetSection(ApiKeyAuthenticationOptions.SectionName).Bind(apiKeyAuthenticationOptions);
builder.Services.AddSingleton<IReadOnlyDictionary<string, EnterpriseRole>>(apiKeyAuthenticationOptions.ToRoleMap());
builder.Services.AddScoped<CallerContextProvider>();
builder.Services.AddScoped<ICallerContextProvider>(sp => sp.GetRequiredService<CallerContextProvider>());

// ---- Phase 11: multi-tenant/contextual access. The key -> TenantId map is bound independently
//      of Phase 10's key -> EnterpriseRole map (additive, defaults to empty -- no live caller
//      sends a tenant-bound key today, so this reproduces pre-Phase-11 behavior: every request
//      resolves to TenantContext.Unscoped). TenantFacilityAccessResolver adapts the real,
//      already-registered ITenantFacilityRegistryV2 (Path C.8) to the seam TenantIsolationCheck
//      uses, without Shared.Authorization depending on Shared.SafetyData -- see
//      ITenantFacilityAccessResolver.cs's own header comment. ----
var tenantAuthenticationOptions = new TenantAuthenticationOptions();
builder.Configuration.GetSection(TenantAuthenticationOptions.SectionName).Bind(tenantAuthenticationOptions);
builder.Services.AddSingleton<IReadOnlyDictionary<string, string>>(tenantAuthenticationOptions.RegisteredKeyTenants);
builder.Services.AddSingleton<ITenantFacilityAccessResolver, TenantFacilityAccessResolver>();
builder.Services.AddScoped<TenantContextProvider>();
builder.Services.AddScoped<ITenantContextProvider>(sp => sp.GetRequiredService<TenantContextProvider>());

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

// ---- Phase 3.9: HSTS outside Development, environment-manifest-driven (Phase 2.5
//      convention) rather than the default app.Environment.IsDevelopment() branch. ----
if (environmentManifest.EnvironmentName != "Development")
{
    app.UseHsts();
}

app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseRequestTimeouts();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ApiKeyAuthenticationMiddleware>();
app.UseExceptionHandler(exceptionHandlerApp => exceptionHandlerApp.Run(context =>
    GlobalErrorHandler.HandleAsync(
        context,
        context.RequestServices.GetRequiredService<IClock>(),
        context.RequestServices.GetRequiredService<IErrorPolicy>(),
        context.RequestServices.GetRequiredService<CircuitBreaker>(),
        context.RequestServices.GetRequiredService<ILogEventWriter>())));
app.UseCors(uiCorsPolicy);
app.UseHttpsRedirection();
app.UseRateLimiter();
app.UseAuthorization();
app.MapControllers();

app.Run();

/// <summary>Marker partial for WebApplicationFactory-based integration test hosting.</summary>
public partial class Program;

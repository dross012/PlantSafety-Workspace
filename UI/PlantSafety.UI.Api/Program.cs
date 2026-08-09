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

using Shared.EmployeeDomain.Abstractions.ValueObjects;
using Shared.EmployeeDomain.Infrastructure.DependencyInjection;
using Shared.FacilityDomain.Abstractions.Hierarchy;
using Shared.FacilityDomain.Abstractions.ValueObjects;
using Shared.FacilityDomain.Infrastructure.DependencyInjection;
using Shared.MachineDomain.Abstractions.ValueObjects;
using Shared.MachineDomain.Infrastructure.DependencyInjection;
using Shared.SafetyData.Abstractions.Models.Industries.Orchestration;
using Shared.SafetyData.Infrastructure.Industries;

using Enterprise.RulesEngine.Abstractions.Ids;
using Enterprise.WorkflowOrchestration.Abstractions.Ids;
using Enterprise.Graph.Abstractions.Ids;

using PlantSafety.UI.Api.Json;

var builder = WebApplication.CreateBuilder(args);

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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors(uiCorsPolicy);
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

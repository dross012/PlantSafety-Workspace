// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.MachineSafety.CraneHoistSystems
//  Layer:          IntegrationTests
//  File:           CraneHoistSystemsApiTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    API integration tests for the CraneHoistSystems endpoints on
//                  MachineSafetyV1Controller, verifying the authentication/authorization gate
//                  (every endpoint requires a valid bearer token) matches the sibling module
//                  pattern (see PlantSafety.IntegrationTests.Financials.Api /
//                  PlantSafety.IntegrationTests.PlanningProjects.Api).
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.MachineSafety.CraneHoistSystems.Api;

[Collection(nameof(IntegrationTestCollection))]
public sealed class CraneHoistSystemsApiTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GetCraneHoistSystems_WithoutAuth_ShouldReturn401()
    {
        var response = await Client.GetAsync("/api/v1/machinesafety/crane-hoist-systems");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCraneHoistAssetById_WithoutAuth_ShouldReturn401()
    {
        var response = await Client.GetAsync($"/api/v1/machinesafety/crane-hoist-systems/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ListLiftEventsByAsset_WithoutAuth_ShouldReturn401()
    {
        var response = await Client.GetAsync($"/api/v1/machinesafety/crane-hoist-systems/{Guid.NewGuid()}/lift-events");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetInspectionHistory_WithoutAuth_ShouldReturn401()
    {
        var response = await Client.GetAsync($"/api/v1/machinesafety/crane-hoist-systems/{Guid.NewGuid()}/inspection-history");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ListAssetsDueForInspection_WithoutAuth_ShouldReturn401()
    {
        var response = await Client.GetAsync("/api/v1/machinesafety/crane-hoist-systems/due-for-inspection");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ListExpiringOperatorQualifications_WithoutAuth_ShouldReturn401()
    {
        var response = await Client.GetAsync("/api/v1/machinesafety/crane-hoist-systems/expiring-operator-qualifications");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RegisterCraneHoistAsset_WithoutAuth_ShouldReturn401()
    {
        var response = await Client.PostAsJsonAsync("/api/v1/machinesafety/crane-hoist-systems", new
        {
            assetTag = "API-1",
            assetTitle = "Api Test Crane",
            assetType = "Hoist",
            ratedLoadCapacityKg = 1000m,
        });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RecordLiftEvent_WithoutAuth_ShouldReturn401()
    {
        var response = await Client.PostAsJsonAsync($"/api/v1/machinesafety/crane-hoist-systems/{Guid.NewGuid()}/record-lift-event", new
        {
            observedLoadKg = 500m,
        });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RecordOverloadEvent_WithoutAuth_ShouldReturn401()
    {
        var response = await Client.PostAsJsonAsync($"/api/v1/machinesafety/crane-hoist-systems/{Guid.NewGuid()}/record-overload-event", new
        {
            observedLoadKg = 6000m,
        });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ScheduleCraneInspection_WithoutAuth_ShouldReturn401()
    {
        var response = await Client.PostAsJsonAsync($"/api/v1/machinesafety/crane-hoist-systems/{Guid.NewGuid()}/schedule-inspection", new
        {
            dueAtUtc = FixedUtcNow.AddDays(30),
        });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CompleteCraneInspection_WithoutAuth_ShouldReturn401()
    {
        var response = await Client.PostAsJsonAsync($"/api/v1/machinesafety/crane-hoist-systems/{Guid.NewGuid()}/complete-inspection", new
        {
            passed = true,
        });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task QualifyOperator_WithoutAuth_ShouldReturn401()
    {
        var response = await Client.PostAsJsonAsync($"/api/v1/machinesafety/crane-hoist-systems/{Guid.NewGuid()}/qualify-operator", new
        {
            operatorUserId = Guid.NewGuid(),
        });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SuspendOperatorQualification_WithoutAuth_ShouldReturn401()
    {
        var response = await Client.PostAsJsonAsync($"/api/v1/machinesafety/crane-hoist-systems/{Guid.NewGuid()}/suspend-operator-qualification", new
        {
            reason = "audit finding",
        });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RecordRiggingInspection_WithoutAuth_ShouldReturn401()
    {
        var response = await Client.PostAsJsonAsync($"/api/v1/machinesafety/crane-hoist-systems/{Guid.NewGuid()}/record-rigging-inspection", new
        {
            defectDetected = false,
        });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}

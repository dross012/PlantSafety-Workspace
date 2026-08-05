// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.AutonomousVehicleSafety
//  Layer:          IntegrationTests
//  File:           AutonomousVehicleSafetyApiTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Testing Agent)
//  SchemaVersion:  2026.1
//  Description:    API/controller integration tests for AutonomousVehicleSafetyV1Controller.
//                  Confirms every command/query endpoint enforces authentication (401) when no
//                  Bearer token is presented, mirroring the PlanningProjectsApiTests precedent.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.AutonomousVehicleSafety.Api;

[Collection(nameof(IntegrationTestCollection))]
public sealed class AutonomousVehicleSafetyApiTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task RegisterAutonomousVehicleAsset_WithoutAuth_ShouldReturn401()
    {
        var response = await Client.PostAsJsonAsync("/api/v1/autonomousvehiclesafety/assets/register",
            new { AssetTag = "AMR-001", VehicleType = "Amr", AutonomyLevel = "SemiAutonomous" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RecordAutonomyModeTransition_WithoutAuth_ShouldReturn401()
    {
        var response = await Client.PostAsJsonAsync(
            $"/api/v1/autonomousvehiclesafety/assets/{Guid.NewGuid()}/record-mode-transition",
            new { ToMode = "Autonomous" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SuspendAutonomousOperation_WithoutAuth_ShouldReturn401()
    {
        var response = await Client.PostAsJsonAsync(
            $"/api/v1/autonomousvehiclesafety/assets/{Guid.NewGuid()}/suspend",
            new { Reason = "Repeated geofence violations" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAutonomousVehicleAssetById_WithoutAuth_ShouldReturn401()
    {
        var response = await Client.GetAsync($"/api/v1/autonomousvehiclesafety/assets/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DefineOperatingGeofence_WithoutAuth_ShouldReturn401()
    {
        var response = await Client.PostAsJsonAsync("/api/v1/autonomousvehiclesafety/geofences/define",
            new { GeofenceName = "North Yard Exclusion", ZoneType = "ExclusionZone", BoundaryDefinition = "POLYGON((0 0,1 0,1 1,0 1,0 0))" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ListActiveGeofenceViolations_WithoutAuth_ShouldReturn401()
    {
        var response = await Client.GetAsync(
            $"/api/v1/autonomousvehiclesafety/geofences/active-violations?From={FixedUtcNow.AddDays(-7):o}&To={FixedUtcNow:o}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ValidateSafetyCase_WithoutAuth_ShouldReturn401()
    {
        var response = await Client.PostAsJsonAsync("/api/v1/autonomousvehiclesafety/safety-cases/validate",
            new { AutonomousVehicleAssetId = Guid.NewGuid(), StandardReference = "AnsiRia1508", DocumentVersion = "v1.0", RevalidationCadenceDays = 180, ValidatedByUserId = Guid.NewGuid() });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetSafetyCaseStatus_WithoutAuth_ShouldReturn401()
    {
        var response = await Client.GetAsync($"/api/v1/autonomousvehiclesafety/safety-cases/status/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RecordHumanRobotInteractionEvent_WithoutAuth_ShouldReturn401()
    {
        var response = await Client.PostAsJsonAsync("/api/v1/autonomousvehiclesafety/human-robot-interactions/record",
            new { AutonomousVehicleAssetId = Guid.NewGuid(), InteractionType = "NearMiss", SeverityLevel = "High", LocationDescription = "Loading dock 3" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ListHumanRobotInteractionEvents_WithoutAuth_ShouldReturn401()
    {
        var response = await Client.GetAsync(
            $"/api/v1/autonomousvehiclesafety/human-robot-interactions?From={FixedUtcNow.AddDays(-7):o}&To={FixedUtcNow:o}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RecordAutonomousVehicleIncident_WithoutAuth_ShouldReturn401()
    {
        var response = await Client.PostAsJsonAsync("/api/v1/autonomousvehiclesafety/incidents/record",
            new { AutonomousVehicleAssetId = Guid.NewGuid(), IncidentType = "GeofenceBreach", SeverityLevel = "High", DescriptionSummary = "Vehicle breached the north exclusion zone" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetIncidentHistoryByAsset_WithoutAuth_ShouldReturn401()
    {
        var response = await Client.GetAsync($"/api/v1/autonomousvehiclesafety/incidents/history/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}

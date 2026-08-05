// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.PublicSafetyCommunication
//  Layer:          IntegrationTests
//  File:           PublicSafetyCommunicationApiTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Testing Agent)
//  SchemaVersion:  2026.1
//  Description:    API/controller integration tests for PublicSafetyCommunicationV1Controller.
//                  Confirms every command/query endpoint enforces authentication (401) when no
//                  Bearer token is presented, mirroring the CommunitySafetyApiTests/
//                  AutonomousVehicleSafetyApiTests precedent. Deterministic, parallel-safe,
//                  additive-only.
// ============================================================================
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.PublicSafetyCommunication.Api;

[Collection(nameof(IntegrationTestCollection))]
public sealed class PublicSafetyCommunicationApiTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    // ── Agency Contact Directory ─────────────────────────────────────────────

    [Fact]
    public async Task RegisterPublicSafetyAgency_WithoutAuth_ShouldReturn401()
    {
        var response = await Client.PostAsJsonAsync("/api/v1/publicsafetycommunication/agencies",
            new { AgencyName = "County Fire Department", AgencyType = "Fire", JurisdictionDescription = "County district 4", PrimaryPhoneNumber = "555-0100" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ListPublicSafetyAgencies_WithoutAuth_ShouldReturn401()
    {
        var response = await Client.GetAsync("/api/v1/publicsafetycommunication/agencies");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── Incident Notification Dispatch ───────────────────────────────────────

    [Fact]
    public async Task DispatchIncidentNotification_WithoutAuth_ShouldReturn401()
    {
        var response = await Client.PostAsJsonAsync("/api/v1/publicsafetycommunication/notifications",
            new { PublicSafetyAgencyId = Guid.NewGuid(), DeliveryChannelCode = "Phone", Message = "Ammonia release -- HazMat requested." });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AcknowledgeIncidentNotification_WithoutAuth_ShouldReturn401()
    {
        var response = await Client.PostAsJsonAsync(
            $"/api/v1/publicsafetycommunication/notifications/{Guid.NewGuid()}/acknowledge",
            new { AcknowledgedByContactName = "County Dispatch Desk 4" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CloseIncidentNotification_WithoutAuth_ShouldReturn401()
    {
        var response = await Client.PostAsJsonAsync(
            $"/api/v1/publicsafetycommunication/notifications/{Guid.NewGuid()}/close", new { });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetIncidentNotificationStatus_WithoutAuth_ShouldReturn401()
    {
        var response = await Client.GetAsync(
            $"/api/v1/publicsafetycommunication/notifications?From={FixedUtcNow.AddDays(-90):o}&To={FixedUtcNow.AddDays(90):o}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── Public Alert Broadcast ────────────────────────────────────────────────

    [Fact]
    public async Task BroadcastPublicAlert_WithoutAuth_ShouldReturn401()
    {
        var response = await Client.PostAsJsonAsync("/api/v1/publicsafetycommunication/broadcasts",
            new { SeverityCode = "Warning", ChannelCode = "Ipaws", Headline = "Shelter in place", Message = "A chemical release has occurred.", TargetArea = "1-mile radius" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ListActiveBroadcasts_WithoutAuth_ShouldReturn401()
    {
        var response = await Client.GetAsync("/api/v1/publicsafetycommunication/broadcasts/active");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── Communication Log & After-Action ─────────────────────────────────────

    [Fact]
    public async Task LogCommunicationEntry_WithoutAuth_ShouldReturn401()
    {
        var response = await Client.PostAsJsonAsync("/api/v1/publicsafetycommunication/communication-log",
            new { Direction = "Outbound", Medium = "Phone", CounterpartyName = "County Fire Dispatch", Summary = "Notified dispatch.", OccurredAtUtc = FixedUtcNow });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCommunicationLogByIncident_WithoutAuth_ShouldReturn401()
    {
        var response = await Client.GetAsync($"/api/v1/publicsafetycommunication/communication-log/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}

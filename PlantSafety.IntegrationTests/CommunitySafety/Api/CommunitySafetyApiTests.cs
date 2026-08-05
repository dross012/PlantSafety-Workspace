// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.CommunitySafety
//  Layer:          IntegrationTests
//  File:           CommunitySafetyApiTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Testing Agent)
//  SchemaVersion:  2026.1
//  Description:    API/controller integration tests for CommunitySafetyV1Controller. Confirms
//                  every command/query endpoint enforces authentication (401) when no Bearer
//                  token is presented, mirroring the AutonomousVehicleSafetyApiTests precedent.
//                  Given CommunityContact carries PII subject to the data-governance
//                  access-restriction mandate, its endpoints receive explicit unauthenticated
//                  coverage alongside every other sub-component. Deterministic, parallel-safe,
//                  additive-only.
// ============================================================================
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.CommunitySafety.Api;

[Collection(nameof(IntegrationTestCollection))]
public sealed class CommunitySafetyApiTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    // ── CommunityContact ─────────────────────────────────────────────────────────

    [Fact]
    public async Task RegisterCommunityContact_WithoutAuth_ShouldReturn401()
    {
        var response = await Client.PostAsJsonAsync("/api/v1/communitysafety/community-contacts",
            new { DisplayName = "Fire Chief - Station 4", ContactType = "EmergencyServices", CommunityArea = "North Ridge", Email = "chief@example.org", OptedInForNotifications = true });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCommunityContacts_WithoutAuth_ShouldReturn401()
    {
        var response = await Client.GetAsync(
            $"/api/v1/communitysafety/community-contacts?From={FixedUtcNow.AddDays(-7):o}&To={FixedUtcNow:o}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── CommunityConcern ─────────────────────────────────────────────────────────

    [Fact]
    public async Task LogCommunityConcern_WithoutAuth_ShouldReturn401()
    {
        var response = await Client.PostAsJsonAsync("/api/v1/communitysafety/community-concerns",
            new { SiteId = Guid.NewGuid(), Category = "Odor", Severity = "Medium", FacilityArea = "Tank Farm", Description = "desc" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ResolveCommunityConcern_WithoutAuth_ShouldReturn401()
    {
        var response = await Client.PostAsJsonAsync(
            $"/api/v1/communitysafety/community-concerns/{Guid.NewGuid()}/resolve",
            new { ResolutionNotes = "Root cause corrected." });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ListOpenCommunityConcerns_WithoutAuth_ShouldReturn401()
    {
        var response = await Client.GetAsync("/api/v1/communitysafety/community-concerns/open");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCommunityConcernById_WithoutAuth_ShouldReturn401()
    {
        var response = await Client.GetAsync($"/api/v1/communitysafety/community-concerns/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── CommunityImpactAssessment ────────────────────────────────────────────────

    [Fact]
    public async Task RecordCommunityImpactAssessment_WithoutAuth_ShouldReturn401()
    {
        var response = await Client.PostAsJsonAsync("/api/v1/communitysafety/community-impact-assessments",
            new { SiteId = Guid.NewGuid(), IncidentId = Guid.NewGuid(), Severity = "Severe", AffectedArea = "North Ridge", Description = "desc", AssessedByUserId = Guid.NewGuid() });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetImpactAssessmentsByIncident_WithoutAuth_ShouldReturn401()
    {
        var response = await Client.GetAsync($"/api/v1/communitysafety/community-impact-assessments/by-incident/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── CommunityNotification ────────────────────────────────────────────────────

    [Fact]
    public async Task SendCommunityNotification_WithoutAuth_ShouldReturn401()
    {
        var response = await Client.PostAsJsonAsync("/api/v1/communitysafety/community-notifications",
            new { Channel = "Email", RecipientContactId = Guid.NewGuid(), Subject = "Notice", MessageBody = "body" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ListNotificationsByPeriod_WithoutAuth_ShouldReturn401()
    {
        var response = await Client.GetAsync(
            $"/api/v1/communitysafety/community-notifications?From={FixedUtcNow.AddDays(-7):o}&To={FixedUtcNow:o}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── CommunityMeetingRecord ───────────────────────────────────────────────────

    [Fact]
    public async Task RecordCommunityMeeting_WithoutAuth_ShouldReturn401()
    {
        var response = await Client.PostAsJsonAsync("/api/v1/communitysafety/community-meetings",
            new { SiteId = Guid.NewGuid(), MeetingTitle = "Q3 Community Advisory Panel", MeetingDateUtc = FixedUtcNow, Location = "Community Center", AttendeeCount = 12, TopicsSummary = "topics", RecordedByUserId = Guid.NewGuid() });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCommunityMeetingRecords_WithoutAuth_ShouldReturn401()
    {
        var response = await Client.GetAsync(
            $"/api/v1/communitysafety/community-meetings?From={FixedUtcNow.AddDays(-30):o}&To={FixedUtcNow:o}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}

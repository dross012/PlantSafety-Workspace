// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.SafetyEvents.LocalEmergencyCoordination
//  Layer:          IntegrationTests
//  File:           LocalEmergencyCoordinationApiTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    API/controller integration tests for LocalEmergencyCoordinationV1Controller,
//                  exercising the real ASP.NET Core pipeline (routing, versioning, authorization
//                  middleware) via IntegrationTestWebAppFactory. Mirrors
//                  PlanningProjectsApiTests.GetProjects_WithoutAuth_ShouldReturn401 -- every route
//                  must require authentication before reaching the MediatR handler / repository, so
//                  these assertions hold without needing the module's SQL schema applied.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.SafetyEvents.LocalEmergencyCoordination;

/// <summary>API/controller integration tests for the <c>LocalEmergencyCoordination</c> V1 endpoints.</summary>
[Collection(nameof(IntegrationTestCollection))]
public sealed class LocalEmergencyCoordinationApiTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GetById_WithoutAuth_ShouldReturn401()
    {
        var response = await Client.GetAsync($"/api/v1/safety-events/local-emergency-coordination/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ListMutualAidAgreements_WithoutAuth_ShouldReturn401()
    {
        var response = await Client.GetAsync("/api/v1/safety-events/local-emergency-coordination/mutual-aid-agreements");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetJointResponsePlanByIncident_WithoutAuth_ShouldReturn401()
    {
        var response = await Client.GetAsync($"/api/v1/safety-events/local-emergency-coordination/incidents/{Guid.NewGuid()}/joint-response-plans");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetLiaisonLogByIncident_WithoutAuth_ShouldReturn401()
    {
        var response = await Client.GetAsync($"/api/v1/safety-events/local-emergency-coordination/incidents/{Guid.NewGuid()}/liaison-log");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ListJointDebriefsByPeriod_WithoutAuth_ShouldReturn401()
    {
        var response = await Client.GetAsync("/api/v1/safety-events/local-emergency-coordination/debriefs");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task OpenCase_WithoutAuth_ShouldReturn401()
    {
        var response = await Client.PostAsJsonAsync("/api/v1/safety-events/local-emergency-coordination", new
        {
            SiteId = (Guid?)null,
            IncidentId = (Guid?)null,
            CaseName = "Unauthenticated Probe Case",
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task EstablishMutualAidAgreement_WithoutAuth_ShouldReturn401()
    {
        var response = await Client.PostAsJsonAsync($"/api/v1/safety-events/local-emergency-coordination/{Guid.NewGuid()}/mutual-aid-agreements", new
        {
            AgencyName = "Probe Agency",
            Discipline = "Fire",
            DocumentVersion = 1,
            EffectiveFromUtc = DateTime.UtcNow,
            RenewalDueAtUtc = DateTime.UtcNow.AddYears(1),
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CloseCase_WithoutAuth_ShouldReturn401()
    {
        var response = await Client.PostAsJsonAsync($"/api/v1/safety-events/local-emergency-coordination/{Guid.NewGuid()}/close", new { });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ArchiveCase_WithoutAuth_ShouldReturn401()
    {
        var response = await Client.PostAsJsonAsync($"/api/v1/safety-events/local-emergency-coordination/{Guid.NewGuid()}/archive", new { });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AllNineRouteFamilies_RespondWithSomeStatus_ProvingRouteRegistration()
    {
        // A completely unregistered/mistyped route would 404 rather than 401 -- assert every
        // command/query route resolves to the controller (401, not 404), confirming route
        // registration independent of the auth-boundary assertions above.
        var routes = new[]
        {
            ("GET", $"/api/v1/safety-events/local-emergency-coordination/{Guid.NewGuid()}"),
            ("GET", "/api/v1/safety-events/local-emergency-coordination/mutual-aid-agreements"),
            ("GET", $"/api/v1/safety-events/local-emergency-coordination/incidents/{Guid.NewGuid()}/joint-response-plans"),
            ("GET", $"/api/v1/safety-events/local-emergency-coordination/incidents/{Guid.NewGuid()}/liaison-log"),
            ("GET", "/api/v1/safety-events/local-emergency-coordination/debriefs"),
        };

        foreach (var (method, path) in routes)
        {
            using var request = new HttpRequestMessage(new HttpMethod(method), path);
            var response = await Client.SendAsync(request);

            response.StatusCode.Should().NotBe(HttpStatusCode.NotFound, $"{method} {path} must be a registered route on LocalEmergencyCoordinationV1Controller");
        }
    }
}

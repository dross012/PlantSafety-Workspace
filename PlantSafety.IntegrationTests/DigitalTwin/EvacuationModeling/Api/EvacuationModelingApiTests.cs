// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.DigitalTwin.EvacuationModeling.Api
//  Layer:          IntegrationTests
//  File:           EvacuationModelingApiTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    API/controller integration tests for EvacuationModelingV1Controller, run
//                  against the real ASP.NET Core pipeline (real routing, real authorization
//                  middleware) via the shared IntegrationTestWebAppFactory. Verifies every write
//                  and read endpoint under DigitalTwin.EvacuationModeling requires authentication
//                  and is reachable via its documented route -- exercising the real
//                  AuthorizationPolicyExtensions/SafetyTierPolicies wiring end-to-end (not just the
//                  reflection-based assertions in PlantSafety.Tests' SecurityTests.cs). Does not
//                  require any DigitalTwin.EvacuationModeling-specific SQL schema, since every
//                  assertion here resolves before the request ever reaches a repository (401
//                  Unauthorized is returned by the authentication/authorization middleware).
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.DigitalTwin.EvacuationModeling.Api;

/// <summary>
/// Integration tests verifying the DigitalTwin.EvacuationModeling API endpoints are reachable
/// through the real ASP.NET Core routing/authorization pipeline and correctly reject unauthenticated
/// requests.
/// </summary>
[Collection(nameof(IntegrationTestCollection))]
public sealed class EvacuationModelingApiTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Theory]
    [InlineData("GET", "/api/v1/digitaltwin/evacuation-modeling/routes/00000000-0000-0000-0000-000000000001")]
    [InlineData("GET", "/api/v1/digitaltwin/evacuation-modeling/zones/00000000-0000-0000-0000-000000000001/routes")]
    [InlineData("GET", "/api/v1/digitaltwin/evacuation-modeling/routes/active-obstructions")]
    [InlineData("GET", "/api/v1/digitaltwin/evacuation-modeling/zones/00000000-0000-0000-0000-000000000001/occupancy")]
    [InlineData("GET", "/api/v1/digitaltwin/evacuation-modeling/zones/00000000-0000-0000-0000-000000000001/simulations/latest")]
    [InlineData("GET", "/api/v1/digitaltwin/evacuation-modeling/bottlenecks")]
    [InlineData("POST", "/api/v1/digitaltwin/evacuation-modeling/routes")]
    [InlineData("POST", "/api/v1/digitaltwin/evacuation-modeling/routes/00000000-0000-0000-0000-000000000001/retire")]
    [InlineData("POST", "/api/v1/digitaltwin/evacuation-modeling/routes/00000000-0000-0000-0000-000000000001/record-obstruction")]
    [InlineData("POST", "/api/v1/digitaltwin/evacuation-modeling/routes/00000000-0000-0000-0000-000000000001/clear-obstruction")]
    [InlineData("POST", "/api/v1/digitaltwin/evacuation-modeling/zones")]
    [InlineData("POST", "/api/v1/digitaltwin/evacuation-modeling/zones/00000000-0000-0000-0000-000000000001/record-occupancy-snapshot")]
    [InlineData("POST", "/api/v1/digitaltwin/evacuation-modeling/simulations")]
    [InlineData("POST", "/api/v1/digitaltwin/evacuation-modeling/simulations/00000000-0000-0000-0000-000000000001/link-to-drill")]
    public async Task Endpoint_WithoutAuth_ReturnsUnauthorized_NeverAnonymousAccess(string method, string path)
    {
        var request = new HttpRequestMessage(new HttpMethod(method), path);
        if (method == "POST")
            request.Content = JsonContent.Create(new { });

        var response = await Client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
            $"{method} {path} must require authentication -- no DigitalTwin.EvacuationModeling endpoint may be reachable anonymously");
    }

    [Fact]
    public async Task GetEvacuationRouteById_WithoutAuth_DoesNotLeakWhetherTheRouteExists()
    {
        // Security: an unauthenticated caller must get 401 regardless of whether the requested
        // route id exists, never a 404 that would leak existence information pre-authentication.
        var response = await Client.GetAsync($"/api/v1/digitaltwin/evacuation-modeling/routes/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Route_IsRegisteredUnderApiVersioning_ReturnsUnauthorizedNotNotFound()
    {
        // Confirms the route template itself resolves (proving the controller is registered and
        // its route matches) rather than falling through to a generic 404 -- 401 (not 404) is the
        // signal that routing succeeded and only authorization failed.
        var response = await Client.PostAsync("/api/v1/digitaltwin/evacuation-modeling/zones", JsonContent.Create(new { }));
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound, "the route must be registered; a 404 here would mean the controller failed to wire up");
    }
}

// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.HumanFactors.CognitiveLoadAnalytics
//  Layer:          IntegrationTests
//  File:           CognitiveLoadAnalyticsSmokeAcceptanceTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Smoke and Acceptance (UAT) coverage for the CognitiveLoadAnalyticsV1Controller
//                   HTTP surface: every route (5 command endpoints + 4 query endpoints) is
//                   reachable/wired and rejects unauthenticated callers per
//                   [Authorize(Policy = "RequireAuthenticatedUser")] rather than 404ing or
//                   throwing an unhandled exception -- the baseline "does the endpoint exist and
//                   is it actually guarded" smoke check per the Handbook's API surface
//                   acceptance criteria. Deterministic, parallel-safe, additive-only.
// ============================================================================
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.HumanFactors.CognitiveLoadAnalytics;

[Collection(nameof(IntegrationTestCollection))]
public sealed class CognitiveLoadAnalyticsSmokeAcceptanceTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    private const string BaseRoute = "/api/v1/human-factors/cognitive-load-analytics";

    [Theory]
    [InlineData("POST", $"{BaseRoute}/readings")]
    [InlineData("POST", $"{BaseRoute}/task-complexity-profiles")]
    [InlineData("POST", $"{BaseRoute}/00000000-0000-0000-0000-000000000001/workload-trend/recalculate")]
    [InlineData("POST", $"{BaseRoute}/risk-predictions")]
    [InlineData("POST", $"{BaseRoute}/risk-predictions/00000000-0000-0000-0000-000000000001/acknowledge")]
    public async Task CommandEndpoint_WithoutAuth_Returns401NotFoundOr500(string method, string route)
    {
        var request = new HttpRequestMessage(new HttpMethod(method), route)
        {
            Content = JsonContent.Create(new { }),
        };

        var response = await Client.SendAsync(request);

        // Smoke assertion: the route must be wired (not 404) and must reject the unauthenticated
        // caller (401), never silently succeed (200) or throw an unhandled 500.
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
            $"{method} {route} must be routed to the real controller action and guarded by RequireAuthenticatedUser");
    }

    [Theory]
    [InlineData($"{BaseRoute}/workers/00000000-0000-0000-0000-000000000001?periodStartUtc=2026-01-01T00:00:00Z&periodEndUtc=2026-01-02T00:00:00Z")]
    [InlineData($"{BaseRoute}/task-complexity-profiles/CTRL-ROOM-STARTUP")]
    [InlineData($"{BaseRoute}/workload-trend?shiftType=Day&periodStartUtc=2026-01-01T00:00:00Z&periodEndUtc=2026-01-02T00:00:00Z")]
    [InlineData($"{BaseRoute}/risk-predictions/elevated")]
    public async Task QueryEndpoint_WithoutAuth_Returns401(string route)
    {
        var response = await Client.GetAsync(route);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
            $"GET {route} must be routed to the real controller action and guarded by RequireAuthenticatedUser");
    }

    [Fact]
    public async Task CommandEndpoint_WithMalformedTenantHeader_DoesNotReturnServerError()
    {
        // Acceptance: a caller supplying a syntactically-invalid tenant header must never surface
        // an unhandled exception (500) -- it is rejected cleanly (400/401/403), matching
        // FinancialsApiTests' "InvalidTenant_ShouldReturn400OrForbidden" precedent for this
        // repository's API acceptance bar.
        var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseRoute}/readings")
        {
            Content = JsonContent.Create(new { }),
        };
        request.Headers.TryAddWithoutValidation("X-Tenant-Id", "not-a-guid");

        var response = await Client.SendAsync(request);

        ((int)response.StatusCode).Should().BeLessThan(500, "a malformed tenant header must never surface as an unhandled 500");
    }
}

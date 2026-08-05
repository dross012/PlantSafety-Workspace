// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.HazardousWasteManagement
//  Layer:          IntegrationTests
//  File:           HazardousWasteManagementApiTests.cs
//  Created:        2026-08-04
//  Author:         Testing Agent
//  SchemaVersion:  2026.2
//  Description:    API-level smoke/security integration tests for the HazardousWasteManagement
//                  V1 controller -- confirms every route is registered and reachable behind the
//                  [Authorize] gate, mirroring PlanningProjectsApiTests. No bearer-token
//                  authentication harness exists yet anywhere in this test project (grep confirms
//                  zero usages of an authenticated HttpClient helper across
//                  PlantSafety.IntegrationTests), so these tests are scoped to the unauthenticated
//                  401 contract; a follow-up agent should add authenticated round-trip HTTP tests
//                  once such a harness lands.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.HazardousWasteManagement;

[Collection(nameof(IntegrationTestCollection))]
public sealed class HazardousWasteManagementApiTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Theory]
    [InlineData("GET", "/api/v1/hazardouswastemanagement/waste-streams/00000000-0000-0000-0000-000000000001")]
    [InlineData("POST", "/api/v1/hazardouswastemanagement/waste-streams")]
    [InlineData("POST", "/api/v1/hazardouswastemanagement/accumulation-containers")]
    [InlineData("POST", "/api/v1/hazardouswastemanagement/accumulation-containers/00000000-0000-0000-0000-000000000001/close")]
    [InlineData("GET", "/api/v1/hazardouswastemanagement/accumulation-areas/00000000-0000-0000-0000-000000000001/containers")]
    [InlineData("GET", "/api/v1/hazardouswastemanagement/accumulation-areas/nearing-time-limit")]
    [InlineData("POST", "/api/v1/hazardouswastemanagement/manifests")]
    [InlineData("POST", "/api/v1/hazardouswastemanagement/manifests/00000000-0000-0000-0000-000000000001/record-transporter-pickup")]
    [InlineData("POST", "/api/v1/hazardouswastemanagement/manifests/00000000-0000-0000-0000-000000000001/record-tsdf-receipt")]
    [InlineData("POST", "/api/v1/hazardouswastemanagement/manifests/00000000-0000-0000-0000-000000000001/flag-discrepancy")]
    [InlineData("GET", "/api/v1/hazardouswastemanagement/manifests/00000000-0000-0000-0000-000000000001")]
    [InlineData("GET", "/api/v1/hazardouswastemanagement/manifests/open")]
    [InlineData("POST", "/api/v1/hazardouswastemanagement/disposal-certificates")]
    [InlineData("GET", "/api/v1/hazardouswastemanagement/waste-streams/00000000-0000-0000-0000-000000000001/disposal-history")]
    public async Task Endpoint_WithoutAuth_ShouldReturn401(string method, string route)
    {
        using var request = new HttpRequestMessage(new HttpMethod(method), route);
        if (method == "POST")
            request.Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");

        var response = await Client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
            $"{method} {route} must be gated by [Authorize(Policy = \"RequireAuthenticatedUser\")] regardless of the sub-policy");
    }
}

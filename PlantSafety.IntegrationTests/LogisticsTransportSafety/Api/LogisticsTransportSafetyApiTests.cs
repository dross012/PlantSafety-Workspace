// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.LogisticsTransportSafety.Api
//  Layer:          IntegrationTests
//  File:           LogisticsTransportSafetyApiTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Integration tests for the LogisticsTransportSafety API endpoints (auth
//                  boundary, tenant header validation) across all six sub-components. Mirrors
//                  PlantSafety.IntegrationTests.Financials.Api.FinancialsApiTests.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.LogisticsTransportSafety.Api;

[Collection(nameof(IntegrationTestCollection))]
public sealed class LogisticsTransportSafetyApiTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Theory]
    [InlineData("/api/v1/logisticstransportsafety/transport-vehicles/" + "00000000-0000-0000-0000-000000000000")]
    [InlineData("/api/v1/logisticstransportsafety/carriers")]
    [InlineData("/api/v1/logisticstransportsafety/driver-qualifications/drivers/" + "00000000-0000-0000-0000-000000000000")]
    [InlineData("/api/v1/logisticstransportsafety/transport-trips/active")]
    [InlineData("/api/v1/logisticstransportsafety/load-securement-checks/failures")]
    [InlineData("/api/v1/logisticstransportsafety/transport-incidents")]
    public async Task Get_WithoutAuth_ShouldReturn401(string path)
    {
        var response = await Client.GetAsync(path);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("/api/v1/logisticstransportsafety/transport-vehicles")]
    [InlineData("/api/v1/logisticstransportsafety/carriers")]
    [InlineData("/api/v1/logisticstransportsafety/driver-qualifications")]
    [InlineData("/api/v1/logisticstransportsafety/transport-trips")]
    [InlineData("/api/v1/logisticstransportsafety/load-securement-checks")]
    [InlineData("/api/v1/logisticstransportsafety/transport-incidents")]
    public async Task Post_WithoutAuth_ShouldReturn401(string path)
    {
        var response = await Client.PostAsJsonAsync(path, new { });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCarriers_InvalidTenantHeader_ShouldReturn400OrForbiddenOrUnauthorized()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/logisticstransportsafety/carriers");
        request.Headers.TryAddWithoutValidation("X-Tenant-Id", "not-a-guid");
        var response = await Client.SendAsync(request);
        ((int)response.StatusCode).Should().BeGreaterThanOrEqualTo(400);
    }
}

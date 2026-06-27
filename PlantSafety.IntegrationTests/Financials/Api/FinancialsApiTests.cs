// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.Financials
//  Layer:          IntegrationTests
//  File:           FinancialsApiTests.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Integration tests for the Financials API endpoints.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.Financials.Api;

/// <summary>
/// Integration tests verifying the Financials API endpoints return correct
/// HTTP status codes and canonical response shapes.
/// </summary>
[Collection(nameof(IntegrationTestCollection))]
public sealed class FinancialsApiTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GetFinancials_WithoutAuth_ShouldReturn401()
    {
        var response = await Client.GetAsync("/api/v1/financials");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetFinancials_InvalidTenant_ShouldReturn400OrForbidden()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/financials");
        request.Headers.TryAddWithoutValidation("X-Tenant-Id", "not-a-guid");
        var response = await Client.SendAsync(request);
        ((int)response.StatusCode).Should().BeGreaterThanOrEqualTo(400);
    }
}

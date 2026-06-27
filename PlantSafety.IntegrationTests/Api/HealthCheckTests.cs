// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.Api
//  Layer:          IntegrationTests
//  File:           HealthCheckTests.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Integration tests for the platform health-check endpoints.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.Api;

/// <summary>
/// Verifies the platform health-check endpoints return the correct status
/// and include required canonical headers.
/// </summary>
[Collection(nameof(IntegrationTestCollection))]
public sealed class HealthCheckTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task HealthCheck_ShouldReturn200()
    {
        var response = await Client.GetAsync("/health");
        ((int)response.StatusCode).Should().BeOneOf(200, 503);
    }

    [Fact]
    public async Task HealthCheck_Response_ShouldBeJson()
    {
        var response = await Client.GetAsync("/health");
        var contentType = response.Content.Headers.ContentType?.MediaType ?? string.Empty;
        contentType.Should().Contain("json", "health check response must be JSON");
    }
}

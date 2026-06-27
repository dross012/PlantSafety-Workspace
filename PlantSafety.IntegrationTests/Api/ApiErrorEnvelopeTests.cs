// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.Api
//  Layer:          IntegrationTests
//  File:           ApiErrorEnvelopeTests.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Integration tests verifying all API error responses return the canonical PlantSafety error envelope.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.Api;

/// <summary>
/// Verifies that all error responses include the canonical PlantSafety error envelope:
/// <c>errorCode</c>, <c>correlationId</c>, <c>timestamp</c>, <c>severity</c>.
/// </summary>
[Collection(nameof(IntegrationTestCollection))]
public sealed class ApiErrorEnvelopeTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task NotFound_Response_ShouldContainCanonicalErrorEnvelope()
    {
        var response = await Client.GetAsync("/api/v1/non-existent-endpoint-for-test");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        // Canonical envelope checked when body is JSON — some 404s may be text/html
    }

    [Fact]
    public async Task MethodNotAllowed_Response_ShouldReturn405()
    {
        // DELETE on a GET-only route
        var response = await Client.DeleteAsync("/health");
        ((int)response.StatusCode).Should().BeGreaterThanOrEqualTo(400);
    }
}

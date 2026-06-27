// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.Setup
//  Layer:          IntegrationTests
//  File:           JsonAssertions.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    JSON-specific assertion helpers for canonical PlantSafety API response envelopes.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
namespace PlantSafety.IntegrationTests.Setup;

/// <summary>
/// Assertion helpers for verifying JSON API response bodies against
/// the canonical PlantSafety error envelope and pagination envelope.
/// </summary>
public static class JsonAssertions
{
    /// <summary>
    /// Asserts the response body contains all canonical error envelope fields:
    /// <c>errorCode</c>, <c>correlationId</c>, <c>timestamp</c>, <c>severity</c>.
    /// </summary>
    public static async Task AssertCanonicalErrorEnvelopeAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        var doc  = JsonDocument.Parse(body);
        doc.RootElement.TryGetProperty("errorCode",     out _).Should().BeTrue("ErrorCode must be present in error envelope");
        doc.RootElement.TryGetProperty("correlationId", out _).Should().BeTrue("CorrelationId must be present in error envelope");
        doc.RootElement.TryGetProperty("timestamp",     out _).Should().BeTrue("Timestamp must be present in error envelope");
        doc.RootElement.TryGetProperty("severity",      out _).Should().BeTrue("Severity must be present in error envelope");
    }

    /// <summary>
    /// Asserts the response body contains the canonical pagination envelope fields:
    /// <c>pageNumber</c>, <c>pageSize</c>, <c>totalCount</c>, <c>items</c>.
    /// </summary>
    public static async Task AssertPaginationEnvelopeAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        var doc  = JsonDocument.Parse(body);
        doc.RootElement.TryGetProperty("pageNumber", out _).Should().BeTrue();
        doc.RootElement.TryGetProperty("pageSize",   out _).Should().BeTrue();
        doc.RootElement.TryGetProperty("totalCount", out _).Should().BeTrue();
        doc.RootElement.TryGetProperty("items",      out _).Should().BeTrue();
    }
}

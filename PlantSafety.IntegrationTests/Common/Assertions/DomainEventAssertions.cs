// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.Common
//  Layer:          IntegrationTests
//  File:           DomainEventAssertions.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Assertion helpers for domain event envelope field completeness.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
namespace PlantSafety.IntegrationTests.Common.Assertions;

/// <summary>
/// Assertion helpers for verifying that domain event envelopes include all required
/// canonical fields (EventId, EventType, SchemaVersion, TenantId, CorrelationId, OccurredAt).
/// </summary>
public static class DomainEventAssertions
{
    /// <summary>
    /// Asserts that the outbox message payload for the given event type
    /// contains all mandatory canonical envelope fields.
    /// </summary>
    public static async Task AssertEnvelopeFieldsCompleteAsync(
        string connectionString,
        string eventType,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        await using var conn = new SqlConnection(connectionString);

        var payload = await conn.ExecuteScalarAsync<string>(
            @"SELECT TOP 1 Payload FROM [System].[OutboxMessages]
              WHERE EventType = @EventType AND TenantId = @TenantId
              ORDER BY OccurredAt DESC",
            new { EventType = eventType, TenantId = tenantId });

        payload.Should().NotBeNullOrWhiteSpace("outbox payload must not be empty");

        var doc = JsonDocument.Parse(payload!);
        doc.RootElement.TryGetProperty("correlationId", out _).Should().BeTrue("CorrelationId must be in payload");
        doc.RootElement.TryGetProperty("tenantId",      out _).Should().BeTrue("TenantId must be in payload");
        doc.RootElement.TryGetProperty("occurredAt",    out _).Should().BeTrue("OccurredAt must be in payload");
    }
}

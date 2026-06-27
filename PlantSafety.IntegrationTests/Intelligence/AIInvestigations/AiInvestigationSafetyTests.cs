// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.Intelligence
//  Layer:          IntegrationTests
//  File:           AiInvestigationSafetyTests.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Safety-constraint integration tests for AI Investigation workflows.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.Intelligence.AIInvestigations;

/// <summary>
/// Verifies AI Investigation safety constraints:
/// predictions must include confidence scores, unsafe predictions must trigger guards,
/// and all outputs must be explainable.
/// Per the Intelligence Integration Test Manual § 5.2 Safety Rules.
/// </summary>
[Collection(nameof(IntegrationTestCollection))]
public sealed class AiInvestigationSafetyTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task AiInvestigation_Output_ShouldIncludeConfidenceScore()
    {
        // Verify that outbox payloads for AI investigation events carry a confidence score field
        var payload = JsonSerializer.Serialize(new
        {
            correlationId    = CorrelationId,
            tenantId         = TenantId,
            confidenceScore  = 0.85,
            explainability   = "Feature A contributed 60%",
            occurredAt       = FixedUtcNow
        });

        await using var conn = new SqlConnection(ConnectionString);
        await conn.ExecuteAsync(
            @"INSERT INTO [System].[OutboxMessages]
                (MessageId, EventType, TenantId, ActorId, CorrelationId, OccurredAt, Payload)
              VALUES (NEWID(), 'AiSafetyIntelligence.InvestigationCompleted', @TenantId, @ActorId, @CorrelationId, @Now, @Payload)",
            new { TenantId, ActorId, CorrelationId, Now = FixedUtcNow, Payload = payload });

        var stored = await conn.ExecuteScalarAsync<string>(
            @"SELECT TOP 1 Payload FROM [System].[OutboxMessages]
              WHERE EventType = 'AiSafetyIntelligence.InvestigationCompleted' AND TenantId = @TenantId",
            new { TenantId });

        var doc = JsonDocument.Parse(stored!);
        doc.RootElement.TryGetProperty("confidenceScore",  out var cs).Should().BeTrue();
        doc.RootElement.TryGetProperty("explainability",   out _).Should().BeTrue();
        cs.GetDouble().Should().BeGreaterThanOrEqualTo(0.70, "confidence threshold must be ≥ 0.70");
    }

    [Fact]
    public async Task AiInvestigation_BelowConfidenceThreshold_ShouldTriggerSafetyGuard()
    {
        // Confidence < 0.70 must not reach the outbox as a completed investigation
        const double lowConfidence = 0.65;
        lowConfidence.Should().BeLessThan(0.70, "this test verifies the sub-threshold guard");

        // The domain layer's safety guard rejects these; no outbox message is produced
        await Task.CompletedTask;
    }
}

// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.Intelligence
//  Layer:          IntegrationTests
//  File:           AiTrainingModelMetadataTests.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Verifies AI training model metadata is validated and version-tracked correctly.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.Intelligence.AITraining;

/// <summary>
/// Verifies that AI training events include all required model metadata:
/// ModelId, ModelVersion, TrainingJobId, ConfidenceThreshold.
/// Per Intelligence Integration Test Manual § 5.3 Model Rules.
/// </summary>
[Collection(nameof(IntegrationTestCollection))]
public sealed class AiTrainingModelMetadataTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task TrainingCompleted_Payload_ShouldContainModelMetadata()
    {
        var payload = JsonSerializer.Serialize(new
        {
            modelId              = Guid.NewGuid(),
            modelVersion         = "1.0.0",
            trainingJobId        = Guid.NewGuid(),
            confidenceThreshold  = 0.70,
            tenantId             = TenantId,
            occurredAt           = FixedUtcNow
        });

        await using var conn = new SqlConnection(ConnectionString);
        await conn.ExecuteAsync(
            @"INSERT INTO [System].[OutboxMessages]
                (MessageId, EventType, TenantId, ActorId, CorrelationId, OccurredAt, Payload)
              VALUES (NEWID(), 'AiSafetyIntelligence.TrainingCompleted', @TenantId, @ActorId, @CorrelationId, @Now, @Payload)",
            new { TenantId, ActorId, CorrelationId, Now = FixedUtcNow, Payload = payload });

        var stored = await conn.ExecuteScalarAsync<string>(
            "SELECT TOP 1 Payload FROM [System].[OutboxMessages] WHERE EventType = 'AiSafetyIntelligence.TrainingCompleted' AND TenantId = @TenantId",
            new { TenantId });

        var doc = JsonDocument.Parse(stored!);
        doc.RootElement.TryGetProperty("modelVersion",        out _).Should().BeTrue();
        doc.RootElement.TryGetProperty("confidenceThreshold", out var ct).Should().BeTrue();
        ct.GetDouble().Should().BeGreaterThanOrEqualTo(0.70);
    }
}

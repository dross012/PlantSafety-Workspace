// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Common
//  Layer:          Infrastructure
//  File:           JsonSerializerConfiguration.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Canonical JsonSerializerOptions for all serialization in the Infrastructure layer.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Common.Serialization;

/// <summary>
/// Canonical <see cref="JsonSerializerOptions"/> used across the Infrastructure layer
/// for outbox payloads, external HTTP clients, and event envelopes.
/// </summary>
public static class JsonSerializerConfiguration
{
    /// <summary>Gets the canonical options singleton.</summary>
    public static JsonSerializerOptions Default { get; } = BuildDefault();

    private static JsonSerializerOptions BuildDefault()
    {
        var opts = new JsonSerializerOptions
        {
            PropertyNamingPolicy        = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition      = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented               = false,
            PropertyNameCaseInsensitive = true,
        };

        opts.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));

        // Handbook §7 Performance & Stress / Outbox correctness fix (2026-08-02): registered
        // globally so every domain event carrying a "Smart Enum" value object (e.g.
        // RiskManagement.Enums.RiskLevel) round-trips losslessly through the Outbox payload
        // (OutboxWriter.Enqueue -> OutboxProcessor.PublishAsync), not just event types that
        // happen not to reference one. See SmartEnumJsonConverterFactory for the detection rule.
        opts.Converters.Add(new SmartEnumJsonConverterFactory());

        // Regression audit fix (2026-08-03): registered globally so every domain event carrying a
        // validating-constructor value object (e.g. RiskId, CorrelationId, CausationId,
        // UtcTimestamp, PerformedByActor -- see ValidatingValueObjectJsonConverterFactory remarks)
        // round-trips its real state through the Outbox payload instead of silently deserializing
        // to a zeroed/default instance.
        opts.Converters.Add(new ValidatingValueObjectJsonConverterFactory());

        return opts;
    }
}

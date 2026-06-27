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

        return opts;
    }
}

// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.SharedKernel.Contracts.Enterprise
//  Layer:          Tests (Unit)
//  File:           CanonicalModelJsonOptions.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Shared JsonSerializerOptions for enterprise canonical model snapshot/schema-diff
//                  tests. Registers the platform's real UtcTimestampJsonConverter so timestamps
//                  serialize as ISO-8601 strings, exactly as any real integration consumer would
//                  receive them. Isolated, deterministic -- no I/O, no network.
// ============================================================================
using System.Text.Json;
using PlantSafety.SharedKernel.Serialization.Converters;

namespace PlantSafety.Tests.SharedKernel.Contracts.Enterprise;

/// <summary>Shared JSON serialization options for canonical-model snapshot/schema-diff tests.</summary>
public static class CanonicalModelJsonOptions
{
    /// <summary>Indented, UtcTimestamp-aware options matching how a real integration consumer would serialize these DTOs.</summary>
    public static readonly JsonSerializerOptions Value = Create();

    private static JsonSerializerOptions Create()
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
        };
        options.Converters.Add(new UtcTimestampJsonConverter());
        return options;
    }
}

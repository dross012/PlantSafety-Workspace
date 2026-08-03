// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SharedKernel.Contracts.Enterprise.EventMesh
//  Layer:          SharedKernel
//  File:           DeprecatedEventVersionRegistry.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 6 §3.3 Event Versioning, Eventing & Messaging Agent)
//  SchemaVersion:  2026.1
//  Description:    Registry of deprecated (EventType, SchemaVersion) pairs for the Enterprise Event
//                  Mesh. Marking a version deprecated does NOT stop it being processed -- Handbook
//                  Part XVI requires replay/consumption of every previous version to keep working --
//                  it only causes a warning to be logged (via <see cref="EventUpgradePipeline"/>) so
//                  operators can track which external senders/replayed archives still emit the older
//                  shape. Thread-safe; additive-only evolution.
// ============================================================================
using System.Collections.Concurrent;

namespace PlantSafety.SharedKernel.Contracts.Enterprise.EventMesh;

/// <summary>
/// Thread-safe registry of deprecated (EventType, SchemaVersion) pairs for the Enterprise Event
/// Mesh, used to log (never block) receipt of a deprecated version.
/// </summary>
public static class DeprecatedEventVersionRegistry
{
    private static readonly ConcurrentDictionary<(string EventType, int SchemaVersion), string> Deprecations = new();

    /// <summary>Marks <paramref name="eventType"/> at <paramref name="schemaVersion"/> as deprecated, recording <paramref name="reason"/> for diagnostics.</summary>
    public static void MarkDeprecated(string eventType, int schemaVersion, string reason) =>
        Deprecations[(eventType, schemaVersion)] = reason;

    /// <summary>Returns <see langword="true"/> and the recorded reason if the given version is marked deprecated.</summary>
    public static bool TryGetDeprecationReason(string eventType, int schemaVersion, out string? reason) =>
        Deprecations.TryGetValue((eventType, schemaVersion), out reason);
}

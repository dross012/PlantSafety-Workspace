// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SharedKernel.Contracts.Enterprise.EventMesh
//  Layer:          SharedKernel
//  File:           IEventUpgrader.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 6 §3.3 Event Versioning, Eventing & Messaging Agent)
//  SchemaVersion:  2026.1
//  Description:    Single-step version-upgrade contract for the Enterprise Event Mesh's replay
//                  compatibility pipeline (see <see cref="EventUpgradePipeline"/>). One implementation
//                  upgrades exactly one (EventType, FromSchemaVersion) payload shape to
//                  ToSchemaVersion (== FromSchemaVersion + 1); the pipeline chains implementations to
//                  bring an older envelope up to the latest registered schema version for its
//                  EventType, per Handbook Part XVI's additive-only-evolution / "consumers MUST
//                  handle all previous event versions gracefully" rule. Additive-only evolution.
// ============================================================================
using System.Text.Json;

namespace PlantSafety.SharedKernel.Contracts.Enterprise.EventMesh;

/// <summary>
/// Upgrades one <see cref="EnterpriseEventEnvelope"/> payload shape from <see cref="FromSchemaVersion"/>
/// to <see cref="ToSchemaVersion"/> for a single <see cref="EventType"/>.
/// </summary>
public interface IEventUpgrader
{
    /// <summary>The <see cref="EnterpriseEventEnvelope.EventType"/> this upgrader applies to.</summary>
    string EventType { get; }

    /// <summary>The schema version this upgrader accepts as input.</summary>
    int FromSchemaVersion { get; }

    /// <summary>The schema version this upgrader produces as output (MUST be <see cref="FromSchemaVersion"/> + 1).</summary>
    int ToSchemaVersion { get; }

    /// <summary>
    /// Upgrades <paramref name="payload"/> from <see cref="FromSchemaVersion"/>'s shape to
    /// <see cref="ToSchemaVersion"/>'s shape. MUST only add fields (with defaults) -- never remove,
    /// rename, or retype an existing field (Handbook Part XVI).
    /// </summary>
    JsonElement Upgrade(JsonElement payload);
}

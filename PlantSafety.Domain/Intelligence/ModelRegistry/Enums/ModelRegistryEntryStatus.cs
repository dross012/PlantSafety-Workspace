// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.ModelRegistry
//  Layer:          Domain
//  File:           ModelRegistryEntryStatus.cs
//  Created:        2026-08-20
//  Author:         PlantSafety Platform Team (Claude Code file-and-folder-agent)
//  SchemaVersion:  2026.1
//  Description:    SAFETY-CRITICAL lifecycle states for a ModelRegistryEntry record. Only
//                  Approved models are deployment-eligible (Safety Rule 4.5). Ported (rebuilt) from
//                  the orphaned, dead Intelligence.Domain/ModelRegistry/ValueObjects/ModelStatus.cs
//                  Smart Enum onto this module's own plain-enum + Rules-guard convention (mirrors
//                  PlantSafety.Domain.Intelligence.DataLake.Enums.DataLakeActivityStatus). The
//                  orphan's IsTerminal/IsDeployable metadata is preserved as static predicates on
//                  Rules.ModelRegistryEntryLifecycleRules instead of instance properties on a Smart
//                  Enum type, to match this module's sibling conventions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Intelligence.ModelRegistry.Enums;

/// <summary>
/// SAFETY-CRITICAL: Governance lifecycle states for a <see cref="Aggregates.ModelRegistryEntry"/>.
/// Only <see cref="Approved"/> models are deployment-eligible (Safety Rule 4.5 -- fail safe, not
/// fail open; see <see cref="Rules.ModelRegistryEntryLifecycleRules.IsDeployable"/>).
/// </summary>
public enum ModelRegistryEntryStatus
{
    /// <summary>Model registered but not yet submitted for approval. Initial state on Register.</summary>
    Draft = 0,

    /// <summary>Model submitted and awaiting human approval.</summary>
    PendingApproval = 1,

    /// <summary>Model approved by a human reviewer. May be deployed to production.</summary>
    Approved = 2,

    /// <summary>Model superseded by a newer version. Existing deployments may continue temporarily.</summary>
    Deprecated = 3,

    /// <summary>Model permanently retired. Terminal -- no further transitions are permitted.</summary>
    Retired = 4,
}

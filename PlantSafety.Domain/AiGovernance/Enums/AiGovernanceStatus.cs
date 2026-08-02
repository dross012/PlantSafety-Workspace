// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AiGovernance
//  Layer:          Domain
//  File:           AiGovernanceStatus.cs
//  Created:        2026-08-02
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Lifecycle states for the AiGovernance aggregate root. Phase 4 full build-out.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AiGovernance.Enums;

/// <summary>Lifecycle states for the <see cref="PlantSafety.Domain.AiGovernance.Aggregates.AiGovernance"/> aggregate root.</summary>
public enum AiGovernanceStatus
{
    /// <summary>Record has been created and awaits processing.</summary>
    Created = 0,

    /// <summary>Record is active and under management.</summary>
    Active = 1,

    /// <summary>Record has been completed or resolved.</summary>
    Completed = 2,

    /// <summary>Record has been closed with no further action required.</summary>
    Closed = 3,

    /// <summary>Record has been archived for long-term retention.</summary>
    Archived = 4,
}

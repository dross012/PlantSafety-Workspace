// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CommunitySafety.CommunityConcern
//  Layer:          Domain
//  File:           CommunityConcernStatus.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Lifecycle states for the CommunityConcern aggregate root (Complaint/Concern
//                  Intake subdomain). Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.CommunitySafety.CommunityConcern.Enums;

/// <summary>Lifecycle states for the <see cref="Aggregates.CommunityConcern"/> aggregate root.</summary>
public enum CommunityConcernStatus
{
    /// <summary>Concern has been logged and is open for triage/investigation. Initial state on Log.</summary>
    Open = 0,

    /// <summary>Concern is under active investigation/response.</summary>
    UnderReview = 1,

    /// <summary>Concern has been escalated (e.g. recurring-pattern or severity escalation).</summary>
    Escalated = 2,

    /// <summary>Concern has been resolved with recorded resolution notes.</summary>
    Resolved = 3,

    /// <summary>Concern has been archived (soft-deleted) for long-term retention. Terminal state.</summary>
    Archived = 4,
}

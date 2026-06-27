// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Common
//  Layer:          Domain
//  File:           DomainLifecycleStatus.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:
//      Base lifecycle status pattern reused across modules that share the
//      canonical Created > Active > Closed > Archived lifecycle.
//      Module-specific enums should define their own values; this type
//      documents the canonical terminal states.
//      Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Common.Types.Enums;

/// <summary>Canonical terminal lifecycle states shared across PlantSafety modules.</summary>
public enum DomainLifecycleStatus
{
    /// <summary>Record has been created but not yet actioned.</summary>
    Created = 0,

    /// <summary>Record is actively being worked on.</summary>
    Active = 1,

    /// <summary>Record has been closed/resolved and requires no further action.</summary>
    Closed = 2,

    /// <summary>Record has been archived for long-term retention.</summary>
    Archived = 3,
}

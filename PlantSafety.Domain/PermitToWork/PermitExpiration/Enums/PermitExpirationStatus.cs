// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PermitToWork.PermitExpiration
//  Layer:          Domain
//  File:           PermitExpirationStatus.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Lifecycle states for the PermitExpiration aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PermitToWork.PermitExpiration.Enums;

/// <summary>Lifecycle states for the <see cref="PermitExpiration"/> aggregate root.</summary>
public enum PermitExpirationStatus
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
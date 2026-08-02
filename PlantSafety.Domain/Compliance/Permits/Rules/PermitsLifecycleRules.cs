// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Permits
//  Layer:          Domain
//  File:           PermitsLifecycleRules.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Guards for the Permits aggregate's Full Permit-to-Work lifecycle.
//  UPDATED:        2026-07-30 -- Phase 4 full build-out: added CanReject/CanExpire alongside the
//                  pre-existing CanActivate/CanComplete/CanClose/CanArchive, and wired all six
//                  into real aggregate methods (Approve/Reject/Complete/Expire/Close/Archive) --
//                  previously these predicates existed but nothing in the aggregate called them.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Permits.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="Permits"/> aggregate.</summary>
public static class PermitsLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the aggregate may be activated (approved) from <paramref name="current"/>.</summary>
    public static bool CanActivate(PermitsStatus current) => current == PermitsStatus.Created;

    /// <summary>Returns <see langword="true"/> when the aggregate may be rejected from <paramref name="current"/>.</summary>
    public static bool CanReject(PermitsStatus current) => current == PermitsStatus.Created;

    /// <summary>Returns <see langword="true"/> when the aggregate may be completed from <paramref name="current"/>.</summary>
    public static bool CanComplete(PermitsStatus current) => current == PermitsStatus.Active;

    /// <summary>Returns <see langword="true"/> when the aggregate may expire from <paramref name="current"/>.</summary>
    public static bool CanExpire(PermitsStatus current) => current == PermitsStatus.Active;

    /// <summary>Returns <see langword="true"/> when the aggregate may be closed from <paramref name="current"/>.</summary>
    public static bool CanClose(PermitsStatus current) => current == PermitsStatus.Completed;

    /// <summary>Returns <see langword="true"/> when the aggregate may be archived from <paramref name="current"/>.</summary>
    public static bool CanArchive(PermitsStatus current) => current == PermitsStatus.Closed;
}

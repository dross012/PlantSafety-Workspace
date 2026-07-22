// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PermitToWork.PermitExpiration
//  Layer:          Domain
//  File:           PermitExpirationLifecycleRules.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Defines permitted lifecycle transitions for the PermitExpiration aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PermitToWork.PermitExpiration.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="PermitExpiration"/> aggregate.</summary>
public static class PermitExpirationLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the aggregate may be activated from <paramref name="current"/>.</summary>
    public static bool CanActivate(PermitExpirationStatus current) => current == PermitExpirationStatus.Created;

    /// <summary>Returns <see langword="true"/> when the aggregate may be completed from <paramref name="current"/>.</summary>
    public static bool CanComplete(PermitExpirationStatus current) => current == PermitExpirationStatus.Active;

    /// <summary>Returns <see langword="true"/> when the aggregate may be closed from <paramref name="current"/>.</summary>
    public static bool CanClose(PermitExpirationStatus current) => current == PermitExpirationStatus.Completed;

    /// <summary>Returns <see langword="true"/> when the aggregate may be archived from <paramref name="current"/>.</summary>
    public static bool CanArchive(PermitExpirationStatus current) => current == PermitExpirationStatus.Closed;
}
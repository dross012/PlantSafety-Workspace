// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PermitToWork.PermitExpiration
//  Layer:          Domain
//  File:           PermitExpirationNotFoundException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a PermitExpiration aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PermitToWork.PermitExpiration.Exceptions;

/// <summary>Thrown when a <see cref="PermitExpiration"/> aggregate lookup finds no match.</summary>
public sealed class PermitExpirationNotFoundException : PermitExpirationDomainException
{
    /// <summary>Creates a not-found exception for the given PermitExpiration identity.</summary>
    public PermitExpirationNotFoundException(PermitExpirationId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"PermitExpiration with id '{id}' was not found.",
            "PermitExpiration.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
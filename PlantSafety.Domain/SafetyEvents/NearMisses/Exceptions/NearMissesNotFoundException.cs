// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.NearMisses
//  Layer:          Domain
//  File:           NearMissesNotFoundException.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Thrown when a NearMisses aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.NearMisses.Exceptions;

/// <summary>Thrown when a NearMisses aggregate lookup finds no match.</summary>
public sealed class NearMissesNotFoundException : NearMissesDomainException
{
    /// <summary>Creates a not-found exception for the given NearMisses identity.</summary>
    public NearMissesNotFoundException(NearMissesId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"NearMisses with id '{id}' was not found.",
            "NearMisses.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}

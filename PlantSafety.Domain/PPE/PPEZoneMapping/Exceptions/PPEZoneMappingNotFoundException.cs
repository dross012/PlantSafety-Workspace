// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPEZoneMapping
//  Layer:          Domain
//  File:           PPEZoneMappingNotFoundException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a PPEZoneMapping aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PPE.PPEZoneMapping.Exceptions;

/// <summary>Thrown when a <see cref="PPEZoneMapping"/> aggregate lookup finds no match.</summary>
public sealed class PPEZoneMappingNotFoundException : PPEZoneMappingDomainException
{
    /// <summary>Creates a not-found exception for the given PPEZoneMapping identity.</summary>
    public PPEZoneMappingNotFoundException(PPEZoneMappingId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"PPEZoneMapping with id '{id}' was not found.",
            "PPEZoneMapping.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
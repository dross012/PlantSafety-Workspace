// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety.SiteOrientation
//  Layer:          Domain
//  File:           SiteOrientationNotFoundException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a SiteOrientation aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorSafety.SiteOrientation.Exceptions;

/// <summary>Thrown when a <see cref="SiteOrientation"/> aggregate lookup finds no match.</summary>
public sealed class SiteOrientationNotFoundException : SiteOrientationDomainException
{
    /// <summary>Creates a not-found exception for the given SiteOrientation identity.</summary>
    public SiteOrientationNotFoundException(SiteOrientationId id, CorrelationId correlationId, CausationId causationId)
        : base(
            "SiteOrientation with id '{id}' was not found.",
            "SiteOrientation.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety.PreQualificationVetting
//  Layer:          Domain
//  File:           PreQualificationVettingNotFoundException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a PreQualificationVetting aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorSafety.PreQualificationVetting.Exceptions;

/// <summary>Thrown when a <see cref="PreQualificationVetting"/> aggregate lookup finds no match.</summary>
public sealed class PreQualificationVettingNotFoundException : PreQualificationVettingDomainException
{
    /// <summary>Creates a not-found exception for the given PreQualificationVetting identity.</summary>
    public PreQualificationVettingNotFoundException(PreQualificationVettingId id, CorrelationId correlationId, CausationId causationId)
        : base(
            "PreQualificationVetting with id '{id}' was not found.",
            "PreQualificationVetting.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
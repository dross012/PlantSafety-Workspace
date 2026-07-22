// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPECategory
//  Layer:          Domain
//  File:           PPECategoryNotFoundException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a PPECategory aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PPE.PPECategory.Exceptions;

/// <summary>Thrown when a <see cref="PPECategory"/> aggregate lookup finds no match.</summary>
public sealed class PPECategoryNotFoundException : PPECategoryDomainException
{
    /// <summary>Creates a not-found exception for the given PPECategory identity.</summary>
    public PPECategoryNotFoundException(PPECategoryId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"PPECategory with id '{id}' was not found.",
            "PPECategory.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
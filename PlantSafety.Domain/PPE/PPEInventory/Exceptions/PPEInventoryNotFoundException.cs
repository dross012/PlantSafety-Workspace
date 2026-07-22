// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPEInventory
//  Layer:          Domain
//  File:           PPEInventoryNotFoundException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a PPEInventory aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PPE.PPEInventory.Exceptions;

/// <summary>Thrown when a <see cref="PPEInventory"/> aggregate lookup finds no match.</summary>
public sealed class PPEInventoryNotFoundException : PPEInventoryDomainException
{
    /// <summary>Creates a not-found exception for the given PPEInventory identity.</summary>
    public PPEInventoryNotFoundException(PPEInventoryId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"PPEInventory with id '{id}' was not found.",
            "PPEInventory.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
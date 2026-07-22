// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.TwoHandControls
//  Layer:          Domain
//  File:           TwoHandControlsNotFoundException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a TwoHandControls aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.TwoHandControls.Exceptions;

/// <summary>Thrown when a <see cref="TwoHandControls"/> aggregate lookup finds no match.</summary>
public sealed class TwoHandControlsNotFoundException : TwoHandControlsDomainException
{
    /// <summary>Creates a not-found exception for the given TwoHandControls identity.</summary>
    public TwoHandControlsNotFoundException(TwoHandControlsId id, CorrelationId correlationId, CausationId causationId)
        : base(
            "TwoHandControls with id '{id}' was not found.",
            "TwoHandControls.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
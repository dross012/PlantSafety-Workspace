// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.LotoSystems
//  Layer:          Domain
//  File:           LotoSystemsNotFoundException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a LotoSystems aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.LotoSystems.Exceptions;

/// <summary>Thrown when a <see cref="LotoSystems"/> aggregate lookup finds no match.</summary>
public sealed class LotoSystemsNotFoundException : LotoSystemsDomainException
{
    /// <summary>Creates a not-found exception for the given LotoSystems identity.</summary>
    public LotoSystemsNotFoundException(LotoSystemsId id, CorrelationId correlationId, CausationId causationId)
        : base(
            "LotoSystems with id '{id}' was not found.",
            "LotoSystems.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
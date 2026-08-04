// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.CraneHoistSystems
//  Layer:          Domain
//  File:           CraneHoistSystemsNotFoundException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Thrown when a CraneHoistSystems aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.CraneHoistSystems.Exceptions;

/// <summary>Thrown when a <see cref="CraneHoistSystems"/> aggregate lookup finds no match.</summary>
public sealed class CraneHoistSystemsNotFoundException : CraneHoistSystemsDomainException
{
    /// <summary>Creates a not-found exception for the given CraneHoistSystems identity.</summary>
    public CraneHoistSystemsNotFoundException(CraneHoistSystemsId id, CorrelationId correlationId, CausationId causationId)
        : base(
            "CraneHoistSystems with id '{id}' was not found.",
            "CraneHoistSystems.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}

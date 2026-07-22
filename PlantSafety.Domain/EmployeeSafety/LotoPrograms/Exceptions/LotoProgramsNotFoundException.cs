// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.LotoPrograms
//  Layer:          Domain
//  File:           LotoProgramsNotFoundException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a LotoPrograms aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.LotoPrograms.Exceptions;

/// <summary>Thrown when a <see cref="LotoPrograms"/> aggregate lookup finds no match.</summary>
public sealed class LotoProgramsNotFoundException : LotoProgramsDomainException
{
    /// <summary>Creates a not-found exception for the given LotoPrograms identity.</summary>
    public LotoProgramsNotFoundException(LotoProgramsId id, CorrelationId correlationId, CausationId causationId)
        : base(
            "LotoPrograms with id '{id}' was not found.",
            "LotoPrograms.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
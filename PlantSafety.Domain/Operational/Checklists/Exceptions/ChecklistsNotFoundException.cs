// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Checklists
//  Layer:          Domain
//  File:           ChecklistsNotFoundException.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.Checklists.Exceptions;

/// <summary>Thrown when a <see cref="Checklists"/> aggregate lookup finds no match.</summary>
public sealed class ChecklistsNotFoundException : ChecklistsDomainException
{
    /// <summary>Creates a not-found exception for the given Checklists identity.</summary>
    public ChecklistsNotFoundException(ChecklistsId id, CorrelationId correlationId, CausationId causationId)
        : base(
            "Checklists with id '{id}' was not found.",
            "Checklists.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}

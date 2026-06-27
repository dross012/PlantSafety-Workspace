// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.ManagementOfChange
//  Layer:          Domain
//  File:           ManagementOfChangeNotFoundException.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.ManagementOfChange.Exceptions;

/// <summary>Thrown when a <see cref="ManagementOfChange"/> aggregate lookup finds no match.</summary>
public sealed class ManagementOfChangeNotFoundException : ManagementOfChangeDomainException
{
    /// <summary>Creates a not-found exception for the given ManagementOfChange identity.</summary>
    public ManagementOfChangeNotFoundException(ManagementOfChangeId id, CorrelationId correlationId, CausationId causationId)
        : base(
            "ManagementOfChange with id '{id}' was not found.",
            "ManagementOfChange.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}

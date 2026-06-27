// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.ManagementOfChange
//  Layer:          Domain
//  File:           ManagementOfChangeInvalidStatusTransitionException.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.ManagementOfChange.Exceptions;

/// <summary>Thrown when a <see cref="ManagementOfChange"/> lifecycle transition is not permitted.</summary>
public sealed class ManagementOfChangeInvalidStatusTransitionException : ManagementOfChangeDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public ManagementOfChangeInvalidStatusTransitionException(
        ManagementOfChangeId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            "Cannot transition 'ManagementOfChange' {id} from '{from}' to '{to}'.",
            "ManagementOfChange.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}

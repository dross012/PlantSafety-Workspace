// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Contractor
//  Layer:          Domain
//  File:           ContractorInvalidStatusTransitionException.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.Contractor.Exceptions;

/// <summary>Thrown when a <see cref="Contractor"/> lifecycle transition is not permitted.</summary>
public sealed class ContractorInvalidStatusTransitionException : ContractorDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public ContractorInvalidStatusTransitionException(
        ContractorId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            "Cannot transition 'Contractor' {id} from '{from}' to '{to}'.",
            "Contractor.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}

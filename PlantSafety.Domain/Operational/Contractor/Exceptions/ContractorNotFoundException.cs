// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Contractor
//  Layer:          Domain
//  File:           ContractorNotFoundException.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.Contractor.Exceptions;

/// <summary>Thrown when a <see cref="Contractor"/> aggregate lookup finds no match.</summary>
public sealed class ContractorNotFoundException : ContractorDomainException
{
    /// <summary>Creates a not-found exception for the given Contractor identity.</summary>
    public ContractorNotFoundException(ContractorId id, CorrelationId correlationId, CausationId causationId)
        : base(
            "Contractor with id '{id}' was not found.",
            "Contractor.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}

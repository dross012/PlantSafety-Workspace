// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorVisitor
//  Layer:          Domain
//  File:           ContractorVisitorInvalidStatusTransitionException.cs
//  Created:        2026-08-02
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Thrown when a ContractorVisitor lifecycle transition is not permitted. Phase 4
//                  full build-out. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorVisitor.Exceptions;

/// <summary>Thrown when a ContractorVisitor lifecycle transition is not permitted.</summary>
public sealed class ContractorVisitorInvalidStatusTransitionException : ContractorVisitorDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public ContractorVisitorInvalidStatusTransitionException(
        Guid id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition ContractorVisitor {id} from '{from}' to '{to}'.",
            "ContractorVisitor.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}

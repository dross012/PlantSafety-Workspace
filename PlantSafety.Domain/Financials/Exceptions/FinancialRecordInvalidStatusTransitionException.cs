// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Financials
//  Layer:          Domain
//  File:           FinancialRecordInvalidStatusTransitionException.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Thrown when a FinancialRecord lifecycle transition is not permitted. Mirrors
//                  PlantSafety.Domain.Compliance.Auditing.Exceptions
//                  .AuditingInvalidStatusTransitionException.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Financials.Exceptions;

/// <summary>Thrown when a <see cref="Aggregates.FinancialRecord"/> lifecycle transition is not permitted.</summary>
public sealed class FinancialRecordInvalidStatusTransitionException : FinancialRecordDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public FinancialRecordInvalidStatusTransitionException(
        FinancialsId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition 'FinancialRecord' {id} from '{from}' to '{to}'.",
            "Financials.FinancialRecord.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}

// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Financials
//  Layer:          Domain
//  File:           FinancialRecordDomainException.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Abstract base for all domain exceptions raised by the FinancialRecord
//                  aggregate. Mirrors
//                  PlantSafety.Domain.Compliance.Auditing.Exceptions.AuditingDomainException.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Financials.Exceptions;

/// <summary>Abstract base for all domain exceptions raised by the FinancialRecord aggregate.</summary>
public abstract class FinancialRecordDomainException : DomainException
{
    /// <summary>Initialises with canonical SIEM exception fields.</summary>
    protected FinancialRecordDomainException(
        string message,
        string errorCode,
        string errorCategory,
        string severity,
        CorrelationId correlationId,
        CausationId causationId)
        : base(message, errorCode, errorCategory, severity, correlationId, causationId)
    {
    }
}

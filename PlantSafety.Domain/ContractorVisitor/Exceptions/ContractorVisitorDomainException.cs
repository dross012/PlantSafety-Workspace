// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorVisitor
//  Layer:          Domain
//  File:           ContractorVisitorDomainException.cs
//  Created:        2026-08-02
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Abstract base for all domain exceptions raised in the ContractorVisitor module.
//                  Phase 4 full build-out. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorVisitor.Exceptions;

/// <summary>Abstract base for all domain exceptions raised in the ContractorVisitor module.</summary>
public abstract class ContractorVisitorDomainException : DomainException
{
    /// <summary>Initialises with canonical SIEM exception fields.</summary>
    protected ContractorVisitorDomainException(
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

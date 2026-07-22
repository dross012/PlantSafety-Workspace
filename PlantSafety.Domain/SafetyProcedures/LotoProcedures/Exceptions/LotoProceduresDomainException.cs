// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.LotoProcedures
//  Layer:          Domain
//  File:           LotoProceduresDomainException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Abstract base for all domain exceptions raised in the LotoProcedures module.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.LotoProcedures.Exceptions;

/// <summary>Abstract base for all domain exceptions raised in the LotoProcedures module.</summary>
public abstract class LotoProceduresDomainException : DomainException
{
    /// <summary>Initialises with canonical SIEM exception fields.</summary>
    protected LotoProceduresDomainException(
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
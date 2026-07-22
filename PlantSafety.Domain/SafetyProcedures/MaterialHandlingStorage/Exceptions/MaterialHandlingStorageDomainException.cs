// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.MaterialHandlingStorage
//  Layer:          Domain
//  File:           MaterialHandlingStorageDomainException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Abstract base for all domain exceptions raised in the MaterialHandlingStorage module.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.MaterialHandlingStorage.Exceptions;

/// <summary>Abstract base for all domain exceptions raised in the MaterialHandlingStorage module.</summary>
public abstract class MaterialHandlingStorageDomainException : DomainException
{
    /// <summary>Initialises with canonical SIEM exception fields.</summary>
    protected MaterialHandlingStorageDomainException(
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
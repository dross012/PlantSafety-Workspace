// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety.PermitToWork
//  Layer:          Domain
//  File:           PermitToWorkDomainException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Abstract base for all domain exceptions raised in the PermitToWork module.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorSafety.PermitToWork.Exceptions;

/// <summary>Abstract base for all domain exceptions raised in the PermitToWork module.</summary>
public abstract class PermitToWorkDomainException : DomainException
{
    /// <summary>Initialises with canonical SIEM exception fields.</summary>
    protected PermitToWorkDomainException(
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
// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Classification
//  Layer:          Domain
//  File:           HazmatMaterialDomainException.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Abstract base for all domain exceptions raised in the PHMSA Hazmat
//                  Classification module.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.PHMSA.Classification.Exceptions;

/// <summary>Abstract base for all domain exceptions raised in the PHMSA Hazmat Classification module.</summary>
public abstract class HazmatMaterialDomainException : DomainException
{
    /// <summary>Initialises with canonical SIEM exception fields.</summary>
    protected HazmatMaterialDomainException(
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

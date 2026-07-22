// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.ErgonomicsWorkstationDesign
//  Layer:          Domain
//  File:           ErgonomicsWorkstationDesignDomainException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Abstract base for all domain exceptions raised in the ErgonomicsWorkstationDesign module.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.ErgonomicsWorkstationDesign.Exceptions;

/// <summary>Abstract base for all domain exceptions raised in the ErgonomicsWorkstationDesign module.</summary>
public abstract class ErgonomicsWorkstationDesignDomainException : DomainException
{
    /// <summary>Initialises with canonical SIEM exception fields.</summary>
    protected ErgonomicsWorkstationDesignDomainException(
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
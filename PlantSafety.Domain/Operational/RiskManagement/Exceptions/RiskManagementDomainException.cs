// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.RiskManagement
//  Layer:          Domain
//  File:           RiskManagementDomainException.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.RiskManagement.Exceptions;

/// <summary>Abstract base for all domain exceptions raised in the RiskManagement module.</summary>
public abstract class RiskManagementDomainException : DomainException
{
    /// <summary>Initialises with canonical SIEM exception fields.</summary>
    protected RiskManagementDomainException(
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

// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.LocalEmergencyCoordination
//  Layer:          Domain
//  File:           LocalEmergencyCoordinationDomainException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Abstract base for all domain exceptions raised in the LocalEmergencyCoordination module.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Exceptions;

/// <summary>Abstract base for all domain exceptions raised in the LocalEmergencyCoordination module.</summary>
public abstract class LocalEmergencyCoordinationDomainException : DomainException
{
    /// <summary>Initialises with canonical SIEM exception fields.</summary>
    protected LocalEmergencyCoordinationDomainException(
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

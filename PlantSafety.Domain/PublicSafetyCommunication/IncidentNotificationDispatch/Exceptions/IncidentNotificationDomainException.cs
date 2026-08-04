// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PublicSafetyCommunication.IncidentNotificationDispatch
//  Layer:          Domain
//  File:           IncidentNotificationDomainException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Abstract base for all domain exceptions raised in the IncidentNotificationDispatch
//                  sub-module. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PublicSafetyCommunication.IncidentNotificationDispatch.Exceptions;

/// <summary>Abstract base for all domain exceptions raised in the IncidentNotificationDispatch sub-module.</summary>
public abstract class IncidentNotificationDomainException : DomainException
{
    /// <summary>Initialises with canonical SIEM exception fields.</summary>
    protected IncidentNotificationDomainException(
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

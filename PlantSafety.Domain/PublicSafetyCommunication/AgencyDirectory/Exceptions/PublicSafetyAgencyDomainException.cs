// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PublicSafetyCommunication.AgencyDirectory
//  Layer:          Domain
//  File:           PublicSafetyAgencyDomainException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Abstract base for all domain exceptions raised in the AgencyDirectory
//                  sub-module. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PublicSafetyCommunication.AgencyDirectory.Exceptions;

/// <summary>Abstract base for all domain exceptions raised in the AgencyDirectory sub-module.</summary>
public abstract class PublicSafetyAgencyDomainException : DomainException
{
    /// <summary>Initialises with canonical SIEM exception fields.</summary>
    protected PublicSafetyAgencyDomainException(
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

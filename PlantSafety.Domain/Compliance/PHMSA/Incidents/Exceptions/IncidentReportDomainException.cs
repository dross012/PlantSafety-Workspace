// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Incidents
//  Layer:          Domain
//  File:           IncidentReportDomainException.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Abstract base for all domain exceptions raised in the PHMSA Incidents module.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.PHMSA.Incidents.Exceptions;

/// <summary>Abstract base for all domain exceptions raised in the PHMSA Incidents module.</summary>
public abstract class IncidentReportDomainException : DomainException
{
    /// <summary>Initialises with canonical SIEM exception fields.</summary>
    protected IncidentReportDomainException(
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

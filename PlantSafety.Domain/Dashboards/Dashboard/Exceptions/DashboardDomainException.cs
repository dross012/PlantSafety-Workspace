// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Dashboards.Dashboard
//  Layer:          Domain
//  File:           DashboardDomainException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Abstract base for all domain exceptions raised in the Dashboards.Dashboard
//                  module.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Dashboards.Dashboard.Exceptions;

/// <summary>Abstract base for all domain exceptions raised in the Dashboards.Dashboard module.</summary>
public abstract class DashboardDomainException : DomainException
{
    /// <summary>Initialises with canonical SIEM exception fields.</summary>
    protected DashboardDomainException(
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

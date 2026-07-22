// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Dashboards.Widget
//  Layer:          Domain
//  File:           WidgetDomainException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Abstract base for all domain exceptions raised in the Dashboards.Widget module.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Dashboards.Widget.Exceptions;

/// <summary>Abstract base for all domain exceptions raised in the Dashboards.Widget module.</summary>
public abstract class WidgetDomainException : DomainException
{
    /// <summary>Initialises with canonical SIEM exception fields.</summary>
    protected WidgetDomainException(
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

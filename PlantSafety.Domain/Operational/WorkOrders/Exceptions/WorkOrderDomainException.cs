// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.WorkOrders
//  Layer:          Domain
//  File:           WorkOrderDomainException.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Abstract base for all domain exceptions raised in the Operational.WorkOrders
//                  module. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.WorkOrders.Exceptions;

/// <summary>Abstract base for all domain exceptions raised in the Operational.WorkOrders module.</summary>
public abstract class WorkOrderDomainException : DomainException
{
    /// <summary>Initialises with canonical SIEM exception fields.</summary>
    protected WorkOrderDomainException(
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

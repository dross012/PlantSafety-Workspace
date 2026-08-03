// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.WorkOrders
//  Layer:          Domain
//  File:           PartDomainException.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Abstract base for all domain exceptions raised for the Part (inventory)
//                  aggregate. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.WorkOrders.Exceptions;

/// <summary>Abstract base for all domain exceptions raised for the Part aggregate.</summary>
public abstract class PartDomainException : DomainException
{
    /// <summary>Initialises with canonical SIEM exception fields.</summary>
    protected PartDomainException(
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

// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin.EvacuationModeling.EvacuationBottlenecks
//  Layer:          Domain
//  File:           EvacuationBottleneckDomainException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Abstract base for all domain exceptions raised in the EvacuationBottlenecks
//                  module. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationBottlenecks.Exceptions;

/// <summary>Abstract base for all domain exceptions raised in the EvacuationBottlenecks module.</summary>
public abstract class EvacuationBottleneckDomainException : DomainException
{
    /// <summary>Initialises with canonical SIEM exception fields.</summary>
    protected EvacuationBottleneckDomainException(
        string message, string errorCode, string errorCategory, string severity,
        CorrelationId correlationId, CausationId causationId)
        : base(message, errorCode, errorCategory, severity, correlationId, causationId)
    {
    }
}

// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.PMScheduling
//  Layer:          Domain
//  File:           PMSchedulingDomainException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Abstract base for all domain exceptions raised in the PMScheduling sub-module.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.PMScheduling.Exceptions;

/// <summary>Abstract base for all domain exceptions raised in the PMScheduling module.</summary>
public abstract class PMSchedulingDomainException : DomainException
{
    /// <summary>Initialises with canonical SIEM exception fields.</summary>
    protected PMSchedulingDomainException(
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
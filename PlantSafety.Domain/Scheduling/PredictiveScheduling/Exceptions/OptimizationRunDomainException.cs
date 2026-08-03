// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.PredictiveScheduling
//  Layer:          Domain
//  File:           OptimizationRunDomainException.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Abstract base for all domain exceptions raised for the OptimizationRun
//                  aggregate. Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.Domain.Common.Exceptions;

namespace PlantSafety.Domain.Scheduling.PredictiveScheduling.Exceptions;

/// <summary>Abstract base for all domain exceptions raised for the OptimizationRun aggregate.</summary>
public abstract class OptimizationRunDomainException : DomainException
{
    /// <summary>Initialises with canonical SIEM exception fields.</summary>
    protected OptimizationRunDomainException(
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

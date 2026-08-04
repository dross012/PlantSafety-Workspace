// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.LoadSecurementChecks
//  Layer:          Domain
//  File:           LoadSecurementCheckDomainException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Abstract base for all domain exceptions raised in the LoadSecurementChecks module.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.LoadSecurementChecks.Exceptions;

/// <summary>Abstract base for all domain exceptions raised in the LoadSecurementChecks module.</summary>
public abstract class LoadSecurementCheckDomainException : DomainException
{
    /// <summary>Initialises with canonical SIEM exception fields.</summary>
    protected LoadSecurementCheckDomainException(
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

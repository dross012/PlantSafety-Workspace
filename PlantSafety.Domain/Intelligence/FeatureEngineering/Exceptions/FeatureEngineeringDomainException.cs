// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.FeatureEngineering
//  Layer:          Domain
//  File:           FeatureEngineeringDomainException.cs
//  Created:        2026-08-20
//  Author:         PlantSafety Platform Team (Claude Code file-and-folder-agent)
//  SchemaVersion:  2026.1
//  Description:    Abstract base for all domain exceptions raised in the
//                  Intelligence.FeatureEngineering module. Mirrors
//                  PlantSafety.Domain.Intelligence.DataLake.Exceptions.DataLakeActivityDomainException.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Intelligence.FeatureEngineering.Exceptions;

/// <summary>Abstract base for all domain exceptions raised in the Intelligence.FeatureEngineering module.</summary>
public abstract class FeatureEngineeringDomainException : DomainException
{
    /// <summary>Initialises with canonical SIEM exception fields.</summary>
    protected FeatureEngineeringDomainException(
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

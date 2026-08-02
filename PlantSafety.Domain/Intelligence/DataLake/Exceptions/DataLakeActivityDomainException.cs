// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.DataLake
//  Layer:          Domain
//  File:           DataLakeActivityDomainException.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code refactoring-agent)
//  SchemaVersion:  2026.1
//  Description:    Abstract base for all domain exceptions raised in the Intelligence.DataLake
//                  module. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Intelligence.DataLake.Exceptions;

/// <summary>Abstract base for all domain exceptions raised in the Intelligence.DataLake module.</summary>
public abstract class DataLakeActivityDomainException : DomainException
{
    /// <summary>Initialises with canonical SIEM exception fields.</summary>
    protected DataLakeActivityDomainException(
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

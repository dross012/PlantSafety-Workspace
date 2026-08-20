// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.ModelRegistry
//  Layer:          Domain
//  File:           ModelRegistryEntryDomainException.cs
//  Created:        2026-08-20
//  Author:         PlantSafety Platform Team (Claude Code file-and-folder-agent)
//  SchemaVersion:  2026.1
//  Description:    Abstract base for all domain exceptions raised in the Intelligence.ModelRegistry
//                  module. Mirrors
//                  PlantSafety.Domain.Intelligence.DataLake.Exceptions.DataLakeActivityDomainException.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Intelligence.ModelRegistry.Exceptions;

/// <summary>Abstract base for all domain exceptions raised in the Intelligence.ModelRegistry module.</summary>
public abstract class ModelRegistryEntryDomainException : DomainException
{
    /// <summary>Initialises with canonical SIEM exception fields.</summary>
    protected ModelRegistryEntryDomainException(
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

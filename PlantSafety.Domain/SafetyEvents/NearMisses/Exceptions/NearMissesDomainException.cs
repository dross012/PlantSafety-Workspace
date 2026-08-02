// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.NearMisses
//  Layer:          Domain
//  File:           NearMissesDomainException.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Abstract base for all domain exceptions raised in the NearMisses module.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.NearMisses.Exceptions;

/// <summary>Abstract base for all domain exceptions raised in the NearMisses module.</summary>
public abstract class NearMissesDomainException : DomainException
{
    /// <summary>Initialises with canonical SIEM exception fields.</summary>
    protected NearMissesDomainException(
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

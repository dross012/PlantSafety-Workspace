// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights
//  Layer:          Domain
//  File:           GlobalSafetyInsightsDomainException.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §2 Multi-Site Safety Intelligence Network)
//  SchemaVersion:  2026.1
//  Description:    Base domain exception for the GlobalSafetyInsights module (Handbook §2.14).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.GlobalSafetyInsights.Exceptions;

/// <summary>Base domain exception for the GlobalSafetyInsights module.</summary>
public abstract class GlobalSafetyInsightsDomainException : Exception
{
    /// <summary>Correlation identifier propagated from the triggering command/event.</summary>
    public Guid CorrelationId { get; }

    /// <summary>Causation identifier propagated from the triggering command/event.</summary>
    public Guid CausationId { get; }

    /// <summary>Initializes the base exception with the required causality metadata.</summary>
    protected GlobalSafetyInsightsDomainException(string message, Guid correlationId, Guid causationId)
        : base(message)
    {
        CorrelationId = correlationId;
        CausationId = causationId;
    }
}

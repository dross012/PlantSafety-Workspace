// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AiSafetyIntelligence
//  Layer:          Domain
//  File:           AiSafetyIntelligenceInvalidStatusTransitionException.cs
//  Created:        2026-08-02
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Thrown when an AiSafetyIntelligence lifecycle transition is not permitted.
//                  Phase 4 full build-out. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AiSafetyIntelligence.Exceptions;

/// <summary>Thrown when an AiSafetyIntelligence lifecycle transition is not permitted.</summary>
public sealed class AiSafetyIntelligenceInvalidStatusTransitionException : AiSafetyIntelligenceDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public AiSafetyIntelligenceInvalidStatusTransitionException(
        Guid id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition AiSafetyIntelligence {id} from '{from}' to '{to}'.",
            "AiSafetyIntelligence.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}

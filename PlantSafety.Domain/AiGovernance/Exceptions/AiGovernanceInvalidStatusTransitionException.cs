// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AiGovernance
//  Layer:          Domain
//  File:           AiGovernanceInvalidStatusTransitionException.cs
//  Created:        2026-08-02
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Thrown when an AiGovernance lifecycle transition is not permitted. Phase 4 full
//                  build-out. Mirrors
//                  PlantSafety.Domain.Compliance.Auditing.Exceptions.AuditingInvalidStatusTransitionException.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AiGovernance.Exceptions;

/// <summary>Thrown when an AiGovernance lifecycle transition is not permitted.</summary>
public sealed class AiGovernanceInvalidStatusTransitionException : AiGovernanceDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public AiGovernanceInvalidStatusTransitionException(
        Guid id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition AiGovernance {id} from '{from}' to '{to}'.",
            "AiGovernance.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}

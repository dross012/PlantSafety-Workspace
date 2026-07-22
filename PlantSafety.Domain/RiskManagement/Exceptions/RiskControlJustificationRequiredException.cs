// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         RiskManagement
//  Layer:          Domain
//  File:           RiskControlJustificationRequiredException.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a control measure below Elimination/Substitution on the Hierarchy of
//                  Controls is applied without documented justification, per Full Risk Management
//                  Process PHASE 2 Step 5: the system "can enforce this hierarchy by requiring
//                  justification when lower-level controls are used".
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.Domain.RiskManagement.ValueObjects.Ids;

namespace PlantSafety.Domain.RiskManagement.Exceptions;

/// <summary>Thrown when a lower-effectiveness control measure is applied without justification.</summary>
public sealed class RiskControlJustificationRequiredException : RiskDomainException
{
    /// <summary>Creates an exception for the given risk and control hierarchy level.</summary>
    public RiskControlJustificationRequiredException(
        RiskId id, string hierarchyLevel,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Applying a '{hierarchyLevel}' control on Risk {id} requires documented justification because higher-effectiveness controls (Elimination/Substitution) were not used.",
            "Risk.ControlJustificationRequired",
            "Validation",
            "Medium",
            correlationId,
            causationId)
    {
    }
}

// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.RulesEngine
//  Layer:          Domain
//  File:           IComplianceEvaluationFactory.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Factory contract for constructing ComplianceEvaluation aggregates.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Compliance.RulesEngine.Aggregates;

namespace PlantSafety.Domain.Compliance.RulesEngine.Interfaces;

/// <summary>Factory contract for constructing ComplianceEvaluation aggregates.</summary>
public interface IComplianceEvaluationFactory
{
    /// <summary>Starts a new ComplianceEvaluation run.</summary>
    ComplianceEvaluation Start(
        Guid tenantId, Guid? siteId, string targetEntityType, Guid targetEntityId,
        UserId triggeredBy, UtcTimestamp startedAtUtc, PerformedByActor actor,
        CorrelationId correlationId, CausationId causationId);
}

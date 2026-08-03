// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.RulesEngine
//  Layer:          Domain
//  File:           ComplianceEvaluationFactory.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing ComplianceEvaluation
//                  aggregates. Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Compliance.RulesEngine.Aggregates;
using PlantSafety.Domain.Compliance.RulesEngine.Interfaces;

namespace PlantSafety.Domain.Compliance.RulesEngine.Factories;

/// <summary>Default factory implementation for constructing ComplianceEvaluation aggregates.</summary>
public sealed class ComplianceEvaluationFactory : IComplianceEvaluationFactory
{
    /// <inheritdoc/>
    public ComplianceEvaluation Start(
        Guid tenantId, Guid? siteId, string targetEntityType, Guid targetEntityId,
        UserId triggeredBy, UtcTimestamp startedAtUtc, PerformedByActor actor,
        CorrelationId correlationId, CausationId causationId)
        => ComplianceEvaluation.Start(tenantId, siteId, targetEntityType, targetEntityId, triggeredBy, startedAtUtc, actor, correlationId, causationId);
}

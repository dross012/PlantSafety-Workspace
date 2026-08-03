// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.RulesEngine
//  Layer:          Domain
//  File:           ComplianceEvaluationNotFoundException.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a ComplianceEvaluation aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.Domain.Compliance.RulesEngine.ValueObjects.Ids;

namespace PlantSafety.Domain.Compliance.RulesEngine.Exceptions;

/// <summary>Thrown when a ComplianceEvaluation aggregate lookup finds no match.</summary>
public sealed class ComplianceEvaluationNotFoundException : ComplianceRuleEngineDomainException
{
    /// <summary>Creates a not-found exception for the given ComplianceEvaluation identity.</summary>
    public ComplianceEvaluationNotFoundException(ComplianceEvaluationId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"ComplianceEvaluation with id '{id}' was not found.",
            "ComplianceEvaluation.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}

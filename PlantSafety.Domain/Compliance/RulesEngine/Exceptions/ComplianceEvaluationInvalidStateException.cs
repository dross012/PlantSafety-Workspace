// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.RulesEngine
//  Layer:          Domain
//  File:           ComplianceEvaluationInvalidStateException.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Thrown when an operation is attempted against a ComplianceEvaluation that is
//                  not in a valid lifecycle state for it (e.g. recording a rule result after
//                  completion). Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.Domain.Compliance.RulesEngine.ValueObjects.Ids;

namespace PlantSafety.Domain.Compliance.RulesEngine.Exceptions;

/// <summary>Thrown when a ComplianceEvaluation operation is attempted from an invalid lifecycle state.</summary>
public sealed class ComplianceEvaluationInvalidStateException : ComplianceRuleEngineDomainException
{
    /// <summary>Creates an invalid-state exception for the given ComplianceEvaluation identity.</summary>
    public ComplianceEvaluationInvalidStateException(ComplianceEvaluationId id, string currentStatus, string attemptedOperation,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot perform '{attemptedOperation}' on ComplianceEvaluation {id} while status is '{currentStatus}'.",
            "ComplianceEvaluation.InvalidState",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}

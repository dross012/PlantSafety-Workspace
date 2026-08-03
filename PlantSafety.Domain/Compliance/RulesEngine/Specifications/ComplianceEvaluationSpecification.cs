// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.RulesEngine
//  Layer:          Domain
//  File:           ComplianceEvaluationSpecification.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for ComplianceEvaluation aggregate predicate
//                  expressions. Additive-only evolution.
// ============================================================================
using System.Linq.Expressions;
using PlantSafety.Domain.Compliance.RulesEngine.Aggregates;

namespace PlantSafety.Domain.Compliance.RulesEngine.Specifications;

/// <summary>Abstract base specification for ComplianceEvaluation aggregate predicate expressions.</summary>
public abstract class ComplianceEvaluationSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<ComplianceEvaluation, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<ComplianceEvaluation, bool> ToPredicate() => ToExpression().Compile();
}

/// <summary>Matches ComplianceEvaluation aggregates for a specific target entity.</summary>
public sealed class ComplianceEvaluationForTargetSpecification(string targetEntityType, Guid targetEntityId) : ComplianceEvaluationSpecification
{
    /// <inheritdoc/>
    public override Expression<Func<ComplianceEvaluation, bool>> ToExpression()
        => e => e.TargetEntityType == targetEntityType && e.TargetEntityId == targetEntityId;
}

/// <summary>Matches ComplianceEvaluation aggregates with a NonCompliant outcome.</summary>
public sealed class NonCompliantComplianceEvaluationSpecification : ComplianceEvaluationSpecification
{
    /// <inheritdoc/>
    public override Expression<Func<ComplianceEvaluation, bool>> ToExpression()
        => e => e.Status == Enums.ComplianceEvaluationStatus.NonCompliant;
}

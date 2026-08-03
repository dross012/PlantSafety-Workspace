// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.PredictiveScheduling
//  Layer:          Domain
//  File:           OptimizationRunSpecification.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for OptimizationRun aggregate predicate
//                  expressions, plus concrete specifications for by-status and date-range
//                  filtering (used by the optimization run history query).
//                  Additive-only evolution.
// ============================================================================
using System.Linq.Expressions;
using PlantSafety.Domain.Scheduling.PredictiveScheduling.Aggregates;
using PlantSafety.Domain.Scheduling.PredictiveScheduling.Enums;

namespace PlantSafety.Domain.Scheduling.PredictiveScheduling.Specifications;

/// <summary>Abstract base specification for OptimizationRun aggregate predicate expressions.</summary>
public abstract class OptimizationRunSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<OptimizationRun, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<OptimizationRun, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public OptimizationRunSpecification And(OptimizationRunSpecification other) => new AndOptimizationRunSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public OptimizationRunSpecification Or(OptimizationRunSpecification other) => new OrOptimizationRunSpecification(this, other);
}

/// <summary>Matches every OptimizationRun (no filtering).</summary>
public sealed class AllOptimizationRunsSpecification : OptimizationRunSpecification
{
    /// <inheritdoc/>
    public override Expression<Func<OptimizationRun, bool>> ToExpression() => _ => true;
}

/// <summary>Matches OptimizationRuns currently in the given <see cref="OptimizationRunStatus"/>.</summary>
public sealed class OptimizationRunByStatusSpecification(OptimizationRunStatus status) : OptimizationRunSpecification
{
    /// <inheritdoc/>
    public override Expression<Func<OptimizationRun, bool>> ToExpression()
        => run => run.Status == status;
}

/// <summary>Matches OptimizationRuns requested within the given inclusive UTC date range.</summary>
public sealed class OptimizationRunByDateRangeSpecification(DateTime fromUtc, DateTime toUtc) : OptimizationRunSpecification
{
    /// <inheritdoc/>
    public override Expression<Func<OptimizationRun, bool>> ToExpression()
        => run => run.RequestedAtUtc.Value >= fromUtc && run.RequestedAtUtc.Value <= toUtc;
}

internal sealed class AndOptimizationRunSpecification(OptimizationRunSpecification left, OptimizationRunSpecification right) : OptimizationRunSpecification
{
    public override Expression<Func<OptimizationRun, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<OptimizationRun, bool>>(body, param);
    }
}

internal sealed class OrOptimizationRunSpecification(OptimizationRunSpecification left, OptimizationRunSpecification right) : OptimizationRunSpecification
{
    public override Expression<Func<OptimizationRun, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<OptimizationRun, bool>>(body, param);
    }
}

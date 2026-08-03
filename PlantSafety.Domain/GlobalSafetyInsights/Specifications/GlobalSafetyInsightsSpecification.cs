// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights
//  Layer:          Domain
//  File:           GlobalSafetyInsightsSpecification.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §2 Multi-Site Safety Intelligence Network)
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for GlobalSafetyInsightsAggregate predicate
//                  expressions, mirroring RiskManagement.Specifications.RiskSpecification's
//                  And/Or composition pattern. Additive-only evolution.
// ============================================================================
using System.Linq.Expressions;
using PlantSafety.Domain.GlobalSafetyInsights.Aggregates;

namespace PlantSafety.Domain.GlobalSafetyInsights.Specifications;

/// <summary>Abstract base specification for GlobalSafetyInsightsAggregate predicate expressions.</summary>
public abstract class GlobalSafetyInsightsSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<GlobalSafetyInsightsAggregate, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<GlobalSafetyInsightsAggregate, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public GlobalSafetyInsightsSpecification And(GlobalSafetyInsightsSpecification other) => new AndGlobalSafetyInsightsSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public GlobalSafetyInsightsSpecification Or(GlobalSafetyInsightsSpecification other) => new OrGlobalSafetyInsightsSpecification(this, other);
}

/// <summary>Matches snapshots for a specific region code.</summary>
public sealed class ByRegionCodeSpecification(string regionCode) : GlobalSafetyInsightsSpecification
{
    /// <inheritdoc/>
    public override Expression<Func<GlobalSafetyInsightsAggregate, bool>> ToExpression()
        => agg => agg.Region.Code == regionCode;
}

/// <summary>Matches snapshots that recorded at least one anomaly.</summary>
public sealed class WithAnomaliesSpecification : GlobalSafetyInsightsSpecification
{
    /// <inheritdoc/>
    public override Expression<Func<GlobalSafetyInsightsAggregate, bool>> ToExpression()
        => agg => agg.Anomalies.Count > 0;
}

internal sealed class AndGlobalSafetyInsightsSpecification(GlobalSafetyInsightsSpecification left, GlobalSafetyInsightsSpecification right) : GlobalSafetyInsightsSpecification
{
    public override Expression<Func<GlobalSafetyInsightsAggregate, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<GlobalSafetyInsightsAggregate, bool>>(body, param);
    }
}

internal sealed class OrGlobalSafetyInsightsSpecification(GlobalSafetyInsightsSpecification left, GlobalSafetyInsightsSpecification right) : GlobalSafetyInsightsSpecification
{
    public override Expression<Func<GlobalSafetyInsightsAggregate, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<GlobalSafetyInsightsAggregate, bool>>(body, param);
    }
}

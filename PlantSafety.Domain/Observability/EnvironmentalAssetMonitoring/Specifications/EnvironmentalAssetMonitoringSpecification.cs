// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.EnvironmentalAssetMonitoring
//  Layer:          Domain
//  File:           EnvironmentalAssetMonitoringSpecification.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for EnvironmentalAssetMonitoring aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.EnvironmentalAssetMonitoring.Specifications;

/// <summary>Abstract base specification for <see cref="EnvironmentalAssetMonitoring"/> aggregate predicate expressions.</summary>
public abstract class EnvironmentalAssetMonitoringSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.Observability.EnvironmentalAssetMonitoring.Aggregates.EnvironmentalAssetMonitoring, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.Observability.EnvironmentalAssetMonitoring.Aggregates.EnvironmentalAssetMonitoring, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public EnvironmentalAssetMonitoringSpecification And(EnvironmentalAssetMonitoringSpecification other) => new AndEnvironmentalAssetMonitoringSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public EnvironmentalAssetMonitoringSpecification Or(EnvironmentalAssetMonitoringSpecification other) => new OrEnvironmentalAssetMonitoringSpecification(this, other);
}

internal sealed class AndEnvironmentalAssetMonitoringSpecification(EnvironmentalAssetMonitoringSpecification left, EnvironmentalAssetMonitoringSpecification right) : EnvironmentalAssetMonitoringSpecification
{
    public override Expression<Func<PlantSafety.Domain.Observability.EnvironmentalAssetMonitoring.Aggregates.EnvironmentalAssetMonitoring, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.Observability.EnvironmentalAssetMonitoring.Aggregates.EnvironmentalAssetMonitoring, bool>>(body, param);
    }
}

internal sealed class OrEnvironmentalAssetMonitoringSpecification(EnvironmentalAssetMonitoringSpecification left, EnvironmentalAssetMonitoringSpecification right) : EnvironmentalAssetMonitoringSpecification
{
    public override Expression<Func<PlantSafety.Domain.Observability.EnvironmentalAssetMonitoring.Aggregates.EnvironmentalAssetMonitoring, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.Observability.EnvironmentalAssetMonitoring.Aggregates.EnvironmentalAssetMonitoring, bool>>(body, param);
    }
}
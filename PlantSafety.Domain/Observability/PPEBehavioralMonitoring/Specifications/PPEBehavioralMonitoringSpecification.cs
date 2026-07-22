// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.PPEBehavioralMonitoring
//  Layer:          Domain
//  File:           PPEBehavioralMonitoringSpecification.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for PPEBehavioralMonitoring aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.PPEBehavioralMonitoring.Specifications;

/// <summary>Abstract base specification for <see cref="PPEBehavioralMonitoring"/> aggregate predicate expressions.</summary>
public abstract class PPEBehavioralMonitoringSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.Observability.PPEBehavioralMonitoring.Aggregates.PPEBehavioralMonitoring, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.Observability.PPEBehavioralMonitoring.Aggregates.PPEBehavioralMonitoring, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public PPEBehavioralMonitoringSpecification And(PPEBehavioralMonitoringSpecification other) => new AndPPEBehavioralMonitoringSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public PPEBehavioralMonitoringSpecification Or(PPEBehavioralMonitoringSpecification other) => new OrPPEBehavioralMonitoringSpecification(this, other);
}

internal sealed class AndPPEBehavioralMonitoringSpecification(PPEBehavioralMonitoringSpecification left, PPEBehavioralMonitoringSpecification right) : PPEBehavioralMonitoringSpecification
{
    public override Expression<Func<PlantSafety.Domain.Observability.PPEBehavioralMonitoring.Aggregates.PPEBehavioralMonitoring, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.Observability.PPEBehavioralMonitoring.Aggregates.PPEBehavioralMonitoring, bool>>(body, param);
    }
}

internal sealed class OrPPEBehavioralMonitoringSpecification(PPEBehavioralMonitoringSpecification left, PPEBehavioralMonitoringSpecification right) : PPEBehavioralMonitoringSpecification
{
    public override Expression<Func<PlantSafety.Domain.Observability.PPEBehavioralMonitoring.Aggregates.PPEBehavioralMonitoring, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.Observability.PPEBehavioralMonitoring.Aggregates.PPEBehavioralMonitoring, bool>>(body, param);
    }
}
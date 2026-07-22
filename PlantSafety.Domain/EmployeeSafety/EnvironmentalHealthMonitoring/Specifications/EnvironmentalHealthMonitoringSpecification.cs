// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.EnvironmentalHealthMonitoring
//  Layer:          Domain
//  File:           EnvironmentalHealthMonitoringSpecification.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for EnvironmentalHealthMonitoring aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.EnvironmentalHealthMonitoring.Specifications;

/// <summary>Abstract base specification for <see cref="EnvironmentalHealthMonitoring"/> aggregate predicate expressions.</summary>
public abstract class EnvironmentalHealthMonitoringSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.EmployeeSafety.EnvironmentalHealthMonitoring.Aggregates.EnvironmentalHealthMonitoring, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.EmployeeSafety.EnvironmentalHealthMonitoring.Aggregates.EnvironmentalHealthMonitoring, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public EnvironmentalHealthMonitoringSpecification And(EnvironmentalHealthMonitoringSpecification other) => new AndEnvironmentalHealthMonitoringSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public EnvironmentalHealthMonitoringSpecification Or(EnvironmentalHealthMonitoringSpecification other) => new OrEnvironmentalHealthMonitoringSpecification(this, other);
}

internal sealed class AndEnvironmentalHealthMonitoringSpecification(EnvironmentalHealthMonitoringSpecification left, EnvironmentalHealthMonitoringSpecification right) : EnvironmentalHealthMonitoringSpecification
{
    public override Expression<Func<PlantSafety.Domain.EmployeeSafety.EnvironmentalHealthMonitoring.Aggregates.EnvironmentalHealthMonitoring, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.EmployeeSafety.EnvironmentalHealthMonitoring.Aggregates.EnvironmentalHealthMonitoring, bool>>(body, param);
    }
}

internal sealed class OrEnvironmentalHealthMonitoringSpecification(EnvironmentalHealthMonitoringSpecification left, EnvironmentalHealthMonitoringSpecification right) : EnvironmentalHealthMonitoringSpecification
{
    public override Expression<Func<PlantSafety.Domain.EmployeeSafety.EnvironmentalHealthMonitoring.Aggregates.EnvironmentalHealthMonitoring, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.EmployeeSafety.EnvironmentalHealthMonitoring.Aggregates.EnvironmentalHealthMonitoring, bool>>(body, param);
    }
}
// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.MachineProcessMonitoring
//  Layer:          Domain
//  File:           MachineProcessMonitoringSpecification.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for MachineProcessMonitoring aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.MachineProcessMonitoring.Specifications;

/// <summary>Abstract base specification for <see cref="MachineProcessMonitoring"/> aggregate predicate expressions.</summary>
public abstract class MachineProcessMonitoringSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.Observability.MachineProcessMonitoring.Aggregates.MachineProcessMonitoring, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.Observability.MachineProcessMonitoring.Aggregates.MachineProcessMonitoring, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public MachineProcessMonitoringSpecification And(MachineProcessMonitoringSpecification other) => new AndMachineProcessMonitoringSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public MachineProcessMonitoringSpecification Or(MachineProcessMonitoringSpecification other) => new OrMachineProcessMonitoringSpecification(this, other);
}

internal sealed class AndMachineProcessMonitoringSpecification(MachineProcessMonitoringSpecification left, MachineProcessMonitoringSpecification right) : MachineProcessMonitoringSpecification
{
    public override Expression<Func<PlantSafety.Domain.Observability.MachineProcessMonitoring.Aggregates.MachineProcessMonitoring, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.Observability.MachineProcessMonitoring.Aggregates.MachineProcessMonitoring, bool>>(body, param);
    }
}

internal sealed class OrMachineProcessMonitoringSpecification(MachineProcessMonitoringSpecification left, MachineProcessMonitoringSpecification right) : MachineProcessMonitoringSpecification
{
    public override Expression<Func<PlantSafety.Domain.Observability.MachineProcessMonitoring.Aggregates.MachineProcessMonitoring, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.Observability.MachineProcessMonitoring.Aggregates.MachineProcessMonitoring, bool>>(body, param);
    }
}
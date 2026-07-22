// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.WorkOrderPriority
//  Layer:          Domain
//  File:           WorkOrderPrioritySpecification.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for WorkOrderPriority aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.WorkOrderPriority.Specifications;

/// <summary>Abstract base specification for <see cref="WorkOrderPriority"/> aggregate predicate expressions.</summary>
public abstract class WorkOrderPrioritySpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.Scheduling.WorkOrderPriority.Aggregates.WorkOrderPriority, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.Scheduling.WorkOrderPriority.Aggregates.WorkOrderPriority, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public WorkOrderPrioritySpecification And(WorkOrderPrioritySpecification other) => new AndWorkOrderPrioritySpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public WorkOrderPrioritySpecification Or(WorkOrderPrioritySpecification other) => new OrWorkOrderPrioritySpecification(this, other);
}

internal sealed class AndWorkOrderPrioritySpecification(WorkOrderPrioritySpecification left, WorkOrderPrioritySpecification right) : WorkOrderPrioritySpecification
{
    public override Expression<Func<PlantSafety.Domain.Scheduling.WorkOrderPriority.Aggregates.WorkOrderPriority, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.Scheduling.WorkOrderPriority.Aggregates.WorkOrderPriority, bool>>(body, param);
    }
}

internal sealed class OrWorkOrderPrioritySpecification(WorkOrderPrioritySpecification left, WorkOrderPrioritySpecification right) : WorkOrderPrioritySpecification
{
    public override Expression<Func<PlantSafety.Domain.Scheduling.WorkOrderPriority.Aggregates.WorkOrderPriority, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.Scheduling.WorkOrderPriority.Aggregates.WorkOrderPriority, bool>>(body, param);
    }
}
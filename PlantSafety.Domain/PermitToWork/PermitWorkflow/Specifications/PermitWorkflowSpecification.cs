// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PermitToWork.PermitWorkflow
//  Layer:          Domain
//  File:           PermitWorkflowSpecification.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for PermitWorkflow aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PermitToWork.PermitWorkflow.Specifications;

/// <summary>Abstract base specification for <see cref="PermitWorkflow"/> aggregate predicate expressions.</summary>
public abstract class PermitWorkflowSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.PermitToWork.PermitWorkflow.Aggregates.PermitWorkflow, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.PermitToWork.PermitWorkflow.Aggregates.PermitWorkflow, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public PermitWorkflowSpecification And(PermitWorkflowSpecification other) => new AndPermitWorkflowSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public PermitWorkflowSpecification Or(PermitWorkflowSpecification other) => new OrPermitWorkflowSpecification(this, other);
}

internal sealed class AndPermitWorkflowSpecification(PermitWorkflowSpecification left, PermitWorkflowSpecification right) : PermitWorkflowSpecification
{
    public override Expression<Func<PlantSafety.Domain.PermitToWork.PermitWorkflow.Aggregates.PermitWorkflow, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.PermitToWork.PermitWorkflow.Aggregates.PermitWorkflow, bool>>(body, param);
    }
}

internal sealed class OrPermitWorkflowSpecification(PermitWorkflowSpecification left, PermitWorkflowSpecification right) : PermitWorkflowSpecification
{
    public override Expression<Func<PlantSafety.Domain.PermitToWork.PermitWorkflow.Aggregates.PermitWorkflow, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.PermitToWork.PermitWorkflow.Aggregates.PermitWorkflow, bool>>(body, param);
    }
}
// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Domain
//  File:           WorkflowInstanceSpecification.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for WorkflowInstance aggregate predicate
//                  expressions, plus concrete specifications for pending-approvals-for-actor and
//                  by-state filtering.
//                  Additive-only evolution.
// ============================================================================
using System.Linq.Expressions;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.Domain.Workflow.Aggregates;
using PlantSafety.Domain.Workflow.Enums;

namespace PlantSafety.Domain.Workflow.Specifications;

/// <summary>Abstract base specification for WorkflowInstance aggregate predicate expressions.</summary>
public abstract class WorkflowInstanceSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<WorkflowInstance, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<WorkflowInstance, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public WorkflowInstanceSpecification And(WorkflowInstanceSpecification other) => new AndWorkflowInstanceSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public WorkflowInstanceSpecification Or(WorkflowInstanceSpecification other) => new OrWorkflowInstanceSpecification(this, other);
}

/// <summary>Matches WorkflowInstances currently in the given <see cref="WorkflowState"/>.</summary>
public sealed class WorkflowInstanceByStateSpecification(WorkflowState state) : WorkflowInstanceSpecification
{
    /// <inheritdoc/>
    public override Expression<Func<WorkflowInstance, bool>> ToExpression()
        => instance => instance.State == state;
}

/// <summary>Matches WorkflowInstances whose current step is a HumanApproval step assigned to the given user's role.</summary>
public sealed class PendingApprovalsForActorSpecification(UserId userId, IReadOnlySet<string> userRoles) : WorkflowInstanceSpecification
{
    /// <inheritdoc/>
    public override Expression<Func<WorkflowInstance, bool>> ToExpression()
        => instance => instance.State == WorkflowState.WaitingForApproval
            && instance.StepHistory.Any(s =>
                s.StepId == instance.CurrentStepId
                && s.Status == WorkflowStepStatus.Active
                && (s.AssignedActor.UserId != null
                    ? s.AssignedActor.UserId.Value.Value == userId.Value
                    : s.AssignedActor.RoleCode != null && userRoles.Contains(s.AssignedActor.RoleCode)));
}

internal sealed class AndWorkflowInstanceSpecification(WorkflowInstanceSpecification left, WorkflowInstanceSpecification right) : WorkflowInstanceSpecification
{
    public override Expression<Func<WorkflowInstance, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<WorkflowInstance, bool>>(body, param);
    }
}

internal sealed class OrWorkflowInstanceSpecification(WorkflowInstanceSpecification left, WorkflowInstanceSpecification right) : WorkflowInstanceSpecification
{
    public override Expression<Func<WorkflowInstance, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<WorkflowInstance, bool>>(body, param);
    }
}

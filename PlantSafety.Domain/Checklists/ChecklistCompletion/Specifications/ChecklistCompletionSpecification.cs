// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Checklists.ChecklistCompletion
//  Layer:          Domain
//  File:           ChecklistCompletionSpecification.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for ChecklistCompletion aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Checklists.ChecklistCompletion.Specifications;

/// <summary>Abstract base specification for <see cref="ChecklistCompletion"/> aggregate predicate expressions.</summary>
public abstract class ChecklistCompletionSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.Checklists.ChecklistCompletion.Aggregates.ChecklistCompletion, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.Checklists.ChecklistCompletion.Aggregates.ChecklistCompletion, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public ChecklistCompletionSpecification And(ChecklistCompletionSpecification other) => new AndChecklistCompletionSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public ChecklistCompletionSpecification Or(ChecklistCompletionSpecification other) => new OrChecklistCompletionSpecification(this, other);
}

internal sealed class AndChecklistCompletionSpecification(ChecklistCompletionSpecification left, ChecklistCompletionSpecification right) : ChecklistCompletionSpecification
{
    public override Expression<Func<PlantSafety.Domain.Checklists.ChecklistCompletion.Aggregates.ChecklistCompletion, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.Checklists.ChecklistCompletion.Aggregates.ChecklistCompletion, bool>>(body, param);
    }
}

internal sealed class OrChecklistCompletionSpecification(ChecklistCompletionSpecification left, ChecklistCompletionSpecification right) : ChecklistCompletionSpecification
{
    public override Expression<Func<PlantSafety.Domain.Checklists.ChecklistCompletion.Aggregates.ChecklistCompletion, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.Checklists.ChecklistCompletion.Aggregates.ChecklistCompletion, bool>>(body, param);
    }
}
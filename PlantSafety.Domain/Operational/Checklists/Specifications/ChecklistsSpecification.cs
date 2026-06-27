// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Checklists
//  Layer:          Domain
//  File:           ChecklistsSpecification.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.Checklists.Specifications;

/// <summary>Abstract base specification for <see cref="Checklists"/> aggregate predicate expressions.</summary>
public abstract class ChecklistsSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<Checklists, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<Checklists, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public ChecklistsSpecification And(ChecklistsSpecification other) => new AndChecklistsSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public ChecklistsSpecification Or(ChecklistsSpecification other) => new OrChecklistsSpecification(this, other);
}

internal sealed class AndChecklistsSpecification(ChecklistsSpecification left, ChecklistsSpecification right) : ChecklistsSpecification
{
    public override Expression<Func<Checklists, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<Checklists, bool>>(body, param);
    }
}

internal sealed class OrChecklistsSpecification(ChecklistsSpecification left, ChecklistsSpecification right) : ChecklistsSpecification
{
    public override Expression<Func<Checklists, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<Checklists, bool>>(body, param);
    }
}

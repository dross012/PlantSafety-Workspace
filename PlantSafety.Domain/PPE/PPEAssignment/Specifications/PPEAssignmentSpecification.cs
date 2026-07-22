// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPEAssignment
//  Layer:          Domain
//  File:           PPEAssignmentSpecification.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for PPEAssignment aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PPE.PPEAssignment.Specifications;

/// <summary>Abstract base specification for <see cref="PPEAssignment"/> aggregate predicate expressions.</summary>
public abstract class PPEAssignmentSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.PPE.PPEAssignment.Aggregates.PPEAssignment, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.PPE.PPEAssignment.Aggregates.PPEAssignment, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public PPEAssignmentSpecification And(PPEAssignmentSpecification other) => new AndPPEAssignmentSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public PPEAssignmentSpecification Or(PPEAssignmentSpecification other) => new OrPPEAssignmentSpecification(this, other);
}

internal sealed class AndPPEAssignmentSpecification(PPEAssignmentSpecification left, PPEAssignmentSpecification right) : PPEAssignmentSpecification
{
    public override Expression<Func<PlantSafety.Domain.PPE.PPEAssignment.Aggregates.PPEAssignment, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.PPE.PPEAssignment.Aggregates.PPEAssignment, bool>>(body, param);
    }
}

internal sealed class OrPPEAssignmentSpecification(PPEAssignmentSpecification left, PPEAssignmentSpecification right) : PPEAssignmentSpecification
{
    public override Expression<Func<PlantSafety.Domain.PPE.PPEAssignment.Aggregates.PPEAssignment, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.PPE.PPEAssignment.Aggregates.PPEAssignment, bool>>(body, param);
    }
}
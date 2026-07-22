// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AccidentManagement
//  Layer:          Domain
//  File:           AccidentSpecification.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for Accident aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
using System.Linq.Expressions;
using PlantSafety.Domain.AccidentManagement.Aggregates;

namespace PlantSafety.Domain.AccidentManagement.Specifications;

/// <summary>Abstract base specification for Accident aggregate predicate expressions.</summary>
public abstract class AccidentSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<Accident, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<Accident, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public AccidentSpecification And(AccidentSpecification other) => new AndAccidentSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public AccidentSpecification Or(AccidentSpecification other) => new OrAccidentSpecification(this, other);
}

internal sealed class AndAccidentSpecification(AccidentSpecification left, AccidentSpecification right) : AccidentSpecification
{
    public override Expression<Func<Accident, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<Accident, bool>>(body, param);
    }
}

internal sealed class OrAccidentSpecification(AccidentSpecification left, AccidentSpecification right) : AccidentSpecification
{
    public override Expression<Func<Accident, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<Accident, bool>>(body, param);
    }
}
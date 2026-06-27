// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Permits
//  Layer:          Domain
//  File:           PermitsSpecification.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Permits.Specifications;

/// <summary>Abstract base specification for <see cref="Permits"/> aggregate predicate expressions.</summary>
public abstract class PermitsSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<Permits, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<Permits, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public PermitsSpecification And(PermitsSpecification other) => new AndPermitsSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public PermitsSpecification Or(PermitsSpecification other) => new OrPermitsSpecification(this, other);
}

internal sealed class AndPermitsSpecification(PermitsSpecification left, PermitsSpecification right) : PermitsSpecification
{
    public override Expression<Func<Permits, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<Permits, bool>>(body, param);
    }
}

internal sealed class OrPermitsSpecification(PermitsSpecification left, PermitsSpecification right) : PermitsSpecification
{
    public override Expression<Func<Permits, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<Permits, bool>>(body, param);
    }
}

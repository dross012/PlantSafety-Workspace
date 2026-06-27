// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Culture
//  Layer:          Domain
//  File:           CultureSpecification.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Analytics.Culture.Specifications;

/// <summary>Abstract base specification for <see cref="Culture"/> aggregate predicate expressions.</summary>
public abstract class CultureSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<Culture, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<Culture, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public CultureSpecification And(CultureSpecification other) => new AndCultureSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public CultureSpecification Or(CultureSpecification other) => new OrCultureSpecification(this, other);
}

internal sealed class AndCultureSpecification(CultureSpecification left, CultureSpecification right) : CultureSpecification
{
    public override Expression<Func<Culture, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<Culture, bool>>(body, param);
    }
}

internal sealed class OrCultureSpecification(CultureSpecification left, CultureSpecification right) : CultureSpecification
{
    public override Expression<Func<Culture, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<Culture, bool>>(body, param);
    }
}

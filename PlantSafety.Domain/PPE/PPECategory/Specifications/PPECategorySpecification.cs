// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPECategory
//  Layer:          Domain
//  File:           PPECategorySpecification.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for PPECategory aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PPE.PPECategory.Specifications;

/// <summary>Abstract base specification for <see cref="PPECategory"/> aggregate predicate expressions.</summary>
public abstract class PPECategorySpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.PPE.PPECategory.Aggregates.PPECategory, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.PPE.PPECategory.Aggregates.PPECategory, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public PPECategorySpecification And(PPECategorySpecification other) => new AndPPECategorySpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public PPECategorySpecification Or(PPECategorySpecification other) => new OrPPECategorySpecification(this, other);
}

internal sealed class AndPPECategorySpecification(PPECategorySpecification left, PPECategorySpecification right) : PPECategorySpecification
{
    public override Expression<Func<PlantSafety.Domain.PPE.PPECategory.Aggregates.PPECategory, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.PPE.PPECategory.Aggregates.PPECategory, bool>>(body, param);
    }
}

internal sealed class OrPPECategorySpecification(PPECategorySpecification left, PPECategorySpecification right) : PPECategorySpecification
{
    public override Expression<Func<PlantSafety.Domain.PPE.PPECategory.Aggregates.PPECategory, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.PPE.PPECategory.Aggregates.PPECategory, bool>>(body, param);
    }
}
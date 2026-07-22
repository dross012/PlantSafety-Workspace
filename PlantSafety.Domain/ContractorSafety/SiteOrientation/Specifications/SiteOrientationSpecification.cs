// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety.SiteOrientation
//  Layer:          Domain
//  File:           SiteOrientationSpecification.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for SiteOrientation aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorSafety.SiteOrientation.Specifications;

/// <summary>Abstract base specification for <see cref="SiteOrientation"/> aggregate predicate expressions.</summary>
public abstract class SiteOrientationSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.ContractorSafety.SiteOrientation.Aggregates.SiteOrientation, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.ContractorSafety.SiteOrientation.Aggregates.SiteOrientation, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public SiteOrientationSpecification And(SiteOrientationSpecification other) => new AndSiteOrientationSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public SiteOrientationSpecification Or(SiteOrientationSpecification other) => new OrSiteOrientationSpecification(this, other);
}

internal sealed class AndSiteOrientationSpecification(SiteOrientationSpecification left, SiteOrientationSpecification right) : SiteOrientationSpecification
{
    public override Expression<Func<PlantSafety.Domain.ContractorSafety.SiteOrientation.Aggregates.SiteOrientation, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.ContractorSafety.SiteOrientation.Aggregates.SiteOrientation, bool>>(body, param);
    }
}

internal sealed class OrSiteOrientationSpecification(SiteOrientationSpecification left, SiteOrientationSpecification right) : SiteOrientationSpecification
{
    public override Expression<Func<PlantSafety.Domain.ContractorSafety.SiteOrientation.Aggregates.SiteOrientation, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.ContractorSafety.SiteOrientation.Aggregates.SiteOrientation, bool>>(body, param);
    }
}
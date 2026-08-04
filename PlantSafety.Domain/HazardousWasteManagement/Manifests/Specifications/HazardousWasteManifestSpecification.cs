// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.Manifests
//  Layer:          Domain
//  File:           HazardousWasteManifestSpecification.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Abstract base specification for HazardousWasteManifest aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.HazardousWasteManagement.Manifests.Aggregates;

namespace PlantSafety.Domain.HazardousWasteManagement.Manifests.Specifications;

/// <summary>Abstract base specification for HazardousWasteManifest aggregate predicate expressions.</summary>
public abstract class HazardousWasteManifestSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<HazardousWasteManifest, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<HazardousWasteManifest, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public HazardousWasteManifestSpecification And(HazardousWasteManifestSpecification other) => new AndHazardousWasteManifestSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public HazardousWasteManifestSpecification Or(HazardousWasteManifestSpecification other) => new OrHazardousWasteManifestSpecification(this, other);
}

internal sealed class AndHazardousWasteManifestSpecification(HazardousWasteManifestSpecification left, HazardousWasteManifestSpecification right) : HazardousWasteManifestSpecification
{
    public override Expression<Func<HazardousWasteManifest, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<HazardousWasteManifest, bool>>(body, param);
    }
}

internal sealed class OrHazardousWasteManifestSpecification(HazardousWasteManifestSpecification left, HazardousWasteManifestSpecification right) : HazardousWasteManifestSpecification
{
    public override Expression<Func<HazardousWasteManifest, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<HazardousWasteManifest, bool>>(body, param);
    }
}

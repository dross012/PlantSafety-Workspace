// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.AccessPoints
//  Layer:          Domain
//  File:           AccessPointSpecification.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for AccessPoint aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.AccessPoints.Specifications;

/// <summary>Abstract base specification for <see cref="AccessPoint"/> aggregate predicate expressions.</summary>
public abstract class AccessPointSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.PhysicalAccessControl.AccessPoints.Aggregates.AccessPoint, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.PhysicalAccessControl.AccessPoints.Aggregates.AccessPoint, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public AccessPointSpecification And(AccessPointSpecification other) => new AndAccessPointSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public AccessPointSpecification Or(AccessPointSpecification other) => new OrAccessPointSpecification(this, other);
}

internal sealed class AndAccessPointSpecification(AccessPointSpecification left, AccessPointSpecification right) : AccessPointSpecification
{
    public override Expression<Func<PlantSafety.Domain.PhysicalAccessControl.AccessPoints.Aggregates.AccessPoint, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.PhysicalAccessControl.AccessPoints.Aggregates.AccessPoint, bool>>(body, param);
    }
}

internal sealed class OrAccessPointSpecification(AccessPointSpecification left, AccessPointSpecification right) : AccessPointSpecification
{
    public override Expression<Func<PlantSafety.Domain.PhysicalAccessControl.AccessPoints.Aggregates.AccessPoint, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.PhysicalAccessControl.AccessPoints.Aggregates.AccessPoint, bool>>(body, param);
    }
}

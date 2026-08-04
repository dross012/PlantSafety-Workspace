// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.AccessEvents
//  Layer:          Domain
//  File:           AccessEventSpecification.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for AccessEvent aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.AccessEvents.Specifications;

/// <summary>Abstract base specification for <see cref="AccessEvent"/> aggregate predicate expressions.</summary>
public abstract class AccessEventSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.PhysicalAccessControl.AccessEvents.Aggregates.AccessEvent, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.PhysicalAccessControl.AccessEvents.Aggregates.AccessEvent, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public AccessEventSpecification And(AccessEventSpecification other) => new AndAccessEventSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public AccessEventSpecification Or(AccessEventSpecification other) => new OrAccessEventSpecification(this, other);
}

internal sealed class AndAccessEventSpecification(AccessEventSpecification left, AccessEventSpecification right) : AccessEventSpecification
{
    public override Expression<Func<PlantSafety.Domain.PhysicalAccessControl.AccessEvents.Aggregates.AccessEvent, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.PhysicalAccessControl.AccessEvents.Aggregates.AccessEvent, bool>>(body, param);
    }
}

internal sealed class OrAccessEventSpecification(AccessEventSpecification left, AccessEventSpecification right) : AccessEventSpecification
{
    public override Expression<Func<PlantSafety.Domain.PhysicalAccessControl.AccessEvents.Aggregates.AccessEvent, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.PhysicalAccessControl.AccessEvents.Aggregates.AccessEvent, bool>>(body, param);
    }
}

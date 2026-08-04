// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.AccessAnomalies
//  Layer:          Domain
//  File:           AccessAnomalySpecification.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for AccessAnomaly aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.AccessAnomalies.Specifications;

/// <summary>Abstract base specification for <see cref="AccessAnomaly"/> aggregate predicate expressions.</summary>
public abstract class AccessAnomalySpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.PhysicalAccessControl.AccessAnomalies.Aggregates.AccessAnomaly, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.PhysicalAccessControl.AccessAnomalies.Aggregates.AccessAnomaly, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public AccessAnomalySpecification And(AccessAnomalySpecification other) => new AndAccessAnomalySpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public AccessAnomalySpecification Or(AccessAnomalySpecification other) => new OrAccessAnomalySpecification(this, other);
}

internal sealed class AndAccessAnomalySpecification(AccessAnomalySpecification left, AccessAnomalySpecification right) : AccessAnomalySpecification
{
    public override Expression<Func<PlantSafety.Domain.PhysicalAccessControl.AccessAnomalies.Aggregates.AccessAnomaly, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.PhysicalAccessControl.AccessAnomalies.Aggregates.AccessAnomaly, bool>>(body, param);
    }
}

internal sealed class OrAccessAnomalySpecification(AccessAnomalySpecification left, AccessAnomalySpecification right) : AccessAnomalySpecification
{
    public override Expression<Func<PlantSafety.Domain.PhysicalAccessControl.AccessAnomalies.Aggregates.AccessAnomaly, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.PhysicalAccessControl.AccessAnomalies.Aggregates.AccessAnomaly, bool>>(body, param);
    }
}

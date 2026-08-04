// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.Transporters
//  Layer:          Domain
//  File:           TransporterSpecification.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Abstract base specification for Transporter aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.HazardousWasteManagement.Transporters.Aggregates;

namespace PlantSafety.Domain.HazardousWasteManagement.Transporters.Specifications;

/// <summary>Abstract base specification for Transporter aggregate predicate expressions.</summary>
public abstract class TransporterSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<Transporter, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<Transporter, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public TransporterSpecification And(TransporterSpecification other) => new AndTransporterSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public TransporterSpecification Or(TransporterSpecification other) => new OrTransporterSpecification(this, other);
}

internal sealed class AndTransporterSpecification(TransporterSpecification left, TransporterSpecification right) : TransporterSpecification
{
    public override Expression<Func<Transporter, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<Transporter, bool>>(body, param);
    }
}

internal sealed class OrTransporterSpecification(TransporterSpecification left, TransporterSpecification right) : TransporterSpecification
{
    public override Expression<Func<Transporter, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<Transporter, bool>>(body, param);
    }
}

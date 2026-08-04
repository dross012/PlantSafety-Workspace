// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.LoadSecurementChecks
//  Layer:          Domain
//  File:           LoadSecurementCheckSpecification.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for LoadSecurementCheck aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.LoadSecurementChecks.Specifications;

/// <summary>Abstract base specification for <see cref="LoadSecurementCheck"/> aggregate predicate expressions.</summary>
public abstract class LoadSecurementCheckSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.LogisticsTransportSafety.LoadSecurementChecks.Aggregates.LoadSecurementCheck, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.LogisticsTransportSafety.LoadSecurementChecks.Aggregates.LoadSecurementCheck, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public LoadSecurementCheckSpecification And(LoadSecurementCheckSpecification other) => new AndLoadSecurementCheckSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public LoadSecurementCheckSpecification Or(LoadSecurementCheckSpecification other) => new OrLoadSecurementCheckSpecification(this, other);
}

internal sealed class AndLoadSecurementCheckSpecification(LoadSecurementCheckSpecification left, LoadSecurementCheckSpecification right) : LoadSecurementCheckSpecification
{
    public override Expression<Func<PlantSafety.Domain.LogisticsTransportSafety.LoadSecurementChecks.Aggregates.LoadSecurementCheck, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.LogisticsTransportSafety.LoadSecurementChecks.Aggregates.LoadSecurementCheck, bool>>(body, param);
    }
}

internal sealed class OrLoadSecurementCheckSpecification(LoadSecurementCheckSpecification left, LoadSecurementCheckSpecification right) : LoadSecurementCheckSpecification
{
    public override Expression<Func<PlantSafety.Domain.LogisticsTransportSafety.LoadSecurementChecks.Aggregates.LoadSecurementCheck, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.LogisticsTransportSafety.LoadSecurementChecks.Aggregates.LoadSecurementCheck, bool>>(body, param);
    }
}

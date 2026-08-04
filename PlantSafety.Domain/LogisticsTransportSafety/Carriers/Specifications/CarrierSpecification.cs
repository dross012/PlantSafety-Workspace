// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.Carriers
//  Layer:          Domain
//  File:           CarrierSpecification.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for Carrier aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.Carriers.Specifications;

/// <summary>Abstract base specification for <see cref="Carrier"/> aggregate predicate expressions.</summary>
public abstract class CarrierSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.LogisticsTransportSafety.Carriers.Aggregates.Carrier, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.LogisticsTransportSafety.Carriers.Aggregates.Carrier, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public CarrierSpecification And(CarrierSpecification other) => new AndCarrierSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public CarrierSpecification Or(CarrierSpecification other) => new OrCarrierSpecification(this, other);
}

internal sealed class AndCarrierSpecification(CarrierSpecification left, CarrierSpecification right) : CarrierSpecification
{
    public override Expression<Func<PlantSafety.Domain.LogisticsTransportSafety.Carriers.Aggregates.Carrier, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.LogisticsTransportSafety.Carriers.Aggregates.Carrier, bool>>(body, param);
    }
}

internal sealed class OrCarrierSpecification(CarrierSpecification left, CarrierSpecification right) : CarrierSpecification
{
    public override Expression<Func<PlantSafety.Domain.LogisticsTransportSafety.Carriers.Aggregates.Carrier, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.LogisticsTransportSafety.Carriers.Aggregates.Carrier, bool>>(body, param);
    }
}

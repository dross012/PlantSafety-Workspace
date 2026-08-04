// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.TransportTrips
//  Layer:          Domain
//  File:           TransportTripSpecification.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for TransportTrip aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.TransportTrips.Specifications;

/// <summary>Abstract base specification for <see cref="TransportTrip"/> aggregate predicate expressions.</summary>
public abstract class TransportTripSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.LogisticsTransportSafety.TransportTrips.Aggregates.TransportTrip, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.LogisticsTransportSafety.TransportTrips.Aggregates.TransportTrip, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public TransportTripSpecification And(TransportTripSpecification other) => new AndTransportTripSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public TransportTripSpecification Or(TransportTripSpecification other) => new OrTransportTripSpecification(this, other);
}

internal sealed class AndTransportTripSpecification(TransportTripSpecification left, TransportTripSpecification right) : TransportTripSpecification
{
    public override Expression<Func<PlantSafety.Domain.LogisticsTransportSafety.TransportTrips.Aggregates.TransportTrip, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.LogisticsTransportSafety.TransportTrips.Aggregates.TransportTrip, bool>>(body, param);
    }
}

internal sealed class OrTransportTripSpecification(TransportTripSpecification left, TransportTripSpecification right) : TransportTripSpecification
{
    public override Expression<Func<PlantSafety.Domain.LogisticsTransportSafety.TransportTrips.Aggregates.TransportTrip, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.LogisticsTransportSafety.TransportTrips.Aggregates.TransportTrip, bool>>(body, param);
    }
}

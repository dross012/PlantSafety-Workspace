// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.TransportVehicles
//  Layer:          Domain
//  File:           TransportVehicleSpecification.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for TransportVehicle aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.TransportVehicles.Specifications;

/// <summary>Abstract base specification for <see cref="TransportVehicle"/> aggregate predicate expressions.</summary>
public abstract class TransportVehicleSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.LogisticsTransportSafety.TransportVehicles.Aggregates.TransportVehicle, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.LogisticsTransportSafety.TransportVehicles.Aggregates.TransportVehicle, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public TransportVehicleSpecification And(TransportVehicleSpecification other) => new AndTransportVehicleSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public TransportVehicleSpecification Or(TransportVehicleSpecification other) => new OrTransportVehicleSpecification(this, other);
}

internal sealed class AndTransportVehicleSpecification(TransportVehicleSpecification left, TransportVehicleSpecification right) : TransportVehicleSpecification
{
    public override Expression<Func<PlantSafety.Domain.LogisticsTransportSafety.TransportVehicles.Aggregates.TransportVehicle, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.LogisticsTransportSafety.TransportVehicles.Aggregates.TransportVehicle, bool>>(body, param);
    }
}

internal sealed class OrTransportVehicleSpecification(TransportVehicleSpecification left, TransportVehicleSpecification right) : TransportVehicleSpecification
{
    public override Expression<Func<PlantSafety.Domain.LogisticsTransportSafety.TransportVehicles.Aggregates.TransportVehicle, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.LogisticsTransportSafety.TransportVehicles.Aggregates.TransportVehicle, bool>>(body, param);
    }
}

// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.OperatingGeofence
//  Layer:          Domain
//  File:           OperatingGeofenceSpecification.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for OperatingGeofence aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.OperatingGeofence.Specifications;

/// <summary>Abstract base specification for <see cref="Aggregates.OperatingGeofence"/> aggregate predicate expressions.</summary>
public abstract class OperatingGeofenceSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<Aggregates.OperatingGeofence, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<Aggregates.OperatingGeofence, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public OperatingGeofenceSpecification And(OperatingGeofenceSpecification other) => new AndOperatingGeofenceSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public OperatingGeofenceSpecification Or(OperatingGeofenceSpecification other) => new OrOperatingGeofenceSpecification(this, other);
}

internal sealed class AndOperatingGeofenceSpecification(OperatingGeofenceSpecification left, OperatingGeofenceSpecification right) : OperatingGeofenceSpecification
{
    public override Expression<Func<Aggregates.OperatingGeofence, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<Aggregates.OperatingGeofence, bool>>(body, param);
    }
}

internal sealed class OrOperatingGeofenceSpecification(OperatingGeofenceSpecification left, OperatingGeofenceSpecification right) : OperatingGeofenceSpecification
{
    public override Expression<Func<Aggregates.OperatingGeofence, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<Aggregates.OperatingGeofence, bool>>(body, param);
    }
}

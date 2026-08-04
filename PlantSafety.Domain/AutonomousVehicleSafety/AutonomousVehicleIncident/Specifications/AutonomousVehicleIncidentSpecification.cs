// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.AutonomousVehicleIncident
//  Layer:          Domain
//  File:           AutonomousVehicleIncidentSpecification.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for AutonomousVehicleIncident aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.AutonomousVehicleIncident.Specifications;

/// <summary>Abstract base specification for <see cref="Aggregates.AutonomousVehicleIncident"/> aggregate predicate expressions.</summary>
public abstract class AutonomousVehicleIncidentSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<Aggregates.AutonomousVehicleIncident, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<Aggregates.AutonomousVehicleIncident, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public AutonomousVehicleIncidentSpecification And(AutonomousVehicleIncidentSpecification other) => new AndAutonomousVehicleIncidentSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public AutonomousVehicleIncidentSpecification Or(AutonomousVehicleIncidentSpecification other) => new OrAutonomousVehicleIncidentSpecification(this, other);
}

internal sealed class AndAutonomousVehicleIncidentSpecification(AutonomousVehicleIncidentSpecification left, AutonomousVehicleIncidentSpecification right) : AutonomousVehicleIncidentSpecification
{
    public override Expression<Func<Aggregates.AutonomousVehicleIncident, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<Aggregates.AutonomousVehicleIncident, bool>>(body, param);
    }
}

internal sealed class OrAutonomousVehicleIncidentSpecification(AutonomousVehicleIncidentSpecification left, AutonomousVehicleIncidentSpecification right) : AutonomousVehicleIncidentSpecification
{
    public override Expression<Func<Aggregates.AutonomousVehicleIncident, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<Aggregates.AutonomousVehicleIncident, bool>>(body, param);
    }
}

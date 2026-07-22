// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.EmergencyStopDevices
//  Layer:          Domain
//  File:           EmergencyStopDevicesSpecification.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for EmergencyStopDevices aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.EmergencyStopDevices.Specifications;

/// <summary>Abstract base specification for <see cref="EmergencyStopDevices"/> aggregate predicate expressions.</summary>
public abstract class EmergencyStopDevicesSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.MachineSafety.EmergencyStopDevices.Aggregates.EmergencyStopDevices, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.MachineSafety.EmergencyStopDevices.Aggregates.EmergencyStopDevices, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public EmergencyStopDevicesSpecification And(EmergencyStopDevicesSpecification other) => new AndEmergencyStopDevicesSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public EmergencyStopDevicesSpecification Or(EmergencyStopDevicesSpecification other) => new OrEmergencyStopDevicesSpecification(this, other);
}

internal sealed class AndEmergencyStopDevicesSpecification(EmergencyStopDevicesSpecification left, EmergencyStopDevicesSpecification right) : EmergencyStopDevicesSpecification
{
    public override Expression<Func<PlantSafety.Domain.MachineSafety.EmergencyStopDevices.Aggregates.EmergencyStopDevices, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.MachineSafety.EmergencyStopDevices.Aggregates.EmergencyStopDevices, bool>>(body, param);
    }
}

internal sealed class OrEmergencyStopDevicesSpecification(EmergencyStopDevicesSpecification left, EmergencyStopDevicesSpecification right) : EmergencyStopDevicesSpecification
{
    public override Expression<Func<PlantSafety.Domain.MachineSafety.EmergencyStopDevices.Aggregates.EmergencyStopDevices, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.MachineSafety.EmergencyStopDevices.Aggregates.EmergencyStopDevices, bool>>(body, param);
    }
}
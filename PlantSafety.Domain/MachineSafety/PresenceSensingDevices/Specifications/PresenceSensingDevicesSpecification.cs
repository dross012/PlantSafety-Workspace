// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.PresenceSensingDevices
//  Layer:          Domain
//  File:           PresenceSensingDevicesSpecification.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for PresenceSensingDevices aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.PresenceSensingDevices.Specifications;

/// <summary>Abstract base specification for <see cref="PresenceSensingDevices"/> aggregate predicate expressions.</summary>
public abstract class PresenceSensingDevicesSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.MachineSafety.PresenceSensingDevices.Aggregates.PresenceSensingDevices, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.MachineSafety.PresenceSensingDevices.Aggregates.PresenceSensingDevices, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public PresenceSensingDevicesSpecification And(PresenceSensingDevicesSpecification other) => new AndPresenceSensingDevicesSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public PresenceSensingDevicesSpecification Or(PresenceSensingDevicesSpecification other) => new OrPresenceSensingDevicesSpecification(this, other);
}

internal sealed class AndPresenceSensingDevicesSpecification(PresenceSensingDevicesSpecification left, PresenceSensingDevicesSpecification right) : PresenceSensingDevicesSpecification
{
    public override Expression<Func<PlantSafety.Domain.MachineSafety.PresenceSensingDevices.Aggregates.PresenceSensingDevices, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.MachineSafety.PresenceSensingDevices.Aggregates.PresenceSensingDevices, bool>>(body, param);
    }
}

internal sealed class OrPresenceSensingDevicesSpecification(PresenceSensingDevicesSpecification left, PresenceSensingDevicesSpecification right) : PresenceSensingDevicesSpecification
{
    public override Expression<Func<PlantSafety.Domain.MachineSafety.PresenceSensingDevices.Aggregates.PresenceSensingDevices, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.MachineSafety.PresenceSensingDevices.Aggregates.PresenceSensingDevices, bool>>(body, param);
    }
}
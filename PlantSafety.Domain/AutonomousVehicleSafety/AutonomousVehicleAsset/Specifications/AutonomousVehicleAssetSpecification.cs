// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.AutonomousVehicleAsset
//  Layer:          Domain
//  File:           AutonomousVehicleAssetSpecification.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for AutonomousVehicleAsset aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.AutonomousVehicleAsset.Specifications;

/// <summary>Abstract base specification for <see cref="Aggregates.AutonomousVehicleAsset"/> aggregate predicate expressions.</summary>
public abstract class AutonomousVehicleAssetSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<Aggregates.AutonomousVehicleAsset, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<Aggregates.AutonomousVehicleAsset, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public AutonomousVehicleAssetSpecification And(AutonomousVehicleAssetSpecification other) => new AndAutonomousVehicleAssetSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public AutonomousVehicleAssetSpecification Or(AutonomousVehicleAssetSpecification other) => new OrAutonomousVehicleAssetSpecification(this, other);
}

internal sealed class AndAutonomousVehicleAssetSpecification(AutonomousVehicleAssetSpecification left, AutonomousVehicleAssetSpecification right) : AutonomousVehicleAssetSpecification
{
    public override Expression<Func<Aggregates.AutonomousVehicleAsset, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<Aggregates.AutonomousVehicleAsset, bool>>(body, param);
    }
}

internal sealed class OrAutonomousVehicleAssetSpecification(AutonomousVehicleAssetSpecification left, AutonomousVehicleAssetSpecification right) : AutonomousVehicleAssetSpecification
{
    public override Expression<Func<Aggregates.AutonomousVehicleAsset, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<Aggregates.AutonomousVehicleAsset, bool>>(body, param);
    }
}

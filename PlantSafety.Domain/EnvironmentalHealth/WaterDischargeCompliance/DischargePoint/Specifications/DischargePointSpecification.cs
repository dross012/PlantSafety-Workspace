// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.DischargePoint
//  Layer:          Domain
//  File:           DischargePointSpecification.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for DischargePoint aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargePoint.Specifications;

/// <summary>Abstract base specification for <see cref="DischargePoint"/> aggregate predicate expressions.</summary>
public abstract class DischargePointSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargePoint.Aggregates.DischargePoint, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargePoint.Aggregates.DischargePoint, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public DischargePointSpecification And(DischargePointSpecification other) => new AndDischargePointSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public DischargePointSpecification Or(DischargePointSpecification other) => new OrDischargePointSpecification(this, other);
}

internal sealed class AndDischargePointSpecification(DischargePointSpecification left, DischargePointSpecification right) : DischargePointSpecification
{
    public override Expression<Func<PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargePoint.Aggregates.DischargePoint, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargePoint.Aggregates.DischargePoint, bool>>(body, param);
    }
}

internal sealed class OrDischargePointSpecification(DischargePointSpecification left, DischargePointSpecification right) : DischargePointSpecification
{
    public override Expression<Func<PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargePoint.Aggregates.DischargePoint, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargePoint.Aggregates.DischargePoint, bool>>(body, param);
    }
}
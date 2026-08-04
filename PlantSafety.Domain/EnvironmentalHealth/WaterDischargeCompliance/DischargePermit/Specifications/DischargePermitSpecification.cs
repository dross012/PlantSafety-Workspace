// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.DischargePermit
//  Layer:          Domain
//  File:           DischargePermitSpecification.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for DischargePermit aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargePermit.Specifications;

/// <summary>Abstract base specification for <see cref="DischargePermit"/> aggregate predicate expressions.</summary>
public abstract class DischargePermitSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargePermit.Aggregates.DischargePermit, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargePermit.Aggregates.DischargePermit, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public DischargePermitSpecification And(DischargePermitSpecification other) => new AndDischargePermitSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public DischargePermitSpecification Or(DischargePermitSpecification other) => new OrDischargePermitSpecification(this, other);
}

internal sealed class AndDischargePermitSpecification(DischargePermitSpecification left, DischargePermitSpecification right) : DischargePermitSpecification
{
    public override Expression<Func<PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargePermit.Aggregates.DischargePermit, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargePermit.Aggregates.DischargePermit, bool>>(body, param);
    }
}

internal sealed class OrDischargePermitSpecification(DischargePermitSpecification left, DischargePermitSpecification right) : DischargePermitSpecification
{
    public override Expression<Func<PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargePermit.Aggregates.DischargePermit, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargePermit.Aggregates.DischargePermit, bool>>(body, param);
    }
}
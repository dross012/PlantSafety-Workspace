// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.EffluentSample
//  Layer:          Domain
//  File:           EffluentSampleSpecification.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for EffluentSample aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.EffluentSample.Specifications;

/// <summary>Abstract base specification for <see cref="EffluentSample"/> aggregate predicate expressions.</summary>
public abstract class EffluentSampleSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.EffluentSample.Aggregates.EffluentSample, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.EffluentSample.Aggregates.EffluentSample, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public EffluentSampleSpecification And(EffluentSampleSpecification other) => new AndEffluentSampleSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public EffluentSampleSpecification Or(EffluentSampleSpecification other) => new OrEffluentSampleSpecification(this, other);
}

internal sealed class AndEffluentSampleSpecification(EffluentSampleSpecification left, EffluentSampleSpecification right) : EffluentSampleSpecification
{
    public override Expression<Func<PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.EffluentSample.Aggregates.EffluentSample, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.EffluentSample.Aggregates.EffluentSample, bool>>(body, param);
    }
}

internal sealed class OrEffluentSampleSpecification(EffluentSampleSpecification left, EffluentSampleSpecification right) : EffluentSampleSpecification
{
    public override Expression<Func<PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.EffluentSample.Aggregates.EffluentSample, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.EffluentSample.Aggregates.EffluentSample, bool>>(body, param);
    }
}
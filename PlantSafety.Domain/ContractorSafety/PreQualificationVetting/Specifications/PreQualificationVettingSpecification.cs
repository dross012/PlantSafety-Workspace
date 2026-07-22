// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety.PreQualificationVetting
//  Layer:          Domain
//  File:           PreQualificationVettingSpecification.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for PreQualificationVetting aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorSafety.PreQualificationVetting.Specifications;

/// <summary>Abstract base specification for <see cref="PreQualificationVetting"/> aggregate predicate expressions.</summary>
public abstract class PreQualificationVettingSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.ContractorSafety.PreQualificationVetting.Aggregates.PreQualificationVetting, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.ContractorSafety.PreQualificationVetting.Aggregates.PreQualificationVetting, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public PreQualificationVettingSpecification And(PreQualificationVettingSpecification other) => new AndPreQualificationVettingSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public PreQualificationVettingSpecification Or(PreQualificationVettingSpecification other) => new OrPreQualificationVettingSpecification(this, other);
}

internal sealed class AndPreQualificationVettingSpecification(PreQualificationVettingSpecification left, PreQualificationVettingSpecification right) : PreQualificationVettingSpecification
{
    public override Expression<Func<PlantSafety.Domain.ContractorSafety.PreQualificationVetting.Aggregates.PreQualificationVetting, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.ContractorSafety.PreQualificationVetting.Aggregates.PreQualificationVetting, bool>>(body, param);
    }
}

internal sealed class OrPreQualificationVettingSpecification(PreQualificationVettingSpecification left, PreQualificationVettingSpecification right) : PreQualificationVettingSpecification
{
    public override Expression<Func<PlantSafety.Domain.ContractorSafety.PreQualificationVetting.Aggregates.PreQualificationVetting, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.ContractorSafety.PreQualificationVetting.Aggregates.PreQualificationVetting, bool>>(body, param);
    }
}
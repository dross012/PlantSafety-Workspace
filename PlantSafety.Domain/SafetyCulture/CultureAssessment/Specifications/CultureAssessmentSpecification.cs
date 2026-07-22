// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyCulture.CultureAssessment
//  Layer:          Domain
//  File:           CultureAssessmentSpecification.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for CultureAssessment aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyCulture.CultureAssessment.Specifications;

/// <summary>Abstract base specification for <see cref="CultureAssessment"/> aggregate predicate expressions.</summary>
public abstract class CultureAssessmentSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.SafetyCulture.CultureAssessment.Aggregates.CultureAssessment, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.SafetyCulture.CultureAssessment.Aggregates.CultureAssessment, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public CultureAssessmentSpecification And(CultureAssessmentSpecification other) => new AndCultureAssessmentSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public CultureAssessmentSpecification Or(CultureAssessmentSpecification other) => new OrCultureAssessmentSpecification(this, other);
}

internal sealed class AndCultureAssessmentSpecification(CultureAssessmentSpecification left, CultureAssessmentSpecification right) : CultureAssessmentSpecification
{
    public override Expression<Func<PlantSafety.Domain.SafetyCulture.CultureAssessment.Aggregates.CultureAssessment, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.SafetyCulture.CultureAssessment.Aggregates.CultureAssessment, bool>>(body, param);
    }
}

internal sealed class OrCultureAssessmentSpecification(CultureAssessmentSpecification left, CultureAssessmentSpecification right) : CultureAssessmentSpecification
{
    public override Expression<Func<PlantSafety.Domain.SafetyCulture.CultureAssessment.Aggregates.CultureAssessment, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.SafetyCulture.CultureAssessment.Aggregates.CultureAssessment, bool>>(body, param);
    }
}
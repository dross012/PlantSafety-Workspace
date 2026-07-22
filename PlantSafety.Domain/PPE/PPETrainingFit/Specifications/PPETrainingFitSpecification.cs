// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPETrainingFit
//  Layer:          Domain
//  File:           PPETrainingFitSpecification.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for PPETrainingFit aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PPE.PPETrainingFit.Specifications;

/// <summary>Abstract base specification for <see cref="PPETrainingFit"/> aggregate predicate expressions.</summary>
public abstract class PPETrainingFitSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.PPE.PPETrainingFit.Aggregates.PPETrainingFit, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.PPE.PPETrainingFit.Aggregates.PPETrainingFit, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public PPETrainingFitSpecification And(PPETrainingFitSpecification other) => new AndPPETrainingFitSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public PPETrainingFitSpecification Or(PPETrainingFitSpecification other) => new OrPPETrainingFitSpecification(this, other);
}

internal sealed class AndPPETrainingFitSpecification(PPETrainingFitSpecification left, PPETrainingFitSpecification right) : PPETrainingFitSpecification
{
    public override Expression<Func<PlantSafety.Domain.PPE.PPETrainingFit.Aggregates.PPETrainingFit, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.PPE.PPETrainingFit.Aggregates.PPETrainingFit, bool>>(body, param);
    }
}

internal sealed class OrPPETrainingFitSpecification(PPETrainingFitSpecification left, PPETrainingFitSpecification right) : PPETrainingFitSpecification
{
    public override Expression<Func<PlantSafety.Domain.PPE.PPETrainingFit.Aggregates.PPETrainingFit, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.PPE.PPETrainingFit.Aggregates.PPETrainingFit, bool>>(body, param);
    }
}
// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.ScoreCards
//  Layer:          Domain
//  File:           ScoreCardsSpecification.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Analytics.ScoreCards.Specifications;

/// <summary>Abstract base specification for <see cref="ScoreCards"/> aggregate predicate expressions.</summary>
public abstract class ScoreCardsSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.Analytics.ScoreCards.Aggregates.ScoreCards, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.Analytics.ScoreCards.Aggregates.ScoreCards, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public ScoreCardsSpecification And(ScoreCardsSpecification other) => new AndScoreCardsSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public ScoreCardsSpecification Or(ScoreCardsSpecification other) => new OrScoreCardsSpecification(this, other);
}

internal sealed class AndScoreCardsSpecification(ScoreCardsSpecification left, ScoreCardsSpecification right) : ScoreCardsSpecification
{
    public override Expression<Func<PlantSafety.Domain.Analytics.ScoreCards.Aggregates.ScoreCards, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.Analytics.ScoreCards.Aggregates.ScoreCards, bool>>(body, param);
    }
}

internal sealed class OrScoreCardsSpecification(ScoreCardsSpecification left, ScoreCardsSpecification right) : ScoreCardsSpecification
{
    public override Expression<Func<PlantSafety.Domain.Analytics.ScoreCards.Aggregates.ScoreCards, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.Analytics.ScoreCards.Aggregates.ScoreCards, bool>>(body, param);
    }
}

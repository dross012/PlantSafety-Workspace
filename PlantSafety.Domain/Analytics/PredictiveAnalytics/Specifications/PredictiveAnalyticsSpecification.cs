// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.PredictiveAnalytics
//  Layer:          Domain
//  File:           PredictiveAnalyticsSpecification.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Analytics.PredictiveAnalytics.Specifications;

/// <summary>Abstract base specification for <see cref="PredictiveAnalytics"/> aggregate predicate expressions.</summary>
public abstract class PredictiveAnalyticsSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.Analytics.PredictiveAnalytics.Aggregates.PredictiveAnalytics, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.Analytics.PredictiveAnalytics.Aggregates.PredictiveAnalytics, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public PredictiveAnalyticsSpecification And(PredictiveAnalyticsSpecification other) => new AndPredictiveAnalyticsSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public PredictiveAnalyticsSpecification Or(PredictiveAnalyticsSpecification other) => new OrPredictiveAnalyticsSpecification(this, other);
}

internal sealed class AndPredictiveAnalyticsSpecification(PredictiveAnalyticsSpecification left, PredictiveAnalyticsSpecification right) : PredictiveAnalyticsSpecification
{
    public override Expression<Func<PlantSafety.Domain.Analytics.PredictiveAnalytics.Aggregates.PredictiveAnalytics, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.Analytics.PredictiveAnalytics.Aggregates.PredictiveAnalytics, bool>>(body, param);
    }
}

internal sealed class OrPredictiveAnalyticsSpecification(PredictiveAnalyticsSpecification left, PredictiveAnalyticsSpecification right) : PredictiveAnalyticsSpecification
{
    public override Expression<Func<PlantSafety.Domain.Analytics.PredictiveAnalytics.Aggregates.PredictiveAnalytics, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.Analytics.PredictiveAnalytics.Aggregates.PredictiveAnalytics, bool>>(body, param);
    }
}

// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Engagement
//  Layer:          Domain
//  File:           EngagementSpecification.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Analytics.Engagement.Specifications;

/// <summary>Abstract base specification for <see cref="Engagement"/> aggregate predicate expressions.</summary>
public abstract class EngagementSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.Analytics.Engagement.Aggregates.Engagement, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.Analytics.Engagement.Aggregates.Engagement, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public EngagementSpecification And(EngagementSpecification other) => new AndEngagementSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public EngagementSpecification Or(EngagementSpecification other) => new OrEngagementSpecification(this, other);
}

internal sealed class AndEngagementSpecification(EngagementSpecification left, EngagementSpecification right) : EngagementSpecification
{
    public override Expression<Func<PlantSafety.Domain.Analytics.Engagement.Aggregates.Engagement, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.Analytics.Engagement.Aggregates.Engagement, bool>>(body, param);
    }
}

internal sealed class OrEngagementSpecification(EngagementSpecification left, EngagementSpecification right) : EngagementSpecification
{
    public override Expression<Func<PlantSafety.Domain.Analytics.Engagement.Aggregates.Engagement, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.Analytics.Engagement.Aggregates.Engagement, bool>>(body, param);
    }
}

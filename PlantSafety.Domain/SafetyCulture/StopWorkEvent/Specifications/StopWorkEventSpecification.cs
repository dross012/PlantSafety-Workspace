// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyCulture.StopWorkEvent
//  Layer:          Domain
//  File:           StopWorkEventSpecification.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for StopWorkEvent aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyCulture.StopWorkEvent.Specifications;

/// <summary>Abstract base specification for <see cref="StopWorkEvent"/> aggregate predicate expressions.</summary>
public abstract class StopWorkEventSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.SafetyCulture.StopWorkEvent.Aggregates.StopWorkEvent, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.SafetyCulture.StopWorkEvent.Aggregates.StopWorkEvent, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public StopWorkEventSpecification And(StopWorkEventSpecification other) => new AndStopWorkEventSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public StopWorkEventSpecification Or(StopWorkEventSpecification other) => new OrStopWorkEventSpecification(this, other);
}

internal sealed class AndStopWorkEventSpecification(StopWorkEventSpecification left, StopWorkEventSpecification right) : StopWorkEventSpecification
{
    public override Expression<Func<PlantSafety.Domain.SafetyCulture.StopWorkEvent.Aggregates.StopWorkEvent, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.SafetyCulture.StopWorkEvent.Aggregates.StopWorkEvent, bool>>(body, param);
    }
}

internal sealed class OrStopWorkEventSpecification(StopWorkEventSpecification left, StopWorkEventSpecification right) : StopWorkEventSpecification
{
    public override Expression<Func<PlantSafety.Domain.SafetyCulture.StopWorkEvent.Aggregates.StopWorkEvent, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.SafetyCulture.StopWorkEvent.Aggregates.StopWorkEvent, bool>>(body, param);
    }
}
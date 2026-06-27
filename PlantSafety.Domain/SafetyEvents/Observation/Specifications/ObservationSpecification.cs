// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.Observation
//  Layer:          Domain
//  File:           ObservationSpecification.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.Observation.Specifications;

/// <summary>Abstract base specification for <see cref="Observation"/> aggregate predicate expressions.</summary>
public abstract class ObservationSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<Observation, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<Observation, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public ObservationSpecification And(ObservationSpecification other) => new AndObservationSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public ObservationSpecification Or(ObservationSpecification other) => new OrObservationSpecification(this, other);
}

internal sealed class AndObservationSpecification(ObservationSpecification left, ObservationSpecification right) : ObservationSpecification
{
    public override Expression<Func<Observation, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<Observation, bool>>(body, param);
    }
}

internal sealed class OrObservationSpecification(ObservationSpecification left, ObservationSpecification right) : ObservationSpecification
{
    public override Expression<Func<Observation, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<Observation, bool>>(body, param);
    }
}

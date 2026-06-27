// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Schedules
//  Layer:          Domain
//  File:           SchedulesSpecification.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.Schedules.Specifications;

/// <summary>Abstract base specification for <see cref="Schedules"/> aggregate predicate expressions.</summary>
public abstract class SchedulesSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<Schedules, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<Schedules, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public SchedulesSpecification And(SchedulesSpecification other) => new AndSchedulesSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public SchedulesSpecification Or(SchedulesSpecification other) => new OrSchedulesSpecification(this, other);
}

internal sealed class AndSchedulesSpecification(SchedulesSpecification left, SchedulesSpecification right) : SchedulesSpecification
{
    public override Expression<Func<Schedules, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<Schedules, bool>>(body, param);
    }
}

internal sealed class OrSchedulesSpecification(SchedulesSpecification left, SchedulesSpecification right) : SchedulesSpecification
{
    public override Expression<Func<Schedules, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<Schedules, bool>>(body, param);
    }
}

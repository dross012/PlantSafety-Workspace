// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.ShiftFatigue
//  Layer:          Domain
//  File:           ShiftFatigueSpecification.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for ShiftFatigue aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.ShiftFatigue.Specifications;

/// <summary>Abstract base specification for <see cref="ShiftFatigue"/> aggregate predicate expressions.</summary>
public abstract class ShiftFatigueSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.Scheduling.ShiftFatigue.Aggregates.ShiftFatigue, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.Scheduling.ShiftFatigue.Aggregates.ShiftFatigue, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public ShiftFatigueSpecification And(ShiftFatigueSpecification other) => new AndShiftFatigueSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public ShiftFatigueSpecification Or(ShiftFatigueSpecification other) => new OrShiftFatigueSpecification(this, other);
}

internal sealed class AndShiftFatigueSpecification(ShiftFatigueSpecification left, ShiftFatigueSpecification right) : ShiftFatigueSpecification
{
    public override Expression<Func<PlantSafety.Domain.Scheduling.ShiftFatigue.Aggregates.ShiftFatigue, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.Scheduling.ShiftFatigue.Aggregates.ShiftFatigue, bool>>(body, param);
    }
}

internal sealed class OrShiftFatigueSpecification(ShiftFatigueSpecification left, ShiftFatigueSpecification right) : ShiftFatigueSpecification
{
    public override Expression<Func<PlantSafety.Domain.Scheduling.ShiftFatigue.Aggregates.ShiftFatigue, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.Scheduling.ShiftFatigue.Aggregates.ShiftFatigue, bool>>(body, param);
    }
}
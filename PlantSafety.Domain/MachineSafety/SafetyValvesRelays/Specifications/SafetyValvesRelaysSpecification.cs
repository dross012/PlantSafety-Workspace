// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.SafetyValvesRelays
//  Layer:          Domain
//  File:           SafetyValvesRelaysSpecification.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for SafetyValvesRelays aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.SafetyValvesRelays.Specifications;

/// <summary>Abstract base specification for <see cref="SafetyValvesRelays"/> aggregate predicate expressions.</summary>
public abstract class SafetyValvesRelaysSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.MachineSafety.SafetyValvesRelays.Aggregates.SafetyValvesRelays, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.MachineSafety.SafetyValvesRelays.Aggregates.SafetyValvesRelays, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public SafetyValvesRelaysSpecification And(SafetyValvesRelaysSpecification other) => new AndSafetyValvesRelaysSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public SafetyValvesRelaysSpecification Or(SafetyValvesRelaysSpecification other) => new OrSafetyValvesRelaysSpecification(this, other);
}

internal sealed class AndSafetyValvesRelaysSpecification(SafetyValvesRelaysSpecification left, SafetyValvesRelaysSpecification right) : SafetyValvesRelaysSpecification
{
    public override Expression<Func<PlantSafety.Domain.MachineSafety.SafetyValvesRelays.Aggregates.SafetyValvesRelays, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.MachineSafety.SafetyValvesRelays.Aggregates.SafetyValvesRelays, bool>>(body, param);
    }
}

internal sealed class OrSafetyValvesRelaysSpecification(SafetyValvesRelaysSpecification left, SafetyValvesRelaysSpecification right) : SafetyValvesRelaysSpecification
{
    public override Expression<Func<PlantSafety.Domain.MachineSafety.SafetyValvesRelays.Aggregates.SafetyValvesRelays, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.MachineSafety.SafetyValvesRelays.Aggregates.SafetyValvesRelays, bool>>(body, param);
    }
}
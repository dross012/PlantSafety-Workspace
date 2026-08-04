// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.LocalEmergencyCoordination
//  Layer:          Domain
//  File:           LocalEmergencyCoordinationSpecification.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for LocalEmergencyCoordination aggregate predicate
//                  expressions. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Specifications;

/// <summary>Abstract base specification for <see cref="Aggregates.LocalEmergencyCoordination"/> aggregate predicate expressions.</summary>
public abstract class LocalEmergencyCoordinationSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Aggregates.LocalEmergencyCoordination, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Aggregates.LocalEmergencyCoordination, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public LocalEmergencyCoordinationSpecification And(LocalEmergencyCoordinationSpecification other) => new AndLocalEmergencyCoordinationSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public LocalEmergencyCoordinationSpecification Or(LocalEmergencyCoordinationSpecification other) => new OrLocalEmergencyCoordinationSpecification(this, other);
}

internal sealed class AndLocalEmergencyCoordinationSpecification(LocalEmergencyCoordinationSpecification left, LocalEmergencyCoordinationSpecification right) : LocalEmergencyCoordinationSpecification
{
    public override Expression<Func<PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Aggregates.LocalEmergencyCoordination, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Aggregates.LocalEmergencyCoordination, bool>>(body, param);
    }
}

internal sealed class OrLocalEmergencyCoordinationSpecification(LocalEmergencyCoordinationSpecification left, LocalEmergencyCoordinationSpecification right) : LocalEmergencyCoordinationSpecification
{
    public override Expression<Func<PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Aggregates.LocalEmergencyCoordination, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Aggregates.LocalEmergencyCoordination, bool>>(body, param);
    }
}

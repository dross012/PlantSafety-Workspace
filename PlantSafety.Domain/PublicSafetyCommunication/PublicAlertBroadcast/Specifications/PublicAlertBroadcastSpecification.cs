// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PublicSafetyCommunication.PublicAlertBroadcast
//  Layer:          Domain
//  File:           PublicAlertBroadcastSpecification.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for PublicAlertBroadcast aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PublicSafetyCommunication.PublicAlertBroadcast.Specifications;

/// <summary>Abstract base specification for <see cref="PublicAlertBroadcast"/> aggregate predicate expressions.</summary>
public abstract class PublicAlertBroadcastSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.PublicSafetyCommunication.PublicAlertBroadcast.Aggregates.PublicAlertBroadcast, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.PublicSafetyCommunication.PublicAlertBroadcast.Aggregates.PublicAlertBroadcast, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public PublicAlertBroadcastSpecification And(PublicAlertBroadcastSpecification other) => new AndPublicAlertBroadcastSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public PublicAlertBroadcastSpecification Or(PublicAlertBroadcastSpecification other) => new OrPublicAlertBroadcastSpecification(this, other);
}

internal sealed class AndPublicAlertBroadcastSpecification(PublicAlertBroadcastSpecification left, PublicAlertBroadcastSpecification right) : PublicAlertBroadcastSpecification
{
    public override Expression<Func<PlantSafety.Domain.PublicSafetyCommunication.PublicAlertBroadcast.Aggregates.PublicAlertBroadcast, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.PublicSafetyCommunication.PublicAlertBroadcast.Aggregates.PublicAlertBroadcast, bool>>(body, param);
    }
}

internal sealed class OrPublicAlertBroadcastSpecification(PublicAlertBroadcastSpecification left, PublicAlertBroadcastSpecification right) : PublicAlertBroadcastSpecification
{
    public override Expression<Func<PlantSafety.Domain.PublicSafetyCommunication.PublicAlertBroadcast.Aggregates.PublicAlertBroadcast, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.PublicSafetyCommunication.PublicAlertBroadcast.Aggregates.PublicAlertBroadcast, bool>>(body, param);
    }
}

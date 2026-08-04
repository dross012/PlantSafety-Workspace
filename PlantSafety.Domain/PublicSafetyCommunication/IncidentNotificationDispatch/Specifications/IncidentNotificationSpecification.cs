// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PublicSafetyCommunication.IncidentNotificationDispatch
//  Layer:          Domain
//  File:           IncidentNotificationSpecification.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for IncidentNotification aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PublicSafetyCommunication.IncidentNotificationDispatch.Specifications;

/// <summary>Abstract base specification for <see cref="IncidentNotification"/> aggregate predicate expressions.</summary>
public abstract class IncidentNotificationSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.PublicSafetyCommunication.IncidentNotificationDispatch.Aggregates.IncidentNotification, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.PublicSafetyCommunication.IncidentNotificationDispatch.Aggregates.IncidentNotification, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public IncidentNotificationSpecification And(IncidentNotificationSpecification other) => new AndIncidentNotificationSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public IncidentNotificationSpecification Or(IncidentNotificationSpecification other) => new OrIncidentNotificationSpecification(this, other);
}

internal sealed class AndIncidentNotificationSpecification(IncidentNotificationSpecification left, IncidentNotificationSpecification right) : IncidentNotificationSpecification
{
    public override Expression<Func<PlantSafety.Domain.PublicSafetyCommunication.IncidentNotificationDispatch.Aggregates.IncidentNotification, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.PublicSafetyCommunication.IncidentNotificationDispatch.Aggregates.IncidentNotification, bool>>(body, param);
    }
}

internal sealed class OrIncidentNotificationSpecification(IncidentNotificationSpecification left, IncidentNotificationSpecification right) : IncidentNotificationSpecification
{
    public override Expression<Func<PlantSafety.Domain.PublicSafetyCommunication.IncidentNotificationDispatch.Aggregates.IncidentNotification, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.PublicSafetyCommunication.IncidentNotificationDispatch.Aggregates.IncidentNotification, bool>>(body, param);
    }
}

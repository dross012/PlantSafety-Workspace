// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PublicSafetyCommunication.CommunicationLog
//  Layer:          Domain
//  File:           CommunicationLogEntrySpecification.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for CommunicationLogEntry aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PublicSafetyCommunication.CommunicationLog.Specifications;

/// <summary>Abstract base specification for <see cref="CommunicationLogEntry"/> aggregate predicate expressions.</summary>
public abstract class CommunicationLogEntrySpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.PublicSafetyCommunication.CommunicationLog.Aggregates.CommunicationLogEntry, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.PublicSafetyCommunication.CommunicationLog.Aggregates.CommunicationLogEntry, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public CommunicationLogEntrySpecification And(CommunicationLogEntrySpecification other) => new AndCommunicationLogEntrySpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public CommunicationLogEntrySpecification Or(CommunicationLogEntrySpecification other) => new OrCommunicationLogEntrySpecification(this, other);
}

internal sealed class AndCommunicationLogEntrySpecification(CommunicationLogEntrySpecification left, CommunicationLogEntrySpecification right) : CommunicationLogEntrySpecification
{
    public override Expression<Func<PlantSafety.Domain.PublicSafetyCommunication.CommunicationLog.Aggregates.CommunicationLogEntry, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.PublicSafetyCommunication.CommunicationLog.Aggregates.CommunicationLogEntry, bool>>(body, param);
    }
}

internal sealed class OrCommunicationLogEntrySpecification(CommunicationLogEntrySpecification left, CommunicationLogEntrySpecification right) : CommunicationLogEntrySpecification
{
    public override Expression<Func<PlantSafety.Domain.PublicSafetyCommunication.CommunicationLog.Aggregates.CommunicationLogEntry, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.PublicSafetyCommunication.CommunicationLog.Aggregates.CommunicationLogEntry, bool>>(body, param);
    }
}

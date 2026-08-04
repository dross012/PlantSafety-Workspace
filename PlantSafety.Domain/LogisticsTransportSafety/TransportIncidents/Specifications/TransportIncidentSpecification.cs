// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.TransportIncidents
//  Layer:          Domain
//  File:           TransportIncidentSpecification.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for TransportIncident aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.TransportIncidents.Specifications;

/// <summary>Abstract base specification for <see cref="TransportIncident"/> aggregate predicate expressions.</summary>
public abstract class TransportIncidentSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.LogisticsTransportSafety.TransportIncidents.Aggregates.TransportIncident, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.LogisticsTransportSafety.TransportIncidents.Aggregates.TransportIncident, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public TransportIncidentSpecification And(TransportIncidentSpecification other) => new AndTransportIncidentSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public TransportIncidentSpecification Or(TransportIncidentSpecification other) => new OrTransportIncidentSpecification(this, other);
}

internal sealed class AndTransportIncidentSpecification(TransportIncidentSpecification left, TransportIncidentSpecification right) : TransportIncidentSpecification
{
    public override Expression<Func<PlantSafety.Domain.LogisticsTransportSafety.TransportIncidents.Aggregates.TransportIncident, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.LogisticsTransportSafety.TransportIncidents.Aggregates.TransportIncident, bool>>(body, param);
    }
}

internal sealed class OrTransportIncidentSpecification(TransportIncidentSpecification left, TransportIncidentSpecification right) : TransportIncidentSpecification
{
    public override Expression<Func<PlantSafety.Domain.LogisticsTransportSafety.TransportIncidents.Aggregates.TransportIncident, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.LogisticsTransportSafety.TransportIncidents.Aggregates.TransportIncident, bool>>(body, param);
    }
}

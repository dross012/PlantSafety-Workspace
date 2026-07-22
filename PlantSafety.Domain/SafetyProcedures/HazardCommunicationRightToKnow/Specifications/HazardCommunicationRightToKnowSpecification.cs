// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.HazardCommunicationRightToKnow
//  Layer:          Domain
//  File:           HazardCommunicationRightToKnowSpecification.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for HazardCommunicationRightToKnow aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.HazardCommunicationRightToKnow.Specifications;

/// <summary>Abstract base specification for <see cref="HazardCommunicationRightToKnow"/> aggregate predicate expressions.</summary>
public abstract class HazardCommunicationRightToKnowSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.SafetyProcedures.HazardCommunicationRightToKnow.Aggregates.HazardCommunicationRightToKnow, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.SafetyProcedures.HazardCommunicationRightToKnow.Aggregates.HazardCommunicationRightToKnow, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public HazardCommunicationRightToKnowSpecification And(HazardCommunicationRightToKnowSpecification other) => new AndHazardCommunicationRightToKnowSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public HazardCommunicationRightToKnowSpecification Or(HazardCommunicationRightToKnowSpecification other) => new OrHazardCommunicationRightToKnowSpecification(this, other);
}

internal sealed class AndHazardCommunicationRightToKnowSpecification(HazardCommunicationRightToKnowSpecification left, HazardCommunicationRightToKnowSpecification right) : HazardCommunicationRightToKnowSpecification
{
    public override Expression<Func<PlantSafety.Domain.SafetyProcedures.HazardCommunicationRightToKnow.Aggregates.HazardCommunicationRightToKnow, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.SafetyProcedures.HazardCommunicationRightToKnow.Aggregates.HazardCommunicationRightToKnow, bool>>(body, param);
    }
}

internal sealed class OrHazardCommunicationRightToKnowSpecification(HazardCommunicationRightToKnowSpecification left, HazardCommunicationRightToKnowSpecification right) : HazardCommunicationRightToKnowSpecification
{
    public override Expression<Func<PlantSafety.Domain.SafetyProcedures.HazardCommunicationRightToKnow.Aggregates.HazardCommunicationRightToKnow, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.SafetyProcedures.HazardCommunicationRightToKnow.Aggregates.HazardCommunicationRightToKnow, bool>>(body, param);
    }
}
// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.EmergencyResponsePlanning
//  Layer:          Domain
//  File:           EmergencyResponsePlanningSpecification.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for EmergencyResponsePlanning aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.EmergencyResponsePlanning.Specifications;

/// <summary>Abstract base specification for <see cref="EmergencyResponsePlanning"/> aggregate predicate expressions.</summary>
public abstract class EmergencyResponsePlanningSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.SafetyProcedures.EmergencyResponsePlanning.Aggregates.EmergencyResponsePlanning, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.SafetyProcedures.EmergencyResponsePlanning.Aggregates.EmergencyResponsePlanning, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public EmergencyResponsePlanningSpecification And(EmergencyResponsePlanningSpecification other) => new AndEmergencyResponsePlanningSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public EmergencyResponsePlanningSpecification Or(EmergencyResponsePlanningSpecification other) => new OrEmergencyResponsePlanningSpecification(this, other);
}

internal sealed class AndEmergencyResponsePlanningSpecification(EmergencyResponsePlanningSpecification left, EmergencyResponsePlanningSpecification right) : EmergencyResponsePlanningSpecification
{
    public override Expression<Func<PlantSafety.Domain.SafetyProcedures.EmergencyResponsePlanning.Aggregates.EmergencyResponsePlanning, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.SafetyProcedures.EmergencyResponsePlanning.Aggregates.EmergencyResponsePlanning, bool>>(body, param);
    }
}

internal sealed class OrEmergencyResponsePlanningSpecification(EmergencyResponsePlanningSpecification left, EmergencyResponsePlanningSpecification right) : EmergencyResponsePlanningSpecification
{
    public override Expression<Func<PlantSafety.Domain.SafetyProcedures.EmergencyResponsePlanning.Aggregates.EmergencyResponsePlanning, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.SafetyProcedures.EmergencyResponsePlanning.Aggregates.EmergencyResponsePlanning, bool>>(body, param);
    }
}
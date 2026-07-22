// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.EmergencyDrillPlanning
//  Layer:          Domain
//  File:           EmergencyDrillPlanningSpecification.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for EmergencyDrillPlanning aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.EmergencyDrillPlanning.Specifications;

/// <summary>Abstract base specification for <see cref="EmergencyDrillPlanning"/> aggregate predicate expressions.</summary>
public abstract class EmergencyDrillPlanningSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.Scheduling.EmergencyDrillPlanning.Aggregates.EmergencyDrillPlanning, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.Scheduling.EmergencyDrillPlanning.Aggregates.EmergencyDrillPlanning, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public EmergencyDrillPlanningSpecification And(EmergencyDrillPlanningSpecification other) => new AndEmergencyDrillPlanningSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public EmergencyDrillPlanningSpecification Or(EmergencyDrillPlanningSpecification other) => new OrEmergencyDrillPlanningSpecification(this, other);
}

internal sealed class AndEmergencyDrillPlanningSpecification(EmergencyDrillPlanningSpecification left, EmergencyDrillPlanningSpecification right) : EmergencyDrillPlanningSpecification
{
    public override Expression<Func<PlantSafety.Domain.Scheduling.EmergencyDrillPlanning.Aggregates.EmergencyDrillPlanning, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.Scheduling.EmergencyDrillPlanning.Aggregates.EmergencyDrillPlanning, bool>>(body, param);
    }
}

internal sealed class OrEmergencyDrillPlanningSpecification(EmergencyDrillPlanningSpecification left, EmergencyDrillPlanningSpecification right) : EmergencyDrillPlanningSpecification
{
    public override Expression<Func<PlantSafety.Domain.Scheduling.EmergencyDrillPlanning.Aggregates.EmergencyDrillPlanning, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.Scheduling.EmergencyDrillPlanning.Aggregates.EmergencyDrillPlanning, bool>>(body, param);
    }
}
// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.RoutineSafetyAuditsIncidentReporting
//  Layer:          Domain
//  File:           RoutineSafetyAuditsIncidentReportingSpecification.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for RoutineSafetyAuditsIncidentReporting aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.RoutineSafetyAuditsIncidentReporting.Specifications;

/// <summary>Abstract base specification for <see cref="RoutineSafetyAuditsIncidentReporting"/> aggregate predicate expressions.</summary>
public abstract class RoutineSafetyAuditsIncidentReportingSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.SafetyProcedures.RoutineSafetyAuditsIncidentReporting.Aggregates.RoutineSafetyAuditsIncidentReporting, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.SafetyProcedures.RoutineSafetyAuditsIncidentReporting.Aggregates.RoutineSafetyAuditsIncidentReporting, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public RoutineSafetyAuditsIncidentReportingSpecification And(RoutineSafetyAuditsIncidentReportingSpecification other) => new AndRoutineSafetyAuditsIncidentReportingSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public RoutineSafetyAuditsIncidentReportingSpecification Or(RoutineSafetyAuditsIncidentReportingSpecification other) => new OrRoutineSafetyAuditsIncidentReportingSpecification(this, other);
}

internal sealed class AndRoutineSafetyAuditsIncidentReportingSpecification(RoutineSafetyAuditsIncidentReportingSpecification left, RoutineSafetyAuditsIncidentReportingSpecification right) : RoutineSafetyAuditsIncidentReportingSpecification
{
    public override Expression<Func<PlantSafety.Domain.SafetyProcedures.RoutineSafetyAuditsIncidentReporting.Aggregates.RoutineSafetyAuditsIncidentReporting, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.SafetyProcedures.RoutineSafetyAuditsIncidentReporting.Aggregates.RoutineSafetyAuditsIncidentReporting, bool>>(body, param);
    }
}

internal sealed class OrRoutineSafetyAuditsIncidentReportingSpecification(RoutineSafetyAuditsIncidentReportingSpecification left, RoutineSafetyAuditsIncidentReportingSpecification right) : RoutineSafetyAuditsIncidentReportingSpecification
{
    public override Expression<Func<PlantSafety.Domain.SafetyProcedures.RoutineSafetyAuditsIncidentReporting.Aggregates.RoutineSafetyAuditsIncidentReporting, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.SafetyProcedures.RoutineSafetyAuditsIncidentReporting.Aggregates.RoutineSafetyAuditsIncidentReporting, bool>>(body, param);
    }
}
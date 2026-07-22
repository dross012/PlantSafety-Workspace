// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.AutomatedIncidentDetection
//  Layer:          Domain
//  File:           AutomatedIncidentDetectionSpecification.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for AutomatedIncidentDetection aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.AutomatedIncidentDetection.Specifications;

/// <summary>Abstract base specification for <see cref="AutomatedIncidentDetection"/> aggregate predicate expressions.</summary>
public abstract class AutomatedIncidentDetectionSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.Observability.AutomatedIncidentDetection.Aggregates.AutomatedIncidentDetection, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.Observability.AutomatedIncidentDetection.Aggregates.AutomatedIncidentDetection, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public AutomatedIncidentDetectionSpecification And(AutomatedIncidentDetectionSpecification other) => new AndAutomatedIncidentDetectionSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public AutomatedIncidentDetectionSpecification Or(AutomatedIncidentDetectionSpecification other) => new OrAutomatedIncidentDetectionSpecification(this, other);
}

internal sealed class AndAutomatedIncidentDetectionSpecification(AutomatedIncidentDetectionSpecification left, AutomatedIncidentDetectionSpecification right) : AutomatedIncidentDetectionSpecification
{
    public override Expression<Func<PlantSafety.Domain.Observability.AutomatedIncidentDetection.Aggregates.AutomatedIncidentDetection, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.Observability.AutomatedIncidentDetection.Aggregates.AutomatedIncidentDetection, bool>>(body, param);
    }
}

internal sealed class OrAutomatedIncidentDetectionSpecification(AutomatedIncidentDetectionSpecification left, AutomatedIncidentDetectionSpecification right) : AutomatedIncidentDetectionSpecification
{
    public override Expression<Func<PlantSafety.Domain.Observability.AutomatedIncidentDetection.Aggregates.AutomatedIncidentDetection, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.Observability.AutomatedIncidentDetection.Aggregates.AutomatedIncidentDetection, bool>>(body, param);
    }
}
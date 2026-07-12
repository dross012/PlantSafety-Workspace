// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.IncidentManagement
//  Layer:          Domain
//  File:           IncidentManagementSpecification.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.IncidentManagement.Specifications;

/// <summary>Abstract base specification for <see cref="IncidentManagement"/> aggregate predicate expressions.</summary>
public abstract class IncidentManagementSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.SafetyEvents.IncidentManagement.Aggregates.IncidentManagement, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.SafetyEvents.IncidentManagement.Aggregates.IncidentManagement, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public IncidentManagementSpecification And(IncidentManagementSpecification other) => new AndIncidentManagementSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public IncidentManagementSpecification Or(IncidentManagementSpecification other) => new OrIncidentManagementSpecification(this, other);
}

internal sealed class AndIncidentManagementSpecification(IncidentManagementSpecification left, IncidentManagementSpecification right) : IncidentManagementSpecification
{
    public override Expression<Func<PlantSafety.Domain.SafetyEvents.IncidentManagement.Aggregates.IncidentManagement, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.SafetyEvents.IncidentManagement.Aggregates.IncidentManagement, bool>>(body, param);
    }
}

internal sealed class OrIncidentManagementSpecification(IncidentManagementSpecification left, IncidentManagementSpecification right) : IncidentManagementSpecification
{
    public override Expression<Func<PlantSafety.Domain.SafetyEvents.IncidentManagement.Aggregates.IncidentManagement, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.SafetyEvents.IncidentManagement.Aggregates.IncidentManagement, bool>>(body, param);
    }
}

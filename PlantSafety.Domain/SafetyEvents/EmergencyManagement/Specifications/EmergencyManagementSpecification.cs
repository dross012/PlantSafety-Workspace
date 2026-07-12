// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.EmergencyManagement
//  Layer:          Domain
//  File:           EmergencyManagementSpecification.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.EmergencyManagement.Specifications;

/// <summary>Abstract base specification for <see cref="EmergencyManagement"/> aggregate predicate expressions.</summary>
public abstract class EmergencyManagementSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.SafetyEvents.EmergencyManagement.Aggregates.EmergencyManagement, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.SafetyEvents.EmergencyManagement.Aggregates.EmergencyManagement, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public EmergencyManagementSpecification And(EmergencyManagementSpecification other) => new AndEmergencyManagementSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public EmergencyManagementSpecification Or(EmergencyManagementSpecification other) => new OrEmergencyManagementSpecification(this, other);
}

internal sealed class AndEmergencyManagementSpecification(EmergencyManagementSpecification left, EmergencyManagementSpecification right) : EmergencyManagementSpecification
{
    public override Expression<Func<PlantSafety.Domain.SafetyEvents.EmergencyManagement.Aggregates.EmergencyManagement, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.SafetyEvents.EmergencyManagement.Aggregates.EmergencyManagement, bool>>(body, param);
    }
}

internal sealed class OrEmergencyManagementSpecification(EmergencyManagementSpecification left, EmergencyManagementSpecification right) : EmergencyManagementSpecification
{
    public override Expression<Func<PlantSafety.Domain.SafetyEvents.EmergencyManagement.Aggregates.EmergencyManagement, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.SafetyEvents.EmergencyManagement.Aggregates.EmergencyManagement, bool>>(body, param);
    }
}

// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.EmergencyPreparedness
//  Layer:          Domain
//  File:           EmergencyPreparednessSpecification.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for EmergencyPreparedness aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.EmergencyPreparedness.Specifications;

/// <summary>Abstract base specification for <see cref="EmergencyPreparedness"/> aggregate predicate expressions.</summary>
public abstract class EmergencyPreparednessSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.EmployeeSafety.EmergencyPreparedness.Aggregates.EmergencyPreparedness, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.EmployeeSafety.EmergencyPreparedness.Aggregates.EmergencyPreparedness, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public EmergencyPreparednessSpecification And(EmergencyPreparednessSpecification other) => new AndEmergencyPreparednessSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public EmergencyPreparednessSpecification Or(EmergencyPreparednessSpecification other) => new OrEmergencyPreparednessSpecification(this, other);
}

internal sealed class AndEmergencyPreparednessSpecification(EmergencyPreparednessSpecification left, EmergencyPreparednessSpecification right) : EmergencyPreparednessSpecification
{
    public override Expression<Func<PlantSafety.Domain.EmployeeSafety.EmergencyPreparedness.Aggregates.EmergencyPreparedness, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.EmployeeSafety.EmergencyPreparedness.Aggregates.EmergencyPreparedness, bool>>(body, param);
    }
}

internal sealed class OrEmergencyPreparednessSpecification(EmergencyPreparednessSpecification left, EmergencyPreparednessSpecification right) : EmergencyPreparednessSpecification
{
    public override Expression<Func<PlantSafety.Domain.EmployeeSafety.EmergencyPreparedness.Aggregates.EmergencyPreparedness, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.EmployeeSafety.EmergencyPreparedness.Aggregates.EmergencyPreparedness, bool>>(body, param);
    }
}
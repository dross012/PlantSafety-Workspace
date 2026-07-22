// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.MachineGuardingInspections
//  Layer:          Domain
//  File:           MachineGuardingInspectionsSpecification.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for MachineGuardingInspections aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.MachineGuardingInspections.Specifications;

/// <summary>Abstract base specification for <see cref="MachineGuardingInspections"/> aggregate predicate expressions.</summary>
public abstract class MachineGuardingInspectionsSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.SafetyProcedures.MachineGuardingInspections.Aggregates.MachineGuardingInspections, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.SafetyProcedures.MachineGuardingInspections.Aggregates.MachineGuardingInspections, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public MachineGuardingInspectionsSpecification And(MachineGuardingInspectionsSpecification other) => new AndMachineGuardingInspectionsSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public MachineGuardingInspectionsSpecification Or(MachineGuardingInspectionsSpecification other) => new OrMachineGuardingInspectionsSpecification(this, other);
}

internal sealed class AndMachineGuardingInspectionsSpecification(MachineGuardingInspectionsSpecification left, MachineGuardingInspectionsSpecification right) : MachineGuardingInspectionsSpecification
{
    public override Expression<Func<PlantSafety.Domain.SafetyProcedures.MachineGuardingInspections.Aggregates.MachineGuardingInspections, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.SafetyProcedures.MachineGuardingInspections.Aggregates.MachineGuardingInspections, bool>>(body, param);
    }
}

internal sealed class OrMachineGuardingInspectionsSpecification(MachineGuardingInspectionsSpecification left, MachineGuardingInspectionsSpecification right) : MachineGuardingInspectionsSpecification
{
    public override Expression<Func<PlantSafety.Domain.SafetyProcedures.MachineGuardingInspections.Aggregates.MachineGuardingInspections, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.SafetyProcedures.MachineGuardingInspections.Aggregates.MachineGuardingInspections, bool>>(body, param);
    }
}
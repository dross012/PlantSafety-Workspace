// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.MachineGuarding
//  Layer:          Domain
//  File:           MachineGuardingSpecification.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for MachineGuarding aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.MachineGuarding.Specifications;

/// <summary>Abstract base specification for <see cref="MachineGuarding"/> aggregate predicate expressions.</summary>
public abstract class MachineGuardingSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.EmployeeSafety.MachineGuarding.Aggregates.MachineGuarding, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.EmployeeSafety.MachineGuarding.Aggregates.MachineGuarding, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public MachineGuardingSpecification And(MachineGuardingSpecification other) => new AndMachineGuardingSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public MachineGuardingSpecification Or(MachineGuardingSpecification other) => new OrMachineGuardingSpecification(this, other);
}

internal sealed class AndMachineGuardingSpecification(MachineGuardingSpecification left, MachineGuardingSpecification right) : MachineGuardingSpecification
{
    public override Expression<Func<PlantSafety.Domain.EmployeeSafety.MachineGuarding.Aggregates.MachineGuarding, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.EmployeeSafety.MachineGuarding.Aggregates.MachineGuarding, bool>>(body, param);
    }
}

internal sealed class OrMachineGuardingSpecification(MachineGuardingSpecification left, MachineGuardingSpecification right) : MachineGuardingSpecification
{
    public override Expression<Func<PlantSafety.Domain.EmployeeSafety.MachineGuarding.Aggregates.MachineGuarding, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.EmployeeSafety.MachineGuarding.Aggregates.MachineGuarding, bool>>(body, param);
    }
}
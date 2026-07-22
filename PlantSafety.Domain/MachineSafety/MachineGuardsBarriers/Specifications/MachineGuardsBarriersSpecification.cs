// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.MachineGuardsBarriers
//  Layer:          Domain
//  File:           MachineGuardsBarriersSpecification.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for MachineGuardsBarriers aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.MachineGuardsBarriers.Specifications;

/// <summary>Abstract base specification for <see cref="MachineGuardsBarriers"/> aggregate predicate expressions.</summary>
public abstract class MachineGuardsBarriersSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.MachineSafety.MachineGuardsBarriers.Aggregates.MachineGuardsBarriers, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.MachineSafety.MachineGuardsBarriers.Aggregates.MachineGuardsBarriers, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public MachineGuardsBarriersSpecification And(MachineGuardsBarriersSpecification other) => new AndMachineGuardsBarriersSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public MachineGuardsBarriersSpecification Or(MachineGuardsBarriersSpecification other) => new OrMachineGuardsBarriersSpecification(this, other);
}

internal sealed class AndMachineGuardsBarriersSpecification(MachineGuardsBarriersSpecification left, MachineGuardsBarriersSpecification right) : MachineGuardsBarriersSpecification
{
    public override Expression<Func<PlantSafety.Domain.MachineSafety.MachineGuardsBarriers.Aggregates.MachineGuardsBarriers, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.MachineSafety.MachineGuardsBarriers.Aggregates.MachineGuardsBarriers, bool>>(body, param);
    }
}

internal sealed class OrMachineGuardsBarriersSpecification(MachineGuardsBarriersSpecification left, MachineGuardsBarriersSpecification right) : MachineGuardsBarriersSpecification
{
    public override Expression<Func<PlantSafety.Domain.MachineSafety.MachineGuardsBarriers.Aggregates.MachineGuardsBarriers, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.MachineSafety.MachineGuardsBarriers.Aggregates.MachineGuardsBarriers, bool>>(body, param);
    }
}
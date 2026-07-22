// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety.PermitToWork
//  Layer:          Domain
//  File:           PermitToWorkSpecification.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for PermitToWork aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorSafety.PermitToWork.Specifications;

/// <summary>Abstract base specification for <see cref="PermitToWork"/> aggregate predicate expressions.</summary>
public abstract class PermitToWorkSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.ContractorSafety.PermitToWork.Aggregates.PermitToWork, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.ContractorSafety.PermitToWork.Aggregates.PermitToWork, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public PermitToWorkSpecification And(PermitToWorkSpecification other) => new AndPermitToWorkSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public PermitToWorkSpecification Or(PermitToWorkSpecification other) => new OrPermitToWorkSpecification(this, other);
}

internal sealed class AndPermitToWorkSpecification(PermitToWorkSpecification left, PermitToWorkSpecification right) : PermitToWorkSpecification
{
    public override Expression<Func<PlantSafety.Domain.ContractorSafety.PermitToWork.Aggregates.PermitToWork, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.ContractorSafety.PermitToWork.Aggregates.PermitToWork, bool>>(body, param);
    }
}

internal sealed class OrPermitToWorkSpecification(PermitToWorkSpecification left, PermitToWorkSpecification right) : PermitToWorkSpecification
{
    public override Expression<Func<PlantSafety.Domain.ContractorSafety.PermitToWork.Aggregates.PermitToWork, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.ContractorSafety.PermitToWork.Aggregates.PermitToWork, bool>>(body, param);
    }
}
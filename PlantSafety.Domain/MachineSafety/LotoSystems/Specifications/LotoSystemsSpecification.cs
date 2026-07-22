// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.LotoSystems
//  Layer:          Domain
//  File:           LotoSystemsSpecification.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for LotoSystems aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.LotoSystems.Specifications;

/// <summary>Abstract base specification for <see cref="LotoSystems"/> aggregate predicate expressions.</summary>
public abstract class LotoSystemsSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.MachineSafety.LotoSystems.Aggregates.LotoSystems, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.MachineSafety.LotoSystems.Aggregates.LotoSystems, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public LotoSystemsSpecification And(LotoSystemsSpecification other) => new AndLotoSystemsSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public LotoSystemsSpecification Or(LotoSystemsSpecification other) => new OrLotoSystemsSpecification(this, other);
}

internal sealed class AndLotoSystemsSpecification(LotoSystemsSpecification left, LotoSystemsSpecification right) : LotoSystemsSpecification
{
    public override Expression<Func<PlantSafety.Domain.MachineSafety.LotoSystems.Aggregates.LotoSystems, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.MachineSafety.LotoSystems.Aggregates.LotoSystems, bool>>(body, param);
    }
}

internal sealed class OrLotoSystemsSpecification(LotoSystemsSpecification left, LotoSystemsSpecification right) : LotoSystemsSpecification
{
    public override Expression<Func<PlantSafety.Domain.MachineSafety.LotoSystems.Aggregates.LotoSystems, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.MachineSafety.LotoSystems.Aggregates.LotoSystems, bool>>(body, param);
    }
}
// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPEInventory
//  Layer:          Domain
//  File:           PPEInventorySpecification.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for PPEInventory aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PPE.PPEInventory.Specifications;

/// <summary>Abstract base specification for <see cref="PPEInventory"/> aggregate predicate expressions.</summary>
public abstract class PPEInventorySpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.PPE.PPEInventory.Aggregates.PPEInventory, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.PPE.PPEInventory.Aggregates.PPEInventory, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public PPEInventorySpecification And(PPEInventorySpecification other) => new AndPPEInventorySpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public PPEInventorySpecification Or(PPEInventorySpecification other) => new OrPPEInventorySpecification(this, other);
}

internal sealed class AndPPEInventorySpecification(PPEInventorySpecification left, PPEInventorySpecification right) : PPEInventorySpecification
{
    public override Expression<Func<PlantSafety.Domain.PPE.PPEInventory.Aggregates.PPEInventory, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.PPE.PPEInventory.Aggregates.PPEInventory, bool>>(body, param);
    }
}

internal sealed class OrPPEInventorySpecification(PPEInventorySpecification left, PPEInventorySpecification right) : PPEInventorySpecification
{
    public override Expression<Func<PlantSafety.Domain.PPE.PPEInventory.Aggregates.PPEInventory, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.PPE.PPEInventory.Aggregates.PPEInventory, bool>>(body, param);
    }
}
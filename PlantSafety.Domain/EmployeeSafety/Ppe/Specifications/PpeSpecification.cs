// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.Ppe
//  Layer:          Domain
//  File:           PpeSpecification.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for Ppe aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.Ppe.Specifications;

/// <summary>Abstract base specification for <see cref="Ppe"/> aggregate predicate expressions.</summary>
public abstract class PpeSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.EmployeeSafety.Ppe.Aggregates.Ppe, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.EmployeeSafety.Ppe.Aggregates.Ppe, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public PpeSpecification And(PpeSpecification other) => new AndPpeSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public PpeSpecification Or(PpeSpecification other) => new OrPpeSpecification(this, other);
}

internal sealed class AndPpeSpecification(PpeSpecification left, PpeSpecification right) : PpeSpecification
{
    public override Expression<Func<PlantSafety.Domain.EmployeeSafety.Ppe.Aggregates.Ppe, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.EmployeeSafety.Ppe.Aggregates.Ppe, bool>>(body, param);
    }
}

internal sealed class OrPpeSpecification(PpeSpecification left, PpeSpecification right) : PpeSpecification
{
    public override Expression<Func<PlantSafety.Domain.EmployeeSafety.Ppe.Aggregates.Ppe, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.EmployeeSafety.Ppe.Aggregates.Ppe, bool>>(body, param);
    }
}
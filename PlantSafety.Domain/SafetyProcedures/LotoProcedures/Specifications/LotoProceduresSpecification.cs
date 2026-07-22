// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.LotoProcedures
//  Layer:          Domain
//  File:           LotoProceduresSpecification.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for LotoProcedures aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.LotoProcedures.Specifications;

/// <summary>Abstract base specification for <see cref="LotoProcedures"/> aggregate predicate expressions.</summary>
public abstract class LotoProceduresSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.SafetyProcedures.LotoProcedures.Aggregates.LotoProcedures, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.SafetyProcedures.LotoProcedures.Aggregates.LotoProcedures, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public LotoProceduresSpecification And(LotoProceduresSpecification other) => new AndLotoProceduresSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public LotoProceduresSpecification Or(LotoProceduresSpecification other) => new OrLotoProceduresSpecification(this, other);
}

internal sealed class AndLotoProceduresSpecification(LotoProceduresSpecification left, LotoProceduresSpecification right) : LotoProceduresSpecification
{
    public override Expression<Func<PlantSafety.Domain.SafetyProcedures.LotoProcedures.Aggregates.LotoProcedures, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.SafetyProcedures.LotoProcedures.Aggregates.LotoProcedures, bool>>(body, param);
    }
}

internal sealed class OrLotoProceduresSpecification(LotoProceduresSpecification left, LotoProceduresSpecification right) : LotoProceduresSpecification
{
    public override Expression<Func<PlantSafety.Domain.SafetyProcedures.LotoProcedures.Aggregates.LotoProcedures, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.SafetyProcedures.LotoProcedures.Aggregates.LotoProcedures, bool>>(body, param);
    }
}
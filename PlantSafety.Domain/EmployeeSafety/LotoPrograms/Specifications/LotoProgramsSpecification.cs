// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.LotoPrograms
//  Layer:          Domain
//  File:           LotoProgramsSpecification.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for LotoPrograms aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.LotoPrograms.Specifications;

/// <summary>Abstract base specification for <see cref="LotoPrograms"/> aggregate predicate expressions.</summary>
public abstract class LotoProgramsSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.EmployeeSafety.LotoPrograms.Aggregates.LotoPrograms, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.EmployeeSafety.LotoPrograms.Aggregates.LotoPrograms, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public LotoProgramsSpecification And(LotoProgramsSpecification other) => new AndLotoProgramsSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public LotoProgramsSpecification Or(LotoProgramsSpecification other) => new OrLotoProgramsSpecification(this, other);
}

internal sealed class AndLotoProgramsSpecification(LotoProgramsSpecification left, LotoProgramsSpecification right) : LotoProgramsSpecification
{
    public override Expression<Func<PlantSafety.Domain.EmployeeSafety.LotoPrograms.Aggregates.LotoPrograms, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.EmployeeSafety.LotoPrograms.Aggregates.LotoPrograms, bool>>(body, param);
    }
}

internal sealed class OrLotoProgramsSpecification(LotoProgramsSpecification left, LotoProgramsSpecification right) : LotoProgramsSpecification
{
    public override Expression<Func<PlantSafety.Domain.EmployeeSafety.LotoPrograms.Aggregates.LotoPrograms, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.EmployeeSafety.LotoPrograms.Aggregates.LotoPrograms, bool>>(body, param);
    }
}
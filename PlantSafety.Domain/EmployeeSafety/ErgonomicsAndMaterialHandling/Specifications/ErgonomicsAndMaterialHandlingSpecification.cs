// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.ErgonomicsAndMaterialHandling
//  Layer:          Domain
//  File:           ErgonomicsAndMaterialHandlingSpecification.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for ErgonomicsAndMaterialHandling aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.ErgonomicsAndMaterialHandling.Specifications;

/// <summary>Abstract base specification for <see cref="ErgonomicsAndMaterialHandling"/> aggregate predicate expressions.</summary>
public abstract class ErgonomicsAndMaterialHandlingSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.EmployeeSafety.ErgonomicsAndMaterialHandling.Aggregates.ErgonomicsAndMaterialHandling, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.EmployeeSafety.ErgonomicsAndMaterialHandling.Aggregates.ErgonomicsAndMaterialHandling, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public ErgonomicsAndMaterialHandlingSpecification And(ErgonomicsAndMaterialHandlingSpecification other) => new AndErgonomicsAndMaterialHandlingSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public ErgonomicsAndMaterialHandlingSpecification Or(ErgonomicsAndMaterialHandlingSpecification other) => new OrErgonomicsAndMaterialHandlingSpecification(this, other);
}

internal sealed class AndErgonomicsAndMaterialHandlingSpecification(ErgonomicsAndMaterialHandlingSpecification left, ErgonomicsAndMaterialHandlingSpecification right) : ErgonomicsAndMaterialHandlingSpecification
{
    public override Expression<Func<PlantSafety.Domain.EmployeeSafety.ErgonomicsAndMaterialHandling.Aggregates.ErgonomicsAndMaterialHandling, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.EmployeeSafety.ErgonomicsAndMaterialHandling.Aggregates.ErgonomicsAndMaterialHandling, bool>>(body, param);
    }
}

internal sealed class OrErgonomicsAndMaterialHandlingSpecification(ErgonomicsAndMaterialHandlingSpecification left, ErgonomicsAndMaterialHandlingSpecification right) : ErgonomicsAndMaterialHandlingSpecification
{
    public override Expression<Func<PlantSafety.Domain.EmployeeSafety.ErgonomicsAndMaterialHandling.Aggregates.ErgonomicsAndMaterialHandling, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.EmployeeSafety.ErgonomicsAndMaterialHandling.Aggregates.ErgonomicsAndMaterialHandling, bool>>(body, param);
    }
}
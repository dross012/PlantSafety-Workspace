// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.CraneHoistSystems
//  Layer:          Domain
//  File:           CraneHoistSystemsSpecification.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Abstract base specification for CraneHoistSystems aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.CraneHoistSystems.Specifications;

/// <summary>Abstract base specification for <see cref="CraneHoistSystems"/> aggregate predicate expressions.</summary>
public abstract class CraneHoistSystemsSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.MachineSafety.CraneHoistSystems.Aggregates.CraneHoistSystems, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.MachineSafety.CraneHoistSystems.Aggregates.CraneHoistSystems, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public CraneHoistSystemsSpecification And(CraneHoistSystemsSpecification other) => new AndCraneHoistSystemsSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public CraneHoistSystemsSpecification Or(CraneHoistSystemsSpecification other) => new OrCraneHoistSystemsSpecification(this, other);
}

internal sealed class AndCraneHoistSystemsSpecification(CraneHoistSystemsSpecification left, CraneHoistSystemsSpecification right) : CraneHoistSystemsSpecification
{
    public override Expression<Func<PlantSafety.Domain.MachineSafety.CraneHoistSystems.Aggregates.CraneHoistSystems, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.MachineSafety.CraneHoistSystems.Aggregates.CraneHoistSystems, bool>>(body, param);
    }
}

internal sealed class OrCraneHoistSystemsSpecification(CraneHoistSystemsSpecification left, CraneHoistSystemsSpecification right) : CraneHoistSystemsSpecification
{
    public override Expression<Func<PlantSafety.Domain.MachineSafety.CraneHoistSystems.Aggregates.CraneHoistSystems, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.MachineSafety.CraneHoistSystems.Aggregates.CraneHoistSystems, bool>>(body, param);
    }
}

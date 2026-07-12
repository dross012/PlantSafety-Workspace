// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Procedures
//  Layer:          Domain
//  File:           ProceduresSpecification.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Procedures.Specifications;

/// <summary>Abstract base specification for <see cref="Procedures"/> aggregate predicate expressions.</summary>
public abstract class ProceduresSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.Compliance.Procedures.Aggregates.Procedures, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.Compliance.Procedures.Aggregates.Procedures, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public ProceduresSpecification And(ProceduresSpecification other) => new AndProceduresSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public ProceduresSpecification Or(ProceduresSpecification other) => new OrProceduresSpecification(this, other);
}

internal sealed class AndProceduresSpecification(ProceduresSpecification left, ProceduresSpecification right) : ProceduresSpecification
{
    public override Expression<Func<PlantSafety.Domain.Compliance.Procedures.Aggregates.Procedures, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.Compliance.Procedures.Aggregates.Procedures, bool>>(body, param);
    }
}

internal sealed class OrProceduresSpecification(ProceduresSpecification left, ProceduresSpecification right) : ProceduresSpecification
{
    public override Expression<Func<PlantSafety.Domain.Compliance.Procedures.Aggregates.Procedures, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.Compliance.Procedures.Aggregates.Procedures, bool>>(body, param);
    }
}

// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Compliance
//  Layer:          Domain
//  File:           ComplianceSpecification.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Compliance.Specifications;

/// <summary>Abstract base specification for <see cref="Compliance"/> aggregate predicate expressions.</summary>
public abstract class ComplianceSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<Compliance, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<Compliance, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public ComplianceSpecification And(ComplianceSpecification other) => new AndComplianceSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public ComplianceSpecification Or(ComplianceSpecification other) => new OrComplianceSpecification(this, other);
}

internal sealed class AndComplianceSpecification(ComplianceSpecification left, ComplianceSpecification right) : ComplianceSpecification
{
    public override Expression<Func<Compliance, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<Compliance, bool>>(body, param);
    }
}

internal sealed class OrComplianceSpecification(ComplianceSpecification left, ComplianceSpecification right) : ComplianceSpecification
{
    public override Expression<Func<Compliance, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<Compliance, bool>>(body, param);
    }
}

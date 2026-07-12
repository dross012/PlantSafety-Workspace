// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PPEManagement
//  Layer:          Domain
//  File:           PPEManagementSpecification.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.PPEManagement.Specifications;

/// <summary>Abstract base specification for <see cref="PPEManagement"/> aggregate predicate expressions.</summary>
public abstract class PPEManagementSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.Operational.PPEManagement.Aggregates.PPEManagement, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.Operational.PPEManagement.Aggregates.PPEManagement, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public PPEManagementSpecification And(PPEManagementSpecification other) => new AndPPEManagementSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public PPEManagementSpecification Or(PPEManagementSpecification other) => new OrPPEManagementSpecification(this, other);
}

internal sealed class AndPPEManagementSpecification(PPEManagementSpecification left, PPEManagementSpecification right) : PPEManagementSpecification
{
    public override Expression<Func<PlantSafety.Domain.Operational.PPEManagement.Aggregates.PPEManagement, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.Operational.PPEManagement.Aggregates.PPEManagement, bool>>(body, param);
    }
}

internal sealed class OrPPEManagementSpecification(PPEManagementSpecification left, PPEManagementSpecification right) : PPEManagementSpecification
{
    public override Expression<Func<PlantSafety.Domain.Operational.PPEManagement.Aggregates.PPEManagement, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.Operational.PPEManagement.Aggregates.PPEManagement, bool>>(body, param);
    }
}

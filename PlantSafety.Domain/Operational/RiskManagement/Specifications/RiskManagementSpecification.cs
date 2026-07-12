// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.RiskManagement
//  Layer:          Domain
//  File:           RiskManagementSpecification.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.RiskManagement.Specifications;

/// <summary>Abstract base specification for <see cref="RiskManagement"/> aggregate predicate expressions.</summary>
public abstract class RiskManagementSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.Operational.RiskManagement.Aggregates.RiskManagement, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.Operational.RiskManagement.Aggregates.RiskManagement, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public RiskManagementSpecification And(RiskManagementSpecification other) => new AndRiskManagementSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public RiskManagementSpecification Or(RiskManagementSpecification other) => new OrRiskManagementSpecification(this, other);
}

internal sealed class AndRiskManagementSpecification(RiskManagementSpecification left, RiskManagementSpecification right) : RiskManagementSpecification
{
    public override Expression<Func<PlantSafety.Domain.Operational.RiskManagement.Aggregates.RiskManagement, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.Operational.RiskManagement.Aggregates.RiskManagement, bool>>(body, param);
    }
}

internal sealed class OrRiskManagementSpecification(RiskManagementSpecification left, RiskManagementSpecification right) : RiskManagementSpecification
{
    public override Expression<Func<PlantSafety.Domain.Operational.RiskManagement.Aggregates.RiskManagement, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.Operational.RiskManagement.Aggregates.RiskManagement, bool>>(body, param);
    }
}

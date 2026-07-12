// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.ManagementOfChange
//  Layer:          Domain
//  File:           ManagementOfChangeSpecification.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.ManagementOfChange.Specifications;

/// <summary>Abstract base specification for <see cref="ManagementOfChange"/> aggregate predicate expressions.</summary>
public abstract class ManagementOfChangeSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.Compliance.ManagementOfChange.Aggregates.ManagementOfChange, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.Compliance.ManagementOfChange.Aggregates.ManagementOfChange, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public ManagementOfChangeSpecification And(ManagementOfChangeSpecification other) => new AndManagementOfChangeSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public ManagementOfChangeSpecification Or(ManagementOfChangeSpecification other) => new OrManagementOfChangeSpecification(this, other);
}

internal sealed class AndManagementOfChangeSpecification(ManagementOfChangeSpecification left, ManagementOfChangeSpecification right) : ManagementOfChangeSpecification
{
    public override Expression<Func<PlantSafety.Domain.Compliance.ManagementOfChange.Aggregates.ManagementOfChange, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.Compliance.ManagementOfChange.Aggregates.ManagementOfChange, bool>>(body, param);
    }
}

internal sealed class OrManagementOfChangeSpecification(ManagementOfChangeSpecification left, ManagementOfChangeSpecification right) : ManagementOfChangeSpecification
{
    public override Expression<Func<PlantSafety.Domain.Compliance.ManagementOfChange.Aggregates.ManagementOfChange, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.Compliance.ManagementOfChange.Aggregates.ManagementOfChange, bool>>(body, param);
    }
}

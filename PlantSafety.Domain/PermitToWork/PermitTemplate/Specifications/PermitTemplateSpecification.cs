// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PermitToWork.PermitTemplate
//  Layer:          Domain
//  File:           PermitTemplateSpecification.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for PermitTemplate aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PermitToWork.PermitTemplate.Specifications;

/// <summary>Abstract base specification for <see cref="PermitTemplate"/> aggregate predicate expressions.</summary>
public abstract class PermitTemplateSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.PermitToWork.PermitTemplate.Aggregates.PermitTemplate, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.PermitToWork.PermitTemplate.Aggregates.PermitTemplate, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public PermitTemplateSpecification And(PermitTemplateSpecification other) => new AndPermitTemplateSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public PermitTemplateSpecification Or(PermitTemplateSpecification other) => new OrPermitTemplateSpecification(this, other);
}

internal sealed class AndPermitTemplateSpecification(PermitTemplateSpecification left, PermitTemplateSpecification right) : PermitTemplateSpecification
{
    public override Expression<Func<PlantSafety.Domain.PermitToWork.PermitTemplate.Aggregates.PermitTemplate, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.PermitToWork.PermitTemplate.Aggregates.PermitTemplate, bool>>(body, param);
    }
}

internal sealed class OrPermitTemplateSpecification(PermitTemplateSpecification left, PermitTemplateSpecification right) : PermitTemplateSpecification
{
    public override Expression<Func<PlantSafety.Domain.PermitToWork.PermitTemplate.Aggregates.PermitTemplate, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.PermitToWork.PermitTemplate.Aggregates.PermitTemplate, bool>>(body, param);
    }
}
// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Checklists.ChecklistTemplate
//  Layer:          Domain
//  File:           ChecklistTemplateSpecification.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for ChecklistTemplate aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Checklists.ChecklistTemplate.Specifications;

/// <summary>Abstract base specification for <see cref="ChecklistTemplate"/> aggregate predicate expressions.</summary>
public abstract class ChecklistTemplateSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.Checklists.ChecklistTemplate.Aggregates.ChecklistTemplate, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.Checklists.ChecklistTemplate.Aggregates.ChecklistTemplate, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public ChecklistTemplateSpecification And(ChecklistTemplateSpecification other) => new AndChecklistTemplateSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public ChecklistTemplateSpecification Or(ChecklistTemplateSpecification other) => new OrChecklistTemplateSpecification(this, other);
}

internal sealed class AndChecklistTemplateSpecification(ChecklistTemplateSpecification left, ChecklistTemplateSpecification right) : ChecklistTemplateSpecification
{
    public override Expression<Func<PlantSafety.Domain.Checklists.ChecklistTemplate.Aggregates.ChecklistTemplate, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.Checklists.ChecklistTemplate.Aggregates.ChecklistTemplate, bool>>(body, param);
    }
}

internal sealed class OrChecklistTemplateSpecification(ChecklistTemplateSpecification left, ChecklistTemplateSpecification right) : ChecklistTemplateSpecification
{
    public override Expression<Func<PlantSafety.Domain.Checklists.ChecklistTemplate.Aggregates.ChecklistTemplate, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.Checklists.ChecklistTemplate.Aggregates.ChecklistTemplate, bool>>(body, param);
    }
}
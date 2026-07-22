// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.HazardCommunicationTraining
//  Layer:          Domain
//  File:           HazardCommunicationTrainingSpecification.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for HazardCommunicationTraining aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.HazardCommunicationTraining.Specifications;

/// <summary>Abstract base specification for <see cref="HazardCommunicationTraining"/> aggregate predicate expressions.</summary>
public abstract class HazardCommunicationTrainingSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.EmployeeSafety.HazardCommunicationTraining.Aggregates.HazardCommunicationTraining, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.EmployeeSafety.HazardCommunicationTraining.Aggregates.HazardCommunicationTraining, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public HazardCommunicationTrainingSpecification And(HazardCommunicationTrainingSpecification other) => new AndHazardCommunicationTrainingSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public HazardCommunicationTrainingSpecification Or(HazardCommunicationTrainingSpecification other) => new OrHazardCommunicationTrainingSpecification(this, other);
}

internal sealed class AndHazardCommunicationTrainingSpecification(HazardCommunicationTrainingSpecification left, HazardCommunicationTrainingSpecification right) : HazardCommunicationTrainingSpecification
{
    public override Expression<Func<PlantSafety.Domain.EmployeeSafety.HazardCommunicationTraining.Aggregates.HazardCommunicationTraining, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.EmployeeSafety.HazardCommunicationTraining.Aggregates.HazardCommunicationTraining, bool>>(body, param);
    }
}

internal sealed class OrHazardCommunicationTrainingSpecification(HazardCommunicationTrainingSpecification left, HazardCommunicationTrainingSpecification right) : HazardCommunicationTrainingSpecification
{
    public override Expression<Func<PlantSafety.Domain.EmployeeSafety.HazardCommunicationTraining.Aggregates.HazardCommunicationTraining, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.EmployeeSafety.HazardCommunicationTraining.Aggregates.HazardCommunicationTraining, bool>>(body, param);
    }
}
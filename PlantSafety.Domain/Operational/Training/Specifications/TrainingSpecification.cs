// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Training
//  Layer:          Domain
//  File:           TrainingSpecification.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.Training.Specifications;

/// <summary>Abstract base specification for <see cref="Training"/> aggregate predicate expressions.</summary>
public abstract class TrainingSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.Operational.Training.Aggregates.Training, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.Operational.Training.Aggregates.Training, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public TrainingSpecification And(TrainingSpecification other) => new AndTrainingSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public TrainingSpecification Or(TrainingSpecification other) => new OrTrainingSpecification(this, other);
}

internal sealed class AndTrainingSpecification(TrainingSpecification left, TrainingSpecification right) : TrainingSpecification
{
    public override Expression<Func<PlantSafety.Domain.Operational.Training.Aggregates.Training, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.Operational.Training.Aggregates.Training, bool>>(body, param);
    }
}

internal sealed class OrTrainingSpecification(TrainingSpecification left, TrainingSpecification right) : TrainingSpecification
{
    public override Expression<Func<PlantSafety.Domain.Operational.Training.Aggregates.Training, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.Operational.Training.Aggregates.Training, bool>>(body, param);
    }
}

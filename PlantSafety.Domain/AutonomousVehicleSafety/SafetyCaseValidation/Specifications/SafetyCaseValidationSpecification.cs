// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.SafetyCaseValidation
//  Layer:          Domain
//  File:           SafetyCaseValidationSpecification.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for SafetyCaseValidation aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.SafetyCaseValidation.Specifications;

/// <summary>Abstract base specification for <see cref="Aggregates.SafetyCaseValidation"/> aggregate predicate expressions.</summary>
public abstract class SafetyCaseValidationSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<Aggregates.SafetyCaseValidation, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<Aggregates.SafetyCaseValidation, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public SafetyCaseValidationSpecification And(SafetyCaseValidationSpecification other) => new AndSafetyCaseValidationSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public SafetyCaseValidationSpecification Or(SafetyCaseValidationSpecification other) => new OrSafetyCaseValidationSpecification(this, other);
}

internal sealed class AndSafetyCaseValidationSpecification(SafetyCaseValidationSpecification left, SafetyCaseValidationSpecification right) : SafetyCaseValidationSpecification
{
    public override Expression<Func<Aggregates.SafetyCaseValidation, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<Aggregates.SafetyCaseValidation, bool>>(body, param);
    }
}

internal sealed class OrSafetyCaseValidationSpecification(SafetyCaseValidationSpecification left, SafetyCaseValidationSpecification right) : SafetyCaseValidationSpecification
{
    public override Expression<Func<Aggregates.SafetyCaseValidation, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<Aggregates.SafetyCaseValidation, bool>>(body, param);
    }
}

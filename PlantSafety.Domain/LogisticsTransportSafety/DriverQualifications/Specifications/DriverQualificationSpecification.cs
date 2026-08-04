// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.DriverQualifications
//  Layer:          Domain
//  File:           DriverQualificationSpecification.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for DriverQualification aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.DriverQualifications.Specifications;

/// <summary>Abstract base specification for <see cref="DriverQualification"/> aggregate predicate expressions.</summary>
public abstract class DriverQualificationSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.LogisticsTransportSafety.DriverQualifications.Aggregates.DriverQualification, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.LogisticsTransportSafety.DriverQualifications.Aggregates.DriverQualification, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public DriverQualificationSpecification And(DriverQualificationSpecification other) => new AndDriverQualificationSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public DriverQualificationSpecification Or(DriverQualificationSpecification other) => new OrDriverQualificationSpecification(this, other);
}

internal sealed class AndDriverQualificationSpecification(DriverQualificationSpecification left, DriverQualificationSpecification right) : DriverQualificationSpecification
{
    public override Expression<Func<PlantSafety.Domain.LogisticsTransportSafety.DriverQualifications.Aggregates.DriverQualification, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.LogisticsTransportSafety.DriverQualifications.Aggregates.DriverQualification, bool>>(body, param);
    }
}

internal sealed class OrDriverQualificationSpecification(DriverQualificationSpecification left, DriverQualificationSpecification right) : DriverQualificationSpecification
{
    public override Expression<Func<PlantSafety.Domain.LogisticsTransportSafety.DriverQualifications.Aggregates.DriverQualification, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.LogisticsTransportSafety.DriverQualifications.Aggregates.DriverQualification, bool>>(body, param);
    }
}

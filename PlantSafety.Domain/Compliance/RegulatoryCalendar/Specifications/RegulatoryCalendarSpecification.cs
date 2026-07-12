// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.RegulatoryCalendar
//  Layer:          Domain
//  File:           RegulatoryCalendarSpecification.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.RegulatoryCalendar.Specifications;

/// <summary>Abstract base specification for <see cref="RegulatoryCalendar"/> aggregate predicate expressions.</summary>
public abstract class RegulatoryCalendarSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.Compliance.RegulatoryCalendar.Aggregates.RegulatoryCalendar, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.Compliance.RegulatoryCalendar.Aggregates.RegulatoryCalendar, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public RegulatoryCalendarSpecification And(RegulatoryCalendarSpecification other) => new AndRegulatoryCalendarSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public RegulatoryCalendarSpecification Or(RegulatoryCalendarSpecification other) => new OrRegulatoryCalendarSpecification(this, other);
}

internal sealed class AndRegulatoryCalendarSpecification(RegulatoryCalendarSpecification left, RegulatoryCalendarSpecification right) : RegulatoryCalendarSpecification
{
    public override Expression<Func<PlantSafety.Domain.Compliance.RegulatoryCalendar.Aggregates.RegulatoryCalendar, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.Compliance.RegulatoryCalendar.Aggregates.RegulatoryCalendar, bool>>(body, param);
    }
}

internal sealed class OrRegulatoryCalendarSpecification(RegulatoryCalendarSpecification left, RegulatoryCalendarSpecification right) : RegulatoryCalendarSpecification
{
    public override Expression<Func<PlantSafety.Domain.Compliance.RegulatoryCalendar.Aggregates.RegulatoryCalendar, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.Compliance.RegulatoryCalendar.Aggregates.RegulatoryCalendar, bool>>(body, param);
    }
}

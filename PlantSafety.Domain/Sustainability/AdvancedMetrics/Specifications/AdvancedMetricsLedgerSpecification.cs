// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability.AdvancedMetrics
//  Layer:          Domain
//  File:           AdvancedMetricsLedgerSpecification.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for AdvancedMetricsLedger aggregate predicate
//                  expressions. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Sustainability.AdvancedMetrics.Specifications;

/// <summary>Abstract base specification for <see cref="Aggregates.AdvancedMetricsLedger"/> aggregate predicate expressions.</summary>
public abstract class AdvancedMetricsLedgerSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<Aggregates.AdvancedMetricsLedger, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<Aggregates.AdvancedMetricsLedger, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public AdvancedMetricsLedgerSpecification And(AdvancedMetricsLedgerSpecification other) => new AndAdvancedMetricsLedgerSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public AdvancedMetricsLedgerSpecification Or(AdvancedMetricsLedgerSpecification other) => new OrAdvancedMetricsLedgerSpecification(this, other);
}

internal sealed class AndAdvancedMetricsLedgerSpecification(AdvancedMetricsLedgerSpecification left, AdvancedMetricsLedgerSpecification right) : AdvancedMetricsLedgerSpecification
{
    public override Expression<Func<Aggregates.AdvancedMetricsLedger, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<Aggregates.AdvancedMetricsLedger, bool>>(body, param);
    }
}

internal sealed class OrAdvancedMetricsLedgerSpecification(AdvancedMetricsLedgerSpecification left, AdvancedMetricsLedgerSpecification right) : AdvancedMetricsLedgerSpecification
{
    public override Expression<Func<Aggregates.AdvancedMetricsLedger, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<Aggregates.AdvancedMetricsLedger, bool>>(body, param);
    }
}

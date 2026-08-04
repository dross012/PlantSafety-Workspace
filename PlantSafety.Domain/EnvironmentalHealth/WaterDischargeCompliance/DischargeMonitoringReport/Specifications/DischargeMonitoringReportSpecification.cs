// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport
//  Layer:          Domain
//  File:           DischargeMonitoringReportSpecification.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for DischargeMonitoringReport aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport.Specifications;

/// <summary>Abstract base specification for <see cref="DischargeMonitoringReport"/> aggregate predicate expressions.</summary>
public abstract class DischargeMonitoringReportSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport.Aggregates.DischargeMonitoringReport, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport.Aggregates.DischargeMonitoringReport, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public DischargeMonitoringReportSpecification And(DischargeMonitoringReportSpecification other) => new AndDischargeMonitoringReportSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public DischargeMonitoringReportSpecification Or(DischargeMonitoringReportSpecification other) => new OrDischargeMonitoringReportSpecification(this, other);
}

internal sealed class AndDischargeMonitoringReportSpecification(DischargeMonitoringReportSpecification left, DischargeMonitoringReportSpecification right) : DischargeMonitoringReportSpecification
{
    public override Expression<Func<PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport.Aggregates.DischargeMonitoringReport, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport.Aggregates.DischargeMonitoringReport, bool>>(body, param);
    }
}

internal sealed class OrDischargeMonitoringReportSpecification(DischargeMonitoringReportSpecification left, DischargeMonitoringReportSpecification right) : DischargeMonitoringReportSpecification
{
    public override Expression<Func<PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport.Aggregates.DischargeMonitoringReport, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport.Aggregates.DischargeMonitoringReport, bool>>(body, param);
    }
}
// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Alerts
//  Layer:          Domain
//  File:           AlertSpecification.cs
//  Created:        2026-05-26
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:
//      Canonical base class for all Alert specifications. Provides a strongly
//      typed, LINQ-safe, analyzer-safe predicate expression used by
//      repositories and application queries.
//      Complies with:
//          - Canonical Specification Rule (2.5)
//          - Repository Architectural Rule (2.10)
//          - Deterministic Behavior Rule
//      Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.Alerts.Specifications;

public abstract class AlertSpecification
{
    public abstract Expression<Func<Alert, bool>> ToExpression();

    public Func<Alert, bool> ToPredicate()
        => ToExpression().Compile();

    public AlertSpecification And(AlertSpecification other)
        => new AndAlertSpecification(this, other);

    public AlertSpecification Or(AlertSpecification other)
        => new OrAlertSpecification(this, other);
}

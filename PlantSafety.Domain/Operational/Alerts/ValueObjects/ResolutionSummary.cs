// ============================================================================
//  Project:        PlantSafety.Platform
//  Area:           Operational
//  Module:         Alerts
//  Layer:          Domain (Value Objects)
//  File:           ResolutionSummary.cs
//  Created:        2026-05-26
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//
//  Description:
//      Strong-typed value object representing the human-authored summary of an
//      alert's resolution. This value object encapsulates the final narrative
//      explanation of how the alert was resolved, ensuring that resolution
//      summaries are never passed as raw strings.
//
//  Architectural Notes:
//      • Value objects MUST be immutable (Rule 3.1).
//      • Value objects MUST NOT allow null, empty, or whitespace values
//        (Rule 3.2).
//      • Value objects MUST NOT contain domain behavior (Rule 1.1).
//      • Value objects MUST be deterministic and serialization-safe.
//      • Resolution summaries MUST be treated as opaque text — no parsing,
//        no embedded semantics, no formatting assumptions.
//
//  Onboarding Notes:
//      • Used by ResolveAlertCommand, domain logic, and read models.
//      • ResolutionSummary is NOT responsible for validation beyond structural
//        safety — business rules are enforced in the aggregate.
//      • Validation occurs at construction time; invalid summaries cannot
//        enter the domain.
// ============================================================================

namespace PlantSafety.Domain.Operational.Alerts.ValueObjects;

public sealed record ResolutionSummary
{
    public string Value { get; }

    public ResolutionSummary(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Resolution summary cannot be null, empty, or whitespace.", nameof(value));

        Value = value.Trim();
    }

    public override string ToString() => Value;
}

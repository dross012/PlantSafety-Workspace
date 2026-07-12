// =====================================================================================================
//  File:           ApplyMetadataAdded.cs
//  AUTHOR: David (Ted) + Copilot
//  CREATED: 2026-05-27
//  SCHEMA VERSION: 2026.1
//
//  PURPOSE:
//      Partial class extension for the Alert aggregate containing the pure state mutation
//      method for applying AlertMetadataAddedDomainEvent. This file isolates the Apply()
//      logic for metadata mutations to maintain aggregate clarity and enforce the 2026
//      Domain Event Application Pattern.
//
//  RATIONALE & ONBOARDING NOTES:
//      - Apply methods MUST be pure state transitions (Rule 2.13).
//      - Apply methods MUST NOT contain validation or branching logic (Rule 1.1).
//      - Apply methods MUST be deterministic and replay-safe (Rule 1.4).
//      - Metadata is additive-only (Rule 7.1).
//      - This file exists separately to avoid bloating the main Alert.cs aggregate file.
//      - This method is invoked internally during event replay, snapshot rebuild, and
//        aggregate rehydration.
//
//  ENTERPRISE HANDBOOK COMPLIANCE:
//      ✔ Pure state mutation only
//      ✔ Deterministic, idempotent behavior
//      ✔ No business logic or validation
//      ✔ Immutable metadata entry creation
//      ✔ Replay-safe for event sourcing and rebuild
//
// =====================================================================================================

using PlantSafety.Domain.Operational.Alerts.Events;
using PlantSafety.Domain.Operational.Alerts.ValueObjects;

namespace PlantSafety.Domain.Operational.Alerts.Aggregates;

public sealed partial class Alert
{
    /// <summary>
    /// Applies the AlertMetadataAddedDomainEvent to mutate the aggregate state.
    /// </summary>
    internal void Apply(AlertMetadataAddedEvent e)
    {
        var entry = new AlertMetadataEntry(
            alertMetadataId: AlertMetadataId.New(),
            alertId: AlertId,
            key: e.Key,
            value: e.Value);

        _metadata.Add(entry);
    }
}

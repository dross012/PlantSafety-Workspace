// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Alerts
//  Layer:          Domain
//  File:           AlertEventRegistry.cs
//  Created:        2026-05-26
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:
//      Canonical event registry for the Alerts domain. Maps event type names
//      and schema versions to CLR types. Used by:
//          - Rehydration service
//          - Replay pipelines
//          - Rebuild pipelines
//          - Outbox consumers
//          - SafetyAnalyzer event validation
//
//      Complies with:
//          - Domain Event Registry Rule (2.13)
//          - Canonical Event Envelope Rule (14.4)
//          - Deterministic Behavior Rule (1.3)
//          - Additive-only evolution
// ============================================================================

using PlantSafety.Domain.Operational.Alerts.Constants;
using PlantSafety.Domain.Operational.Alerts.Interfaces;

namespace PlantSafety.Domain.Operational.Alerts.Registry;

public sealed class AlertEventRegistry : IAlertEventRegistry
{
    // NOTE: the tuple key's `Version` component corresponds to each event's
    // `EventVersion` (an int, currently `1` for every Alert*Event — see the
    // Events folder), NOT `SchemaVersion` (a string like "2026.1"). Both
    // EventVersion and SchemaVersion are instance members on the event
    // records (by design — they're read polymorphically off IAlertDomainEvent
    // instances elsewhere), so they cannot be referenced statically here;
    // the literal `1` is used to mirror each event's current EventVersion.
    //
    // The dictionary also no longer takes a StringComparer: StringComparer
    // implements IEqualityComparer<string>, not IEqualityComparer<(string,
    // int)>, so it cannot be used as the comparer for this tuple-keyed
    // dictionary. Default tuple equality (ordinal, case-sensitive) is used
    // instead; all registered EventType values below are literal constants
    // from AlertEventTypes with consistent casing.
    private static readonly Dictionary<(string EventType, int Version), Type> _map =
        new()
        {
            // ------------------------------------------------------------
            //  AlertCreatedEvent
            // ------------------------------------------------------------
            {
                (AlertEventTypes.Created, 1),
                typeof(AlertCreatedEvent)
            },

            // ------------------------------------------------------------
            //  AlertAcknowledgedEvent
            // ------------------------------------------------------------
            {
                (AlertEventTypes.Acknowledged, 1),
                typeof(AlertAcknowledgedEvent)
            },

            // ------------------------------------------------------------
            //  AlertInProgressEvent
            // ------------------------------------------------------------
            {
                (AlertEventTypes.InProgress, 1),
                typeof(AlertInProgressEvent)
            },

            // ------------------------------------------------------------
            //  AlertEscalatedEvent
            // ------------------------------------------------------------
            {
                (AlertEventTypes.Escalated, 1),
                typeof(AlertEscalatedEvent)
            },

            // ------------------------------------------------------------
            //  AlertResolvedEvent
            // ------------------------------------------------------------
            {
                (AlertEventTypes.Resolved, 1),
                typeof(AlertResolvedEvent)
            },

            // ------------------------------------------------------------
            //  AlertClosedEvent
            // ------------------------------------------------------------
            {
                (AlertEventTypes.Closed, 1),
                typeof(AlertClosedEvent)
            },

            // ------------------------------------------------------------
            //  AlertArchivedEvent
            // ------------------------------------------------------------
            {
                (AlertEventTypes.Archived, 1),
                typeof(AlertArchivedEvent)
            },

            // ------------------------------------------------------------
            //  AlertAssignedEvent
            // ------------------------------------------------------------
            {
                (AlertEventTypes.Assigned, 1),
                typeof(AlertAssignedEvent)
            },

            {
                ("AlertMetadataAdded", 1),
                typeof(AlertMetadataAddedEvent)
            },
            {
                ("AlertMetadataUpdated", 1),
                typeof(AlertMetadataUpdatedEvent)
            },
            {
                ("AlertMetadataRemoved", 1),
                typeof(AlertMetadataRemovedEvent)
            }

        };

    public Type? Resolve(string eventType, int version)
    {
        return _map.TryGetValue((eventType, version), out var type)
            ? type
            : null;
    }
}

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
    private static readonly Dictionary<(string EventType, int Version), Type> _map =
        new(StringComparer.OrdinalIgnoreCase)
        {
            // ------------------------------------------------------------
            //  AlertCreatedEvent
            // ------------------------------------------------------------
            {
                (AlertEventTypes.Created, AlertCreatedEvent.SchemaVersion),
                typeof(AlertCreatedEvent)
            },

            // ------------------------------------------------------------
            //  AlertAcknowledgedEvent
            // ------------------------------------------------------------
            {
                (AlertEventTypes.Acknowledged, AlertAcknowledgedEvent.SchemaVersion),
                typeof(AlertAcknowledgedEvent)
            },

            // ------------------------------------------------------------
            //  AlertInProgressEvent
            // ------------------------------------------------------------
            {
                (AlertEventTypes.InProgress, AlertInProgressEvent.SchemaVersion),
                typeof(AlertInProgressEvent)
            },

            // ------------------------------------------------------------
            //  AlertEscalatedEvent
            // ------------------------------------------------------------
            {
                (AlertEventTypes.Escalated, AlertEscalatedEvent.SchemaVersion),
                typeof(AlertEscalatedEvent)
            },

            // ------------------------------------------------------------
            //  AlertResolvedEvent
            // ------------------------------------------------------------
            {
                (AlertEventTypes.Resolved, AlertResolvedEvent.SchemaVersion),
                typeof(AlertResolvedEvent)
            },

            // ------------------------------------------------------------
            //  AlertClosedEvent
            // ------------------------------------------------------------
            {
                (AlertEventTypes.Closed, AlertClosedEvent.SchemaVersion),
                typeof(AlertClosedEvent)
            },

            // ------------------------------------------------------------
            //  AlertArchivedEvent
            // ------------------------------------------------------------
            {
                (AlertEventTypes.Archived, AlertArchivedEvent.SchemaVersion),
                typeof(AlertArchivedEvent)
            },

            // ------------------------------------------------------------
            //  AlertAssignedEvent
            // ------------------------------------------------------------
            {
                (AlertEventTypes.Assigned, AlertAssignedEvent.SchemaVersion),
                typeof(AlertAssignedEvent)
            },

            {
                ("AlertMetadataAdded", AlertMetadataAddedEvent.SchemaVersion),
                typeof(AlertMetadataAddedEvent)
            },
            {
                ("AlertMetadataUpdated", AlertMetadataUpdatedEvent.SchemaVersion),
                typeof(AlertMetadataUpdatedEvent)
            },
            {
                ("AlertMetadataRemoved", AlertMetadataRemovedEvent.SchemaVersion),
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

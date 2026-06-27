// ============================================================================
//  File:           AlertEventTypes.cs
//  Created:        2026-05-26
//  Description:
//      Canonical event type names for Alerts domain events.
// ============================================================================

namespace PlantSafety.Domain.Operational.Alerts.Constants;

public static class AlertEventTypes
{
    public const string Created = "AlertCreated";
    public const string Acknowledged = "AlertAcknowledged";
    public const string InProgress = "AlertInProgress";
    public const string Escalated = "AlertEscalated";
    public const string Resolved = "AlertResolved";
    public const string Closed = "AlertClosed";
    public const string Archived = "AlertArchived";
    public const string Assigned = "AlertAssigned";
    public const string MetadataAdded = "AlertMetadataAdded";
    public const string MetadataUpdated = "AlertMetadataUpdated";
    public const string MetadataRemoved = "AlertMetadataRemoved";

}

// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: CommunitySafety module registration marker and constants.
//   Covers: CommunityContact, CommunityConcern, CommunityImpactAssessment, CommunityNotification, CommunityMeetingRecord.
//   Purpose: Manages the plant's safety relationship with surrounding communities -- community
//   risk notifications, neighbor complaint intake, community impact tracking for on-site
//   incidents. Integrates with SafetyEvents (incident source for impact assessments) and
//   Compliance (community right-to-know/EPCRA reporting); reserves an event-based integration
//   point for a future PublicSafetyCommunication module.
//   All commands, queries, and handlers for this module are namespaced under Application.CommunitySafety.<SubComponent>.
// SchemaVersion: 1
// Additive-only evolution: new constants may be added; no existing constants may be renamed or removed.

namespace PlantSafety.Application.CommunitySafety;

/// <summary>
/// Module marker and constants for the CommunitySafety bounded context. Manages the plant's
/// safety relationship with surrounding communities: community risk notifications, neighbor
/// complaint intake, community impact tracking for on-site incidents, and outreach/notification
/// history.
/// </summary>
public static class CommunitySafetyModule
{
    /// <summary>The canonical module name used in event types, error codes, and log envelopes.</summary>
    public const string ModuleName = "CommunitySafety";

    /// <summary>The SQL schema name owned by this module.</summary>
    public const string SqlSchema = "CommunitySafety";

    /// <summary>Canonical integration event type prefix for this module.</summary>
    public const string EventTypePrefix = "CommunitySafety";

    /// <summary>Sub-components owned by this module.</summary>
    public static class SubComponents
    {
        /// <summary>The CommunityContact sub-component (Community Contact Registry).</summary>
        public const string CommunityContact = "CommunityContact";

        /// <summary>The CommunityConcern sub-component (Complaint/Concern Intake).</summary>
        public const string CommunityConcern = "CommunityConcern";

        /// <summary>The CommunityImpactAssessment sub-component (Community Impact Assessment).</summary>
        public const string CommunityImpactAssessment = "CommunityImpactAssessment";

        /// <summary>The CommunityNotification sub-component (Outreach &amp; Notification History).</summary>
        public const string CommunityNotification = "CommunityNotification";

        /// <summary>The CommunityMeetingRecord sub-component (Outreach &amp; Notification History).</summary>
        public const string CommunityMeetingRecord = "CommunityMeetingRecord";
    }

    /// <summary>Permissions for this module. CommunityContact permissions are deliberately narrow per the PII-minimization data-governance mandate.</summary>
    public static class Permissions
    {
        /// <summary>Permission to view CommunityContact records (PII-restricted).</summary>
        public const string ViewCommunityContact = "CommunitySafety.CommunityContact.View";

        /// <summary>Permission to register/manage CommunityContact records (PII-restricted).</summary>
        public const string ManageCommunityContact = "CommunitySafety.CommunityContact.Manage";

        /// <summary>Permission to view CommunityConcern records.</summary>
        public const string ViewCommunityConcern = "CommunitySafety.CommunityConcern.View";

        /// <summary>Permission to log/manage CommunityConcern records.</summary>
        public const string ManageCommunityConcern = "CommunitySafety.CommunityConcern.Manage";

        /// <summary>Permission to view CommunityImpactAssessment records.</summary>
        public const string ViewCommunityImpactAssessment = "CommunitySafety.CommunityImpactAssessment.View";

        /// <summary>Permission to record CommunityImpactAssessment records.</summary>
        public const string RecordCommunityImpactAssessment = "CommunitySafety.CommunityImpactAssessment.Record";

        /// <summary>Permission to view CommunityNotification history.</summary>
        public const string ViewCommunityNotification = "CommunitySafety.CommunityNotification.View";

        /// <summary>Permission to send CommunityNotification records.</summary>
        public const string SendCommunityNotification = "CommunitySafety.CommunityNotification.Send";

        /// <summary>Permission to view CommunityMeetingRecord history.</summary>
        public const string ViewCommunityMeetingRecord = "CommunitySafety.CommunityMeetingRecord.View";

        /// <summary>Permission to record CommunityMeetingRecord entries.</summary>
        public const string RecordCommunityMeetingRecord = "CommunitySafety.CommunityMeetingRecord.Record";
    }

    /// <summary>
    /// Reserved event-type names consumed by other modules integrating with CommunitySafety.
    /// SafetyEvents supplies IncidentId references for CommunityImpactAssessedEvent; Compliance
    /// subscribes to CommunityImpactAssessedEvent (Severe) for EPCRA/right-to-know reporting;
    /// RiskManagement subscribes to CommunityConcernEscalatedEvent; a future
    /// PublicSafetyCommunication module (not guaranteed to exist in this repository state) is
    /// expected to subscribe to CommunityNotificationSentEvent.
    /// </summary>
    public static class IntegrationEventTypes
    {
        /// <summary>Event type consumed by Compliance/RiskManagement for community impact tracking.</summary>
        public const string CommunityImpactAssessed = "CommunityImpactAssessed";

        /// <summary>Event type consumed by RiskManagement to elevate local risk visibility on recurring concerns.</summary>
        public const string CommunityConcernEscalated = "CommunityConcernEscalated";

        /// <summary>Event type reserved for a future PublicSafetyCommunication module integration.</summary>
        public const string CommunityNotificationSent = "CommunityNotificationSent";
    }
}

// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Module marker, constants, and onboarding documentation for the Alerts
//   bounded context.
// SchemaVersion: 1
// Additive-only evolution: new constants may be added; no existing constants may be renamed or removed.

namespace PlantSafety.Application.Alerts;

/// <summary>
/// Module marker, constants, and onboarding documentation for the Alerts bounded context.
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> Owns the full lifecycle of real-time manufacturing safety alerts —
/// life-safety emergencies, injury/man-down events, operational &amp; hazard conditions,
/// environmental exceedances, facility/traffic hazards, PPE/behavioral violations,
/// permit-to-work (PTW) breaches, maintenance/equipment health warnings, security/access
/// incidents, quality/process deviations, and compliance/audit lapses — grounded in
/// <c>docs/reference/PlantSafety_Program_Structure_extract.txt</c>, "Comprehensive Manufacturing
/// Safety Alerts" and "11. Alerts &amp; Emergency Response Dashboard".</para>
///
/// <para><b>Responsibilities:</b> Trigger, acknowledge, dispatch, escalate, resolve, and close
/// alerts; classify alerts by catalog category and severity; record multi-channel delivery
/// attempts (PA speaker, strobe, digital signage, SMS, mobile push, wearable vibration, email,
/// SCADA/HMI pop-up, Rave Mobile Safety, PlantSafety dashboard); record response actions and
/// close-out verification for compliance logging &amp; reporting.</para>
///
/// <para><b>Dependencies:</b> None on other bounded-context internals (Maximalist Module Design,
/// Part IV). May be referenced by other modules only through published integration events, never
/// through direct calls.</para>
///
/// <para><b>Inputs:</b> Sensor/IoT telemetry, computer-vision detections, wearable device
/// signals, panic-button activations, human-reported observations, and SCADA/PLC system
/// triggers.</para>
///
/// <para><b>Outputs:</b> Alert read models for the Alerts &amp; Emergency Response Dashboard
/// (Alert Type Distribution, Critical Alerts Timeline, Alert Location Heatmap, Response Time
/// Trend, Multi-Channel Delivery Status, Man-Down Alerts Trend, Fire/Smoke Detection Graph, Gas
/// Leak Sensor Trend); delivery dispatch requests for downstream notification infrastructure.</para>
///
/// <para><b>Events emitted:</b> <c>Alerts.AlertTriggered</c>, <c>Alerts.AlertAcknowledged</c>,
/// <c>Alerts.AlertDispatched</c>, <c>Alerts.AlertEscalated</c>, <c>Alerts.AlertResolved</c>,
/// <c>Alerts.AlertClosed</c>, <c>Alerts.AlertDeliveryRecorded</c>.</para>
///
/// <para><b>Events consumed:</b> None directly (Alerts is upstream/source-of-truth for alert
/// state). Sensor/observability telemetry arrives via inbound adapters in the Infrastructure
/// layer, not via cross-module integration events, to avoid coupling to another module's
/// internal event shapes.</para>
///
/// <para><b>Integration points:</b> Dashboards module (Alerts &amp; Emergency Response Dashboard,
/// Full Program Master Dashboard); downstream notification/dispatch infrastructure (PA, SMS,
/// push, SCADA/HMI, Rave Mobile Safety) via the Infrastructure layer (not generated in this
/// Domain+Application-only pass).</para>
/// </remarks>
public static class AlertsModule
{
    /// <summary>The canonical module name used in event types, error codes, and log envelopes.</summary>
    public const string ModuleName = "Alerts";

    /// <summary>The SQL schema name owned by this module (reserved for future Infrastructure work).</summary>
    public const string SqlSchema = "Alerts";

    /// <summary>Canonical integration event type prefix for this module.</summary>
    public const string EventTypePrefix = "Alerts";

    /// <summary>Permissions for this module.</summary>
    public static class Permissions
    {
        /// <summary>Permission to trigger a new alert.</summary>
        public const string TriggerAlert = "Alerts.Alert.Trigger";

        /// <summary>Permission to acknowledge an alert.</summary>
        public const string AcknowledgeAlert = "Alerts.Alert.Acknowledge";

        /// <summary>Permission to escalate an alert.</summary>
        public const string EscalateAlert = "Alerts.Alert.Escalate";

        /// <summary>Permission to resolve an alert.</summary>
        public const string ResolveAlert = "Alerts.Alert.Resolve";

        /// <summary>Permission to close an alert with close-out verification.</summary>
        public const string CloseAlert = "Alerts.Alert.Close";

        /// <summary>Permission to view alert history and logging/reporting data.</summary>
        public const string ViewAlertHistory = "Alerts.Alert.ViewHistory";
    }
}

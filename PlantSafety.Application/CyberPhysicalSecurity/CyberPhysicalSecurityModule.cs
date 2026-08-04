// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: CyberPhysicalSecurity module registration marker and constants. Covers the OT
//   Asset Inventory, Anomalous Control-Change Detection, Sensor Integrity Monitoring, Sabotage
//   Indicator Correlation, and Incident Response subdomains. All commands, queries, and handlers
//   for this module live under PlantSafety.Application.CyberPhysicalSecurity. Conceptually
//   integrates with AiGovernance (AI-model integrity oversight) and leaves an event-based
//   integration point for a future PhysicalAccessControl module.
// SchemaVersion: 1

namespace PlantSafety.Application.CyberPhysicalSecurity;

/// <summary>
/// Module marker and constants for the CyberPhysicalSecurity bounded context.
/// </summary>
public static class CyberPhysicalSecurityModule
{
    /// <summary>The canonical module name used in event types, error codes, and log envelopes.</summary>
    public const string ModuleName = "CyberPhysicalSecurity";

    /// <summary>The SQL schema name owned by this module.</summary>
    public const string SqlSchema = "CyberPhysicalSecurity";

    /// <summary>Canonical integration event type prefix for this module.</summary>
    public const string EventTypePrefix = "CyberPhysicalSecurity";

    /// <summary>Permissions for this module.</summary>
    public static class Permissions
    {
        /// <summary>Permission to register/manage OT asset inventory records.</summary>
        public const string ManageOtAssets = "CyberPhysicalSecurity.OtAssets.Manage";

        /// <summary>Permission to view OT asset inventory records.</summary>
        public const string ViewOtAssets = "CyberPhysicalSecurity.OtAssets.View";

        /// <summary>Permission to record/flag control-change events.</summary>
        public const string ManageControlChangeEvents = "CyberPhysicalSecurity.ControlChangeEvents.Manage";

        /// <summary>Permission to view control-change events.</summary>
        public const string ViewControlChangeEvents = "CyberPhysicalSecurity.ControlChangeEvents.View";

        /// <summary>Permission to run/record sensor-integrity checks.</summary>
        public const string ManageSensorIntegrityChecks = "CyberPhysicalSecurity.SensorIntegrityChecks.Manage";

        /// <summary>Permission to view sensor-integrity check history.</summary>
        public const string ViewSensorIntegrityChecks = "CyberPhysicalSecurity.SensorIntegrityChecks.View";

        /// <summary>Permission to raise/triage sabotage indicators.</summary>
        public const string ManageSabotageIndicators = "CyberPhysicalSecurity.SabotageIndicators.Manage";

        /// <summary>Permission to view sabotage indicators.</summary>
        public const string ViewSabotageIndicators = "CyberPhysicalSecurity.SabotageIndicators.View";

        /// <summary>Permission to open/close cyber-physical incidents.</summary>
        public const string ManageIncidents = "CyberPhysicalSecurity.Incidents.Manage";

        /// <summary>Permission to view cyber-physical incidents.</summary>
        public const string ViewIncidents = "CyberPhysicalSecurity.Incidents.View";
    }
}

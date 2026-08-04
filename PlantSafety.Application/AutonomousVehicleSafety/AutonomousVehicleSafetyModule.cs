// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: AutonomousVehicleSafety module registration marker and constants.
//   Covers: AutonomousVehicleAsset, OperatingGeofence, SafetyCaseValidation, HumanRobotInteractionEvent,
//   AutonomousVehicleIncident.
//   Source: Safety oversight for autonomous/semi-autonomous vehicles and mobile robots operating
//   on-site (AGVs, AMRs, autonomous yard trucks) -- geofencing, safety-case validation, incident
//   tracking. All commands, queries, and handlers for this module are namespaced under
//   Application.AutonomousVehicleSafety.<SubComponent>.
// SchemaVersion: 1
// Additive-only evolution: new constants may be added; no existing constants may be renamed or removed.

namespace PlantSafety.Application.AutonomousVehicleSafety;

/// <summary>
/// Module marker and constants for the AutonomousVehicleSafety bounded context.
/// Safety oversight for autonomous/semi-autonomous vehicles and mobile robots operating on-site.
/// </summary>
public static class AutonomousVehicleSafetyModule
{
    /// <summary>The canonical module name used in event types, error codes, and log envelopes.</summary>
    public const string ModuleName = "AutonomousVehicleSafety";

    /// <summary>The SQL schema name owned by this module.</summary>
    public const string SqlSchema = "AutonomousVehicleSafety";

    /// <summary>Canonical integration event type prefix for this module.</summary>
    public const string EventTypePrefix = "AutonomousVehicleSafety";

    /// <summary>Sub-components owned by this module.</summary>
    public static class SubComponents
    {
        /// <summary>The AutonomousVehicleAsset sub-component (Autonomous Asset Registry).</summary>
        public const string AutonomousVehicleAsset = "AutonomousVehicleAsset";

        /// <summary>The OperatingGeofence sub-component (Geofence &amp; Operating Domain Management).</summary>
        public const string OperatingGeofence = "OperatingGeofence";

        /// <summary>The SafetyCaseValidation sub-component (Safety Case Validation).</summary>
        public const string SafetyCaseValidation = "SafetyCaseValidation";

        /// <summary>The HumanRobotInteractionEvent sub-component (Human-Robot Interaction Monitoring).</summary>
        public const string HumanRobotInteractionEvent = "HumanRobotInteractionEvent";

        /// <summary>The AutonomousVehicleIncident sub-component (Incident Management).</summary>
        public const string AutonomousVehicleIncident = "AutonomousVehicleIncident";
    }

    /// <summary>Permissions for this module.</summary>
    public static class Permissions
    {
        /// <summary>Permission to view AutonomousVehicleAsset records.</summary>
        public const string ViewAutonomousVehicleAsset = "AutonomousVehicleSafety.AutonomousVehicleAsset.View";

        /// <summary>Permission to create/register AutonomousVehicleAsset records.</summary>
        public const string CreateAutonomousVehicleAsset = "AutonomousVehicleSafety.AutonomousVehicleAsset.Create";

        /// <summary>Permission to suspend autonomous operation.</summary>
        public const string SuspendAutonomousVehicleAsset = "AutonomousVehicleSafety.AutonomousVehicleAsset.Suspend";

        /// <summary>Permission to view OperatingGeofence records.</summary>
        public const string ViewOperatingGeofence = "AutonomousVehicleSafety.OperatingGeofence.View";

        /// <summary>Permission to define/manage OperatingGeofence records.</summary>
        public const string ManageOperatingGeofence = "AutonomousVehicleSafety.OperatingGeofence.Manage";

        /// <summary>Permission to view SafetyCaseValidation records.</summary>
        public const string ViewSafetyCaseValidation = "AutonomousVehicleSafety.SafetyCaseValidation.View";

        /// <summary>Permission to validate safety cases.</summary>
        public const string ValidateSafetyCase = "AutonomousVehicleSafety.SafetyCaseValidation.Validate";

        /// <summary>Permission to view HumanRobotInteractionEvent records.</summary>
        public const string ViewHumanRobotInteractionEvent = "AutonomousVehicleSafety.HumanRobotInteractionEvent.View";

        /// <summary>Permission to record HumanRobotInteractionEvent records.</summary>
        public const string CreateHumanRobotInteractionEvent = "AutonomousVehicleSafety.HumanRobotInteractionEvent.Create";

        /// <summary>Permission to view AutonomousVehicleIncident records.</summary>
        public const string ViewAutonomousVehicleIncident = "AutonomousVehicleSafety.AutonomousVehicleIncident.View";

        /// <summary>Permission to record AutonomousVehicleIncident records.</summary>
        public const string CreateAutonomousVehicleIncident = "AutonomousVehicleSafety.AutonomousVehicleIncident.Create";
    }
}

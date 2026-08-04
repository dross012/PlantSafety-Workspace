// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: LogisticsTransportSafety module registration marker and constants.
//   Covers: TransportVehicles, Carriers, DriverQualifications, TransportTrips, LoadSecurementChecks, TransportIncidents.
//   Purpose: Manage safety for material/personnel transport -- on-site vehicle movement, carrier/driver
//   qualification, load safety, and transport incident tracking.
//   All commands, queries, and handlers for this module are namespaced under Application.LogisticsTransportSafety.<SubComponent>.
// SchemaVersion: 1
// Additive-only evolution: new constants may be added; no existing constants may be renamed or removed.

namespace PlantSafety.Application.LogisticsTransportSafety;

/// <summary>
/// Module marker and constants for the LogisticsTransportSafety bounded context.
/// Manages safety for material/personnel transport: on-site vehicle movement, carrier/driver
/// qualification, trip/route safety, load securement, and transport incident management.
/// </summary>
public static class LogisticsTransportSafetyModule
{
    /// <summary>The canonical module name used in event types, error codes, and log envelopes.</summary>
    public const string ModuleName = "LogisticsTransportSafety";

    /// <summary>The SQL schema name owned by this module.</summary>
    public const string SqlSchema = "LogisticsTransportSafety";

    /// <summary>Canonical integration event type prefix for this module.</summary>
    public const string EventTypePrefix = "LogisticsTransportSafety";

    /// <summary>Sub-components owned by this module.</summary>
    public static class SubComponents
    {
        /// <summary>The TransportVehicles sub-component.</summary>
        public const string TransportVehicles = "TransportVehicles";

        /// <summary>The Carriers sub-component.</summary>
        public const string Carriers = "Carriers";

        /// <summary>The DriverQualifications sub-component.</summary>
        public const string DriverQualifications = "DriverQualifications";

        /// <summary>The TransportTrips sub-component.</summary>
        public const string TransportTrips = "TransportTrips";

        /// <summary>The LoadSecurementChecks sub-component.</summary>
        public const string LoadSecurementChecks = "LoadSecurementChecks";

        /// <summary>The TransportIncidents sub-component.</summary>
        public const string TransportIncidents = "TransportIncidents";
    }

    /// <summary>Permissions for this module.</summary>
    public static class Permissions
    {
        /// <summary>Permission to view TransportVehicles records.</summary>
        public const string ViewTransportVehicles = "LogisticsTransportSafety.TransportVehicles.View";

        /// <summary>Permission to create TransportVehicles records.</summary>
        public const string CreateTransportVehicles = "LogisticsTransportSafety.TransportVehicles.Create";

        /// <summary>Permission to view Carriers records.</summary>
        public const string ViewCarriers = "LogisticsTransportSafety.Carriers.View";

        /// <summary>Permission to create Carriers records.</summary>
        public const string CreateCarriers = "LogisticsTransportSafety.Carriers.Create";

        /// <summary>Permission to view DriverQualifications records.</summary>
        public const string ViewDriverQualifications = "LogisticsTransportSafety.DriverQualifications.View";

        /// <summary>Permission to create DriverQualifications records.</summary>
        public const string CreateDriverQualifications = "LogisticsTransportSafety.DriverQualifications.Create";

        /// <summary>Permission to view TransportTrips records.</summary>
        public const string ViewTransportTrips = "LogisticsTransportSafety.TransportTrips.View";

        /// <summary>Permission to create TransportTrips records.</summary>
        public const string CreateTransportTrips = "LogisticsTransportSafety.TransportTrips.Create";

        /// <summary>Permission to view LoadSecurementChecks records.</summary>
        public const string ViewLoadSecurementChecks = "LogisticsTransportSafety.LoadSecurementChecks.View";

        /// <summary>Permission to create LoadSecurementChecks records.</summary>
        public const string CreateLoadSecurementChecks = "LogisticsTransportSafety.LoadSecurementChecks.Create";

        /// <summary>Permission to view TransportIncidents records.</summary>
        public const string ViewTransportIncidents = "LogisticsTransportSafety.TransportIncidents.View";

        /// <summary>Permission to create TransportIncidents records.</summary>
        public const string CreateTransportIncidents = "LogisticsTransportSafety.TransportIncidents.Create";
    }
}

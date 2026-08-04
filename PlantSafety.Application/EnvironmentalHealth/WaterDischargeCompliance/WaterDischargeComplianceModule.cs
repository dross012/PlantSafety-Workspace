// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: WaterDischargeCompliance sub-domain module registration marker and constants.
//   Covers: DischargePermit, DischargePoint, EffluentSample, DischargeLimit, LimitExceedance,
//   DischargeMonitoringReport. Sub-domain of the EnvironmentalHealth module -- tracks permitted
//   discharge points, effluent sampling, regulatory limits, and manages exceedance response and
//   Clean Water Act / NPDES-style Discharge Monitoring Report (DMR) regulatory reporting.
//   All commands, queries, and handlers for this sub-domain are namespaced under
//   Application.EnvironmentalHealth.WaterDischargeCompliance.<Entity>.
// SchemaVersion: 1
// Additive-only evolution: new constants may be added; no existing constants may be renamed or removed.

namespace PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance;

/// <summary>Module marker and constants for the EnvironmentalHealth.WaterDischargeCompliance sub-domain.</summary>
public static class WaterDischargeComplianceModule
{
    /// <summary>The canonical module name used in event types, error codes, and log envelopes.</summary>
    public const string ModuleName = "EnvironmentalHealth.WaterDischargeCompliance";

    /// <summary>The SQL schema name owned by this module.</summary>
    public const string SqlSchema = "EnvironmentalHealth";

    /// <summary>Canonical integration event type prefix for this module.</summary>
    public const string EventTypePrefix = "WaterDischargeCompliance";

    /// <summary>Entities owned by this sub-domain.</summary>
    public static class Entities
    {
        /// <summary>The DischargePermit entity.</summary>
        public const string DischargePermit = "DischargePermit";
        /// <summary>The DischargePoint entity.</summary>
        public const string DischargePoint = "DischargePoint";
        /// <summary>The EffluentSample entity.</summary>
        public const string EffluentSample = "EffluentSample";
        /// <summary>The DischargeLimit entity.</summary>
        public const string DischargeLimit = "DischargeLimit";
        /// <summary>The LimitExceedance entity.</summary>
        public const string LimitExceedance = "LimitExceedance";
        /// <summary>The DischargeMonitoringReport entity.</summary>
        public const string DischargeMonitoringReport = "DischargeMonitoringReport";
    }

    /// <summary>KPI identifiers surfaced by this sub-domain's dashboards.</summary>
    public static class Kpis
    {
        /// <summary>Percent of effluent samples within their applicable discharge limit.</summary>
        public const string PercentSamplesWithinLimit = "WaterDischargeCompliance.PercentSamplesWithinLimit";
        /// <summary>Permit renewal lead time (days between renewal and prior expiration).</summary>
        public const string PermitRenewalLeadTimeDays = "WaterDischargeCompliance.PermitRenewalLeadTimeDays";
        /// <summary>Mean time to close an exceedance response, in hours.</summary>
        public const string MeanTimeToCloseExceedanceHours = "WaterDischargeCompliance.MeanTimeToCloseExceedanceHours";
        /// <summary>Percentage of Discharge Monitoring Reports submitted on or before their due date.</summary>
        public const string ReportingOnTimeRate = "WaterDischargeCompliance.ReportingOnTimeRate";
    }
}
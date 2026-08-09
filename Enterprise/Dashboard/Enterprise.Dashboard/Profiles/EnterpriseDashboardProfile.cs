// ============================================================================
//  Project:        Enterprise.Dashboard
//  Module:         Profiles
//  File:           EnterpriseDashboardProfile.cs
//  Created:        2026-08-08
//  Description:    Phase D.4: deterministically builds an EnterpriseDashboardDto
//                  from Enterprise.DataLake's IDataLakeQueryEngine (Phase D.2)
//                  plus caller-supplied hazard/severity classification and
//                  predictive-alert text (see EnterpriseHazardDistributionDto/
//                  EnterpriseSeverityDistributionDto/EnterprisePredictiveAlertsDto
//                  for why those three inputs can't be derived here directly).
//                  Named "Profile" to mirror the mapping-profile terminology
//                  used elsewhere in this program (Mapster profiles) while
//                  staying explicit, deterministic, hand-written code -- no
//                  reflection, no AutoMapper.
// ============================================================================
using Enterprise.Dashboard.Dtos;
using Enterprise.DataLake.Abstractions.Enums;
using Enterprise.DataLake.Abstractions.Records;
using Enterprise.DataLake.Infrastructure.Query;

namespace Enterprise.Dashboard.Profiles;

/// <summary>Deterministically builds an <see cref="EnterpriseDashboardDto"/> from the enterprise data lake.</summary>
public static class EnterpriseDashboardProfile
{
    /// <summary>Builds the enterprise dashboard payload.</summary>
    /// <param name="dataLakeQueryEngine">The query engine to aggregate record counts from.</param>
    /// <param name="hazardCountsByLabel">A caller-supplied hazard-type breakdown; empty if omitted.</param>
    /// <param name="severityCountsByLabel">A caller-supplied severity-level breakdown; empty if omitted.</param>
    /// <param name="predictiveAlerts">Caller-supplied human-readable predictive alert text; empty if omitted.</param>
    /// <param name="nowUtc">The generation timestamp; <see cref="DateTime.UtcNow"/> if omitted.</param>
    public static EnterpriseDashboardDto Build(
        IDataLakeQueryEngine dataLakeQueryEngine,
        IReadOnlyDictionary<string, int>? hazardCountsByLabel = null,
        IReadOnlyDictionary<string, int>? severityCountsByLabel = null,
        IReadOnlyList<string>? predictiveAlerts = null,
        DateTime? nowUtc = null)
    {
        DateTime generatedAtUtc = nowUtc ?? DateTime.UtcNow;
        IReadOnlyDictionary<DataLakeRecordType, int> counts = dataLakeQueryEngine.CountByType();

        var riskSummary = new EnterpriseRiskSummaryDto(
            TotalFacilities: counts[DataLakeRecordType.Facility],
            TotalHazardRecords: counts[DataLakeRecordType.Hazard],
            GeneratedAtUtc: generatedAtUtc);

        var complianceSummary = new EnterpriseComplianceSummaryDto(
            TotalFacilities: counts[DataLakeRecordType.Facility],
            TotalEventRecords: counts[DataLakeRecordType.Event],
            GeneratedAtUtc: generatedAtUtc);

        var hazardDistribution = new EnterpriseHazardDistributionDto(
            CountsByHazardLabel: hazardCountsByLabel ?? new Dictionary<string, int>(),
            TotalHazardRecords: counts[DataLakeRecordType.Hazard]);

        var severityDistribution = new EnterpriseSeverityDistributionDto(
            CountsBySeverityLabel: severityCountsByLabel ?? new Dictionary<string, int>(),
            TotalClassifiedRecords: (severityCountsByLabel ?? new Dictionary<string, int>()).Values.Sum());

        var predictiveAlertsDto = new EnterprisePredictiveAlertsDto(
            Alerts: predictiveAlerts ?? [],
            TotalPredictionRecords: counts[DataLakeRecordType.Prediction]);

        IReadOnlyList<DataLakeRecord> workflowRecords = dataLakeQueryEngine.QueryByType(DataLakeRecordType.Workflow);
        var workflowStatus = new EnterpriseWorkflowStatusDto(
            TotalWorkflowRecords: workflowRecords.Count,
            MostRecentWorkflowRecordAtUtc: workflowRecords.Count == 0 ? null : workflowRecords.Max(r => r.RecordedAtUtc));

        return new EnterpriseDashboardDto(
            RiskSummary: riskSummary,
            ComplianceSummary: complianceSummary,
            HazardDistribution: hazardDistribution,
            SeverityDistribution: severityDistribution,
            PredictiveAlerts: predictiveAlertsDto,
            WorkflowStatus: workflowStatus,
            TotalEmployees: counts[DataLakeRecordType.Employee],
            TotalMachines: counts[DataLakeRecordType.Machine],
            GeneratedAtUtc: generatedAtUtc);
    }
}

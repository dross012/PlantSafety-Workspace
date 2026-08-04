// ============================================================
// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Client-side DTOs for the EnvironmentalHealth.WaterDischargeCompliance
//   sub-module, mirroring the PlantSafety.Api read-model shapes for DischargePermit,
//   DischargePoint, EffluentSample, DischargeLimit, LimitExceedance, and
//   DischargeMonitoringReport. Display only.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

namespace PlantSafety.UI.Modules.EnvironmentalHealth.WaterDischargeCompliance.Models;

/// <summary>A single discharge permit record.</summary>
public sealed class DischargePermitDto
{
    /// <summary>Gets the permit record identifier.</summary>
    public Guid DischargePermitId { get; init; }

    /// <summary>Gets the site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the regulator-assigned permit number.</summary>
    public string PermitNumber { get; init; } = string.Empty;

    /// <summary>Gets the regulatory program classification.</summary>
    public string RegulatoryProgram { get; init; } = string.Empty;

    /// <summary>Gets the issuing authority.</summary>
    public string IssuingAuthority { get; init; } = string.Empty;

    /// <summary>Gets the UTC effective date.</summary>
    public DateTime EffectiveAtUtc { get; init; }

    /// <summary>Gets the UTC expiration date.</summary>
    public DateTime ExpiresAtUtc { get; init; }

    /// <summary>Gets the number of renewals recorded.</summary>
    public int RenewalCount { get; init; }

    /// <summary>Gets the lifecycle status.</summary>
    public string Status { get; init; } = string.Empty;
}

/// <summary>A single discharge point record.</summary>
public sealed class DischargePointDto
{
    /// <summary>Gets the discharge point record identifier.</summary>
    public Guid DischargePointId { get; init; }

    /// <summary>Gets the site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the identity of the owning discharge permit.</summary>
    public Guid DischargePermitId { get; init; }

    /// <summary>Gets the discharge point code.</summary>
    public string PointCode { get; init; } = string.Empty;

    /// <summary>Gets the discharge point display name.</summary>
    public string PointName { get; init; } = string.Empty;

    /// <summary>Gets the receiving water body.</summary>
    public string ReceivingWaterBody { get; init; } = string.Empty;

    /// <summary>Gets the outfall classification.</summary>
    public string Outfall { get; init; } = string.Empty;

    /// <summary>Gets the lifecycle status.</summary>
    public string Status { get; init; } = string.Empty;
}

/// <summary>Response payload for the discharge-points-by-site read-model query.</summary>
public sealed class ListDischargePointsBySiteResponseDto
{
    /// <summary>Gets the matching discharge point records. Never null; empty when no results.</summary>
    public IReadOnlyList<DischargePointDto> Items { get; init; } = [];
}

/// <summary>A single discharge limit exceedance record.</summary>
public sealed class LimitExceedanceDto
{
    /// <summary>Gets the exceedance record identifier.</summary>
    public Guid LimitExceedanceId { get; init; }

    /// <summary>Gets the identity of the discharge point where the exceedance occurred.</summary>
    public Guid DischargePointId { get; init; }

    /// <summary>Gets the exceeded parameter name.</summary>
    public string ParameterName { get; init; } = string.Empty;

    /// <summary>Gets the measured value.</summary>
    public decimal MeasuredValue { get; init; }

    /// <summary>Gets the limit value that was exceeded.</summary>
    public decimal LimitValue { get; init; }

    /// <summary>Gets the exceedance percentage.</summary>
    public decimal ExceedancePercentage { get; init; }

    /// <summary>Gets the severity classification.</summary>
    public string Severity { get; init; } = string.Empty;

    /// <summary>Gets the UTC instant the exceedance was detected.</summary>
    public DateTime DetectedAtUtc { get; init; }

    /// <summary>Gets the lifecycle status.</summary>
    public string Status { get; init; } = string.Empty;
}

/// <summary>Response payload for the active-exceedances read-model query.</summary>
public sealed class ListActiveExceedancesResponseDto
{
    /// <summary>Gets the matching active exceedance records. Never null; empty when no results.</summary>
    public IReadOnlyList<LimitExceedanceDto> Items { get; init; } = [];
}

/// <summary>A single effluent sample record.</summary>
public sealed class EffluentSampleDto
{
    /// <summary>Gets the sample record identifier.</summary>
    public Guid EffluentSampleId { get; init; }

    /// <summary>Gets the identity of the discharge point this sample was collected from.</summary>
    public Guid DischargePointId { get; init; }

    /// <summary>Gets the parameter measured.</summary>
    public string Parameter { get; init; } = string.Empty;

    /// <summary>Gets the measured value.</summary>
    public decimal MeasuredValue { get; init; }

    /// <summary>Gets the unit of measure.</summary>
    public string UnitOfMeasure { get; init; } = string.Empty;

    /// <summary>Gets the UTC instant the sample was collected.</summary>
    public DateTime SampledAtUtc { get; init; }

    /// <summary>Gets the lab source system tag.</summary>
    public string LabSourceSystem { get; init; } = string.Empty;

    /// <summary>Gets the lifecycle status.</summary>
    public string Status { get; init; } = string.Empty;
}

/// <summary>Response payload for the sample-history-by-point read-model query.</summary>
public sealed class GetSampleHistoryByPointResponseDto
{
    /// <summary>Gets the matching sample records, ordered by sample date. Never null; empty when no results.</summary>
    public IReadOnlyList<EffluentSampleDto> Items { get; init; } = [];
}

/// <summary>A single discharge limit record.</summary>
public sealed class DischargeLimitDto
{
    /// <summary>Gets the discharge limit record identifier.</summary>
    public Guid DischargeLimitId { get; init; }

    /// <summary>Gets the identity of the discharge point this limit governs.</summary>
    public Guid DischargePointId { get; init; }

    /// <summary>Gets the parameter this limit governs.</summary>
    public string Parameter { get; init; } = string.Empty;

    /// <summary>Gets the statistical basis (e.g. DailyMax, MonthlyAverage).</summary>
    public string LimitBasis { get; init; } = string.Empty;

    /// <summary>Gets the current limit value.</summary>
    public decimal LimitValue { get; init; }

    /// <summary>Gets the unit of measure.</summary>
    public string UnitOfMeasure { get; init; } = string.Empty;

    /// <summary>Gets the lifecycle status.</summary>
    public string Status { get; init; } = string.Empty;
}

/// <summary>Response payload for the discharge-limits-by-point read-model query.</summary>
public sealed class ListDischargeLimitsByPointResponseDto
{
    /// <summary>Gets the matching discharge limit records. Never null; empty when no results.</summary>
    public IReadOnlyList<DischargeLimitDto> Items { get; init; } = [];
}

/// <summary>A single Discharge Monitoring Report (DMR) record.</summary>
public sealed class DischargeMonitoringReportDto
{
    /// <summary>Gets the DMR record identifier.</summary>
    public Guid DischargeMonitoringReportId { get; init; }

    /// <summary>Gets the identity of the owning discharge permit.</summary>
    public Guid DischargePermitId { get; init; }

    /// <summary>Gets the UTC start of the reporting period.</summary>
    public DateTime ReportingPeriodStartUtc { get; init; }

    /// <summary>Gets the UTC end of the reporting period.</summary>
    public DateTime ReportingPeriodEndUtc { get; init; }

    /// <summary>Gets the UTC due date.</summary>
    public DateTime DueAtUtc { get; init; }

    /// <summary>Gets the UTC submission timestamp, if submitted.</summary>
    public DateTime? SubmittedAtUtc { get; init; }

    /// <summary>Gets the count of samples within limit for the period.</summary>
    public int SamplesWithinLimitCount { get; init; }

    /// <summary>Gets the total samples for the period.</summary>
    public int TotalSamplesCount { get; init; }

    /// <summary>Gets the lifecycle status.</summary>
    public string Status { get; init; } = string.Empty;
}

/// <summary>Response payload for the DMRs-by-period read-model query.</summary>
public sealed class GetDischargeMonitoringReportsByPeriodResponseDto
{
    /// <summary>Gets the matching DMR records. Never null; empty when no results.</summary>
    public IReadOnlyList<DischargeMonitoringReportDto> Items { get; init; } = [];
}

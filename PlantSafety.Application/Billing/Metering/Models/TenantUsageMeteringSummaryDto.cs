// AUTHOR: PlantSafety Platform Team (Phase 7 §3.1 Usage Metering)
// CREATED: 2026-08-03
// FILE DESCRIPTION: Canonical read model for a tenant's composed usage-metering summary for a
//   single billing period, before it is persisted as a UsageMeteringSnapshot.
// SchemaVersion: 1

namespace PlantSafety.Application.Billing.Metering.Models;

/// <summary>A single metered dimension's recorded value, with the data source it was composed from.</summary>
/// <param name="MetricCode">The <c>UsageMetricType.Code</c> this value was recorded for.</param>
/// <param name="RecordedValue">The recorded usage value for the period.</param>
/// <param name="SourceDescription">Human-readable description of which existing platform data source this value was composed from.</param>
public sealed record MeteredDimensionDto(string MetricCode, decimal RecordedValue, string SourceDescription);

/// <summary>Canonical read model for a tenant's composed usage-metering summary for a single billing period.</summary>
/// <param name="TenantId">The tenant this summary was composed for.</param>
/// <param name="PeriodYear">Calendar year of the billing period (UTC).</param>
/// <param name="PeriodMonth">Calendar month of the billing period (UTC).</param>
/// <param name="Dimensions">One entry per metered usage dimension.</param>
public sealed record TenantUsageMeteringSummaryDto(
    Guid TenantId, int PeriodYear, int PeriodMonth, IReadOnlyList<MeteredDimensionDto> Dimensions);

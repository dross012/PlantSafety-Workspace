// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability.AdvancedMetrics
//  Layer:          Application
//  File:           RecordWaterIntensityMetricCommand.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Command to record a water-intensity meter read against the open
//                  AdvancedMetricsLedger for the given tenant/site/reporting period. Backs the
//                  daily/monthly meter-read scheduling hook. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Sustainability.AdvancedMetrics.Commands;

/// <summary>Records a water-intensity meter read.</summary>
public sealed record RecordWaterIntensityMetricCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid? SiteId,
    DateTime ReportingPeriodStartUtc,
    DateTime ReportingPeriodEndUtc,
    decimal WaterConsumedCubicMeters,
    decimal OutputUnits,
    string MethodologyVersion,
    DateTime ReadingAtUtc,
    Guid RequesterUserId) : ICommand<Result<RecordWaterIntensityMetricResult>>
{
    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => RequesterUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<RecordWaterIntensityMetricResult>>.CausationId => CausationId;

    /// <summary>Builds a <see cref="RecordWaterIntensityMetricCommand"/> from raw, untrusted transport values.</summary>
    public static RecordWaterIntensityMetricCommand FromDto(
        Guid tenantId, Guid? siteId, DateTime reportingPeriodStartUtc, DateTime reportingPeriodEndUtc,
        decimal waterConsumedCubicMeters, decimal outputUnits, string methodologyVersion, DateTime readingAtUtc,
        Guid requesterUserId, Guid? correlationId = null, Guid? causationId = null, string apiVersionToken = "v1")
        => new(
            CommandId: Guid.NewGuid(),
            CorrelationId: correlationId ?? Guid.NewGuid(),
            CausationId: causationId ?? Guid.NewGuid(),
            RequestedAtUtc: DateTime.UtcNow,
            ApiVersionToken: apiVersionToken,
            TenantId: tenantId,
            SiteId: siteId,
            ReportingPeriodStartUtc: reportingPeriodStartUtc,
            ReportingPeriodEndUtc: reportingPeriodEndUtc,
            WaterConsumedCubicMeters: waterConsumedCubicMeters,
            OutputUnits: outputUnits,
            MethodologyVersion: (methodologyVersion ?? string.Empty).Trim(),
            ReadingAtUtc: readingAtUtc,
            RequesterUserId: requesterUserId);
}

/// <summary>Result of a successful <see cref="RecordWaterIntensityMetricCommand"/> execution.</summary>
public sealed record RecordWaterIntensityMetricResult(
    Guid AdvancedMetricsLedgerId,
    Guid WaterIntensityMetricId,
    decimal IntensityValue);

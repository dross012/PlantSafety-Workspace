// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability.AdvancedMetrics
//  Layer:          Application
//  File:           RecordEnergyIntensityMetricCommand.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Command to record an energy-intensity meter read against the open
//                  AdvancedMetricsLedger for the given tenant/site/reporting period. Backs the
//                  daily/monthly meter-read scheduling hook. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Sustainability.AdvancedMetrics.Commands;

/// <summary>Records an energy-intensity meter read.</summary>
public sealed record RecordEnergyIntensityMetricCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid? SiteId,
    DateTime ReportingPeriodStartUtc,
    DateTime ReportingPeriodEndUtc,
    decimal EnergyConsumedKwh,
    decimal OutputUnits,
    string MethodologyVersion,
    DateTime ReadingAtUtc,
    Guid RequesterUserId) : ICommand<Result<RecordEnergyIntensityMetricResult>>
{
    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => RequesterUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<RecordEnergyIntensityMetricResult>>.CausationId => CausationId;

    /// <summary>Builds a <see cref="RecordEnergyIntensityMetricCommand"/> from raw, untrusted transport values.</summary>
    public static RecordEnergyIntensityMetricCommand FromDto(
        Guid tenantId, Guid? siteId, DateTime reportingPeriodStartUtc, DateTime reportingPeriodEndUtc,
        decimal energyConsumedKwh, decimal outputUnits, string methodologyVersion, DateTime readingAtUtc,
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
            EnergyConsumedKwh: energyConsumedKwh,
            OutputUnits: outputUnits,
            MethodologyVersion: (methodologyVersion ?? string.Empty).Trim(),
            ReadingAtUtc: readingAtUtc,
            RequesterUserId: requesterUserId);
}

/// <summary>Result of a successful <see cref="RecordEnergyIntensityMetricCommand"/> execution.</summary>
public sealed record RecordEnergyIntensityMetricResult(
    Guid AdvancedMetricsLedgerId,
    Guid EnergyIntensityMetricId,
    decimal IntensityValue);

// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability.AdvancedMetrics
//  Layer:          Application
//  File:           RecordWasteDiversionCommand.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Command to record a waste diversion interval against the open
//                  AdvancedMetricsLedger for the given tenant/site/reporting period.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Sustainability.AdvancedMetrics.Commands;

/// <summary>Records a waste diversion interval.</summary>
public sealed record RecordWasteDiversionCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid? SiteId,
    DateTime ReportingPeriodStartUtc,
    DateTime ReportingPeriodEndUtc,
    decimal TotalWasteKg,
    decimal DivertedWasteKg,
    string MethodologyVersion,
    DateTime IntervalEndAtUtc,
    Guid RequesterUserId) : ICommand<Result<RecordWasteDiversionResult>>
{
    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => RequesterUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<RecordWasteDiversionResult>>.CausationId => CausationId;

    /// <summary>Builds a <see cref="RecordWasteDiversionCommand"/> from raw, untrusted transport values.</summary>
    public static RecordWasteDiversionCommand FromDto(
        Guid tenantId, Guid? siteId, DateTime reportingPeriodStartUtc, DateTime reportingPeriodEndUtc,
        decimal totalWasteKg, decimal divertedWasteKg, string methodologyVersion, DateTime intervalEndAtUtc,
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
            TotalWasteKg: totalWasteKg,
            DivertedWasteKg: divertedWasteKg,
            MethodologyVersion: (methodologyVersion ?? string.Empty).Trim(),
            IntervalEndAtUtc: intervalEndAtUtc,
            RequesterUserId: requesterUserId);
}

/// <summary>Result of a successful <see cref="RecordWasteDiversionCommand"/> execution.</summary>
public sealed record RecordWasteDiversionResult(
    Guid AdvancedMetricsLedgerId,
    Guid WasteDiversionRecordId,
    decimal DiversionRatePercent);

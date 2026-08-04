// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability.AdvancedMetrics
//  Layer:          Application
//  File:           SetSustainabilityTargetCommand.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Command to set (or re-baseline) a sustainability KPI target against the open
//                  AdvancedMetricsLedger for the given tenant/site/reporting period.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Sustainability.AdvancedMetrics.Commands;

/// <summary>Sets (or re-baselines) a sustainability KPI target.</summary>
public sealed record SetSustainabilityTargetCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid? SiteId,
    DateTime ReportingPeriodStartUtc,
    DateTime ReportingPeriodEndUtc,
    string TargetType,
    decimal BaselineValue,
    decimal TargetValue,
    DateTime TargetDateUtc,
    Guid RequesterUserId) : ICommand<Result<SetSustainabilityTargetResult>>
{
    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => RequesterUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<SetSustainabilityTargetResult>>.CausationId => CausationId;

    /// <summary>Builds a <see cref="SetSustainabilityTargetCommand"/> from raw, untrusted transport values.</summary>
    public static SetSustainabilityTargetCommand FromDto(
        Guid tenantId, Guid? siteId, DateTime reportingPeriodStartUtc, DateTime reportingPeriodEndUtc,
        string targetType, decimal baselineValue, decimal targetValue, DateTime targetDateUtc,
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
            TargetType: targetType,
            BaselineValue: baselineValue,
            TargetValue: targetValue,
            TargetDateUtc: targetDateUtc,
            RequesterUserId: requesterUserId);
}

/// <summary>Result of a successful <see cref="SetSustainabilityTargetCommand"/> execution.</summary>
public sealed record SetSustainabilityTargetResult(
    Guid AdvancedMetricsLedgerId,
    Guid SustainabilityTargetId,
    string Status);

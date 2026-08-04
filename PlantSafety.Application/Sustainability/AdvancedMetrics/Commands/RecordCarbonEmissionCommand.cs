// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability.AdvancedMetrics
//  Layer:          Application
//  File:           RecordCarbonEmissionCommand.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Command to record a single Scope 1/2/3 carbon emission activity against the
//                  open AdvancedMetricsLedger for the given tenant/site/reporting period (the
//                  ledger is get-or-created by the handler). Carries the full command envelope per
//                  Handbook Rule 2.1. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Sustainability.AdvancedMetrics.Commands;

/// <summary>Records a single Scope 1/2/3 carbon emission activity.</summary>
public sealed record RecordCarbonEmissionCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid? SiteId,
    DateTime ReportingPeriodStartUtc,
    DateTime ReportingPeriodEndUtc,
    string Scope,
    string ActivityDescription,
    decimal EmissionsMetricTonsCo2e,
    string MethodologyVersion,
    DateTime ActivityAtUtc,
    Guid RequesterUserId) : ICommand<Result<RecordCarbonEmissionResult>>
{
    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => RequesterUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<RecordCarbonEmissionResult>>.CausationId => CausationId;

    /// <summary>Builds a <see cref="RecordCarbonEmissionCommand"/> from raw, untrusted transport values.</summary>
    public static RecordCarbonEmissionCommand FromDto(
        Guid tenantId, Guid? siteId, DateTime reportingPeriodStartUtc, DateTime reportingPeriodEndUtc,
        string scope, string activityDescription, decimal emissionsMetricTonsCo2e, string methodologyVersion,
        DateTime activityAtUtc, Guid requesterUserId,
        Guid? correlationId = null, Guid? causationId = null, string apiVersionToken = "v1")
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
            Scope: scope,
            ActivityDescription: (activityDescription ?? string.Empty).Trim(),
            EmissionsMetricTonsCo2e: emissionsMetricTonsCo2e,
            MethodologyVersion: (methodologyVersion ?? string.Empty).Trim(),
            ActivityAtUtc: activityAtUtc,
            RequesterUserId: requesterUserId);
}

/// <summary>Result of a successful <see cref="RecordCarbonEmissionCommand"/> execution.</summary>
public sealed record RecordCarbonEmissionResult(
    Guid AdvancedMetricsLedgerId,
    Guid CarbonEmissionRecordId,
    decimal EmissionsMetricTonsCo2e);

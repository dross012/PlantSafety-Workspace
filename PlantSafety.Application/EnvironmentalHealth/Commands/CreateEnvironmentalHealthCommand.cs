// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth
//  Layer:          Application
//  File:           CreateEnvironmentalHealthCommand.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Command to submit a new EnvironmentalHealth record under the
//                  EnvironmentalHealth bounded context. Carries the full command envelope per
//                  Handbook Rule 2.1. Mirrors
//                  PlantSafety.Application.Compliance.Auditing.Commands.CreateAuditingCommand.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.EnvironmentalHealth.Commands;

/// <summary>Submits a new EnvironmentalHealth record.</summary>
public sealed record CreateEnvironmentalHealthCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid? SiteId,
    string MonitoringType,
    decimal? MeasuredValue,
    string Unit,
    decimal Threshold,
    bool ThresholdExceeded,
    string EvaluatedStatus,
    Guid RequesterUserId) : ICommand<Result<CreateEnvironmentalHealthResult>>
{
    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => RequesterUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<CreateEnvironmentalHealthResult>>.CausationId => CausationId;

    /// <summary>Builds a <see cref="CreateEnvironmentalHealthCommand"/> from raw, untrusted transport values.</summary>
    public static CreateEnvironmentalHealthCommand FromDto(
        Guid tenantId,
        Guid? siteId,
        string monitoringType,
        decimal? measuredValue,
        string unit,
        decimal threshold,
        bool thresholdExceeded,
        string evaluatedStatus,
        Guid requesterUserId,
        Guid? correlationId = null,
        Guid? causationId = null,
        string apiVersionToken = "v1")
        => new(
            CommandId: Guid.NewGuid(),
            CorrelationId: correlationId ?? Guid.NewGuid(),
            CausationId: causationId ?? Guid.NewGuid(),
            RequestedAtUtc: DateTime.UtcNow,
            ApiVersionToken: apiVersionToken,
            TenantId: tenantId,
            SiteId: siteId,
            MonitoringType: (monitoringType ?? string.Empty).Trim(),
            MeasuredValue: measuredValue,
            Unit: (unit ?? string.Empty).Trim(),
            Threshold: threshold,
            ThresholdExceeded: thresholdExceeded,
            EvaluatedStatus: (evaluatedStatus ?? "Normal").Trim(),
            RequesterUserId: requesterUserId);
}

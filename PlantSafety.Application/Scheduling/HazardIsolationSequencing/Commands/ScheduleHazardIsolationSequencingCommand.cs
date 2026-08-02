// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.HazardIsolationSequencing
//  Layer:          Application
//  File:           ScheduleHazardIsolationSequencingCommand.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Command to schedule a new HazardIsolationSequencing record -- isolation and
//                  sequencing of incompatible hazardous tasks (hot work, chemical handling, heavy
//                  lifting) within a hazard zone. Carries the full command envelope (CorrelationId,
//                  CausationId, CommandId, RequestedAtUtc, ApiVersionToken) per Handbook Rule 2.1.
//                  Mirrors PlantSafety.Application.Compliance.Permits.Commands.CreatePermitsCommand.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Scheduling.HazardIsolationSequencing.Commands;

/// <summary>Schedules a new HazardIsolationSequencing record.</summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier, constant across the whole request.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event. Null only for root operations.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued, generated once at the API boundary.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site scope. Null applies tenant-wide.</param>
/// <param name="HazardZone">Hazard zone the isolation window applies to.</param>
/// <param name="TaskType">Classification of the hazardous task being isolated/sequenced.</param>
/// <param name="WindowStartUtc">Optional UTC start of the isolation window.</param>
/// <param name="WindowEndUtc">Optional UTC end of the isolation window.</param>
/// <param name="RequesterUserId">Identity of the user scheduling this isolation window.</param>
public sealed record ScheduleHazardIsolationSequencingCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid? SiteId,
    string HazardZone,
    string TaskType,
    DateTime? WindowStartUtc,
    DateTime? WindowEndUtc,
    Guid RequesterUserId) : ICommand<Result<ScheduleHazardIsolationSequencingResult>>
{
    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => RequesterUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<ScheduleHazardIsolationSequencingResult>>.CausationId => CausationId;

    /// <summary>Builds a <see cref="ScheduleHazardIsolationSequencingCommand"/> from raw, untrusted transport values.</summary>
    public static ScheduleHazardIsolationSequencingCommand FromDto(
        Guid tenantId,
        Guid? siteId,
        string hazardZone,
        string taskType,
        DateTime? windowStartUtc,
        DateTime? windowEndUtc,
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
            HazardZone: (hazardZone ?? string.Empty).Trim(),
            TaskType: (taskType ?? string.Empty).Trim(),
            WindowStartUtc: windowStartUtc,
            WindowEndUtc: windowEndUtc,
            RequesterUserId: requesterUserId);
}

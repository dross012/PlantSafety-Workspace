// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.DrillsAndExercises
//  Layer:          Application
//  File:           ScheduleDrillsAndExercisesCommand.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Command to schedule a new DrillsAndExercises drill under the
//                  SafetyEvents.DrillsAndExercises bounded context. Carries the full command
//                  envelope (CorrelationId, CausationId, CommandId, RequestedAtUtc,
//                  ApiVersionToken) per Handbook Rule 2.1. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.CreatePermitsCommand.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.SafetyEvents.DrillsAndExercises.Commands;

/// <summary>Schedules a new DrillsAndExercises drill.</summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier, constant across the whole request.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event. Null only for root operations.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued, generated once at the API boundary.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site scope. Null applies tenant-wide.</param>
/// <param name="DrillType">Type of drill (e.g., FireDrill/Evacuation/ShelterInPlace/SpillResponse).</param>
/// <param name="ScheduledAtUtc">Optional UTC instant the drill is scheduled for.</param>
/// <param name="ScheduledByUserId">Identity of the user or system actor scheduling the drill.</param>
public sealed record ScheduleDrillsAndExercisesCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid? SiteId,
    string DrillType,
    DateTime? ScheduledAtUtc,
    Guid ScheduledByUserId) : ICommand<Result<DrillsAndExercisesStatusResult>>
{
    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => ScheduledByUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<DrillsAndExercisesStatusResult>>.CausationId => CausationId;

    /// <summary>Builds a <see cref="ScheduleDrillsAndExercisesCommand"/> from raw, untrusted transport values.</summary>
    public static ScheduleDrillsAndExercisesCommand FromDto(
        Guid tenantId,
        Guid? siteId,
        string drillType,
        DateTime? scheduledAtUtc,
        Guid scheduledByUserId,
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
            DrillType: (drillType ?? string.Empty).Trim(),
            ScheduledAtUtc: scheduledAtUtc,
            ScheduledByUserId: scheduledByUserId);
}

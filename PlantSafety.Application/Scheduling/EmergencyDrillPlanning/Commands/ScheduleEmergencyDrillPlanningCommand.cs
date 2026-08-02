// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.EmergencyDrillPlanning
//  Layer:          Application
//  File:           ScheduleEmergencyDrillPlanningCommand.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Command to schedule a new emergency drill. Carries the full command envelope
//                  (CorrelationId, CausationId, CommandId, RequestedAtUtc, ApiVersionToken) per
//                  Handbook Rule 2.1. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.CreatePermitsCommand.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Scheduling.EmergencyDrillPlanning.Commands;

/// <summary>Schedules a new emergency drill.</summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier, constant across the whole request.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event. Null only for root operations.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued, generated once at the API boundary.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site scope. Null applies tenant-wide.</param>
/// <param name="DrillType">Drill classification (e.g., FireEvacuation, ChemicalSpill, SevereWeather, AlarmTest).</param>
/// <param name="ShiftCode">Shift the drill is scheduled for.</param>
/// <param name="ScheduledAtUtc">Optional UTC instant the drill is scheduled to occur.</param>
/// <param name="ParticipationCount">Expected/initial participation count.</param>
/// <param name="ScheduledByUserId">Identity of the user scheduling the drill.</param>
public sealed record ScheduleEmergencyDrillPlanningCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid? SiteId,
    string DrillType,
    string ShiftCode,
    DateTime? ScheduledAtUtc,
    int ParticipationCount,
    Guid ScheduledByUserId) : ICommand<Result<ScheduleEmergencyDrillPlanningResult>>
{
    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => ScheduledByUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<ScheduleEmergencyDrillPlanningResult>>.CausationId => CausationId;

    /// <summary>Builds a <see cref="ScheduleEmergencyDrillPlanningCommand"/> from raw, untrusted transport values.</summary>
    public static ScheduleEmergencyDrillPlanningCommand FromDto(
        Guid tenantId,
        Guid? siteId,
        string drillType,
        string shiftCode,
        DateTime? scheduledAtUtc,
        int participationCount,
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
            ShiftCode: (shiftCode ?? string.Empty).Trim(),
            ScheduledAtUtc: scheduledAtUtc,
            ParticipationCount: participationCount,
            ScheduledByUserId: scheduledByUserId);
}

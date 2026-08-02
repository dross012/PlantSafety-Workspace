// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.ShiftFatigue
//  Layer:          Application
//  File:           RecordShiftFatigueCommand.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Command to record a new ShiftFatigue assessment for an employee's shift pattern
//                  -- shift length limits, rest periods, night-shift restrictions, job rotation,
//                  fatigue risk scoring. Named around "recording" a fatigue assessment (not
//                  "scheduling") since this aggregate is not itself a scheduled event. Carries the
//                  full command envelope (CorrelationId, CausationId, CommandId, RequestedAtUtc,
//                  ApiVersionToken) per Handbook Rule 2.1. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.CreatePermitsCommand.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Scheduling.ShiftFatigue.Commands;

/// <summary>Records a new ShiftFatigue assessment.</summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier, constant across the whole request.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event. Null only for root operations.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued, generated once at the API boundary.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site scope. Null applies tenant-wide.</param>
/// <param name="EmployeeId">Identity of the employee the fatigue assessment applies to.</param>
/// <param name="ShiftPattern">Shift pattern classification (e.g., Rotating, FixedNight, FourOnFourOff).</param>
/// <param name="ConsecutiveNightShifts">Number of consecutive night shifts worked at the time of recording.</param>
/// <param name="OvertimeHours">Cumulative overtime hours at the time of recording.</param>
/// <param name="FatigueRiskScore">Initial computed fatigue risk score.</param>
/// <param name="RequesterUserId">Identity of the user recording this assessment.</param>
public sealed record RecordShiftFatigueCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid? SiteId,
    Guid EmployeeId,
    string ShiftPattern,
    int ConsecutiveNightShifts,
    decimal OvertimeHours,
    decimal FatigueRiskScore,
    Guid RequesterUserId) : ICommand<Result<RecordShiftFatigueResult>>
{
    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => RequesterUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<RecordShiftFatigueResult>>.CausationId => CausationId;

    /// <summary>Builds a <see cref="RecordShiftFatigueCommand"/> from raw, untrusted transport values.</summary>
    public static RecordShiftFatigueCommand FromDto(
        Guid tenantId,
        Guid? siteId,
        Guid employeeId,
        string shiftPattern,
        int consecutiveNightShifts,
        decimal overtimeHours,
        decimal fatigueRiskScore,
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
            EmployeeId: employeeId,
            ShiftPattern: (shiftPattern ?? string.Empty).Trim(),
            ConsecutiveNightShifts: consecutiveNightShifts,
            OvertimeHours: overtimeHours,
            FatigueRiskScore: fatigueRiskScore,
            RequesterUserId: requesterUserId);
}

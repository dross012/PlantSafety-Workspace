// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Command to prepare a new DischargeMonitoringReport (DMR) in Draft status
//   ("EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport").
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.Commands;

/// <summary>Prepares a new Discharge Monitoring Report in Draft status.</summary>
public sealed record PrepareDischargeMonitoringReportCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid? SiteId,
    Guid DischargePermitId,
    DateTime ReportingPeriodStartUtc,
    DateTime ReportingPeriodEndUtc,
    DateTime DueAtUtc,
    Guid PerformedByUserId) : ICommand<Result<DischargeMonitoringReportActionResult>>
{
    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    Guid? ICommand<Result<DischargeMonitoringReportActionResult>>.CausationId => CausationId;

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => PerformedByUserId;
}
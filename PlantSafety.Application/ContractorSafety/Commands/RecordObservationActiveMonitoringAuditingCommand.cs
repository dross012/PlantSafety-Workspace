// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Command to RecordObservation (lifecycle transition) for ActiveMonitoringAuditing ("ContractorSafety.ActiveMonitoringAuditing").
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.ContractorSafety.Commands;

/// <summary>Invokes <c>ActiveMonitoringAuditing.RecordObservation</c> on an existing ActiveMonitoringAuditing aggregate.</summary>
public sealed record RecordObservationActiveMonitoringAuditingCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid ActiveMonitoringAuditingId,
    int UnsafeConditionCount,
    decimal ComplianceScore,
    string? FindingNotes,
    Guid PerformedByUserId) : ICommand<Result<ActiveMonitoringAuditingActionResult>>
{
    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    Guid? ICommand<Result<ActiveMonitoringAuditingActionResult>>.CausationId => CausationId;

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => PerformedByUserId;
}

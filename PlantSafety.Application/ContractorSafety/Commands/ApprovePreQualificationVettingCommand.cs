// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Command to Approve (lifecycle transition) for PreQualificationVetting ("ContractorSafety.PreQualificationVetting").
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.ContractorSafety.Commands;

/// <summary>Invokes <c>PreQualificationVetting.Approve</c> on an existing PreQualificationVetting aggregate.</summary>
public sealed record ApprovePreQualificationVettingCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid PreQualificationVettingId,
    decimal EmrScore,
    decimal TrirScore,
    bool OshaTrainingCompliant,
    int DeficiencyCount,
    DateTime InsuranceExpiresAtUtc,
    int ValidityPeriodDays,
    Guid PerformedByUserId) : ICommand<Result<PreQualificationVettingActionResult>>
{
    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    Guid? ICommand<Result<PreQualificationVettingActionResult>>.CausationId => CausationId;

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => PerformedByUserId;
}

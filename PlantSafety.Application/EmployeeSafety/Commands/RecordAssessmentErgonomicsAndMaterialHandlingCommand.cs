// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Command to RecordAssessment (lifecycle transition) for ErgonomicsAndMaterialHandling ("EmployeeSafety.ErgonomicsAndMaterialHandling").
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.EmployeeSafety.Commands;

/// <summary>Invokes <c>ErgonomicsAndMaterialHandling.RecordAssessment</c> on an existing ErgonomicsAndMaterialHandling aggregate.</summary>
public sealed record RecordAssessmentErgonomicsAndMaterialHandlingCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid ErgonomicsAndMaterialHandlingId,
    int RiskFactorCount,
    decimal RiskScore,
    Guid PerformedByUserId) : ICommand<Result<ErgonomicsAndMaterialHandlingActionResult>>
{
    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    Guid? ICommand<Result<ErgonomicsAndMaterialHandlingActionResult>>.CausationId => CausationId;

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => PerformedByUserId;
}

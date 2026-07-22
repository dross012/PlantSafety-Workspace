// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Command to Publish (lifecycle transition) for MaterialHandlingStorage ("SafetyProcedures.MaterialHandlingStorage").
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.SafetyProcedures.Commands;

/// <summary>Invokes <c>MaterialHandlingStorage.Publish</c> on an existing MaterialHandlingStorage aggregate.</summary>
public sealed record PublishMaterialHandlingStorageCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid MaterialHandlingStorageId,
    Guid ApprovedByUserId,
    int ReviewCycleDays,
    Guid PerformedByUserId) : ICommand<Result<MaterialHandlingStorageActionResult>>
{
    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    Guid? ICommand<Result<MaterialHandlingStorageActionResult>>.CausationId => CausationId;

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => PerformedByUserId;
}

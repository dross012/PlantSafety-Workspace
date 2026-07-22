// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Command to publish a draft PermitTemplate, making it available for
// permit requests of its category.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.PermitToWork.Commands;

/// <summary>Publishes a draft PermitTemplate, making it active for permit requests.</summary>
public sealed record PublishPermitTemplateCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid PermitTemplateId,
    Guid PerformedByUserId) : ICommand<Result<PermitTemplateActionResult>>
{
    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => PerformedByUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<PermitTemplateActionResult>>.CausationId => CausationId;
}

// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Command to deprecate an active PermitTemplate so no new permits may be
// requested against it.
// SchemaVersion: 1
// UPDATED: 2026-07-29 -- Implements IConcurrencyAwareCommand (Gap 2 remediation): callers should
//   supply the ETag captured from a prior read; a stale value results in ConcurrencyConflictException.

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Behaviors.Concurrency;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.PermitToWork.Commands;

/// <summary>Deprecates an active PermitTemplate.</summary>
public sealed record DeprecatePermitTemplateCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid PermitTemplateId,
    Guid PerformedByUserId,
    string Reason) : ICommand<Result<PermitTemplateActionResult>>, IConcurrencyAwareCommand
{
    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => PerformedByUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<PermitTemplateActionResult>>.CausationId => CausationId;

    /// <inheritdoc/>
    public string? ETag { get; init; }
}

// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Command to apply a single operation uniformly across multiple Scheduling resources.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Scheduling.Commands;

/// <summary>
/// Applies <paramref name="Operation"/> (e.g. "Close", "Approve", "Reassign") to every resource in
/// <paramref name="ResourceIds"/>. Uses partial-success semantics: individual item failures do not
/// fail the whole command, they are reported in <see cref="BulkUpdateSchedulingResult.Results"/>.
/// </summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site scope. Null applies tenant-wide.</param>
/// <param name="ResourceIds">Identifiers of the Scheduling resources to operate on. Maximum 500 per request.</param>
/// <param name="Operation">The operation to apply (e.g. "Close", "Approve", "Reassign").</param>
/// <param name="Parameters">Optional operation-specific key-value data.</param>
public sealed record BulkUpdateSchedulingCommand(
    Guid TenantId,
    Guid? SiteId,
    IReadOnlyList<Guid> ResourceIds,
    string Operation,
    Dictionary<string, string>? Parameters) : ICommand<Result<BulkUpdateSchedulingResult>>
{
    /// <summary>Gets the unique identifier of this command instance (UUIDv7).</summary>
    public Guid CommandId { get; init; } = Guid.NewGuid();

    /// <summary>Gets the correlation identifier that groups all operations in a single business transaction.</summary>
    public Guid CorrelationId { get; init; } = Guid.NewGuid();

    /// <summary>Gets the causation identifier that identifies the immediate parent command or event.</summary>
    public Guid? CausationId { get; init; }

    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the identifier of the actor who initiated the command.
    /// Placeholder: no dedicated actor field exists on this bulk command yet; defaults to <see cref="Guid.Empty"/>.</summary>
    public Guid ActorId { get; init; } = Guid.Empty;
}
// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-15
// FILE DESCRIPTION: Command to create a new Process Definition record.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;

namespace PlantSafety.Application.Features.ProcessManagement.Commands.Processes.CreateProcess;

/// <summary>
/// Creates a new ProcessDefinition record in Draft status.
/// Caller must hold <c>ProcessManagement.Processes.Create</c>.
/// </summary>
public sealed record CreateProcessCommand : ICommand<Guid>
{
    /// <inheritdoc/>
    public required Guid CommandId { get; init; }

    /// <inheritdoc/>
    public required Guid CorrelationId { get; init; }

    /// <inheritdoc/>
    public required Guid? CausationId { get; init; }

    /// <inheritdoc/>
    public required DateTime RequestedAtUtc { get; init; }

    /// <inheritdoc/>
    public required string ApiVersionToken { get; init; }

    /// <inheritdoc/>
    public required Guid TenantId { get; init; }

    /// <inheritdoc/>
    public required Guid ActorId { get; init; }

    /// <summary>Site this process is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Human-readable process name.</summary>
    public required string ProcessName { get; init; }

    /// <summary>Safety-critical process category (e.g., HazardousMaterials, ConfinedSpace, LOTO, HotWork).</summary>
    public required string Category { get; init; }

    /// <summary>Identity of the accountable process owner.</summary>
    public required Guid OwnerId { get; init; }
}

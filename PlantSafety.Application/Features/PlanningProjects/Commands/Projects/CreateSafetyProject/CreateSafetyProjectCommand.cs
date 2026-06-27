// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Command to create a Safety Project in Draft status.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;

namespace PlantSafety.Application.Features.PlanningProjects.Commands.Projects.CreateSafetyProject;

/// <summary>
/// Creates a new Safety Project in Draft status.
/// Caller must hold <c>PlanningProjects.Projects.Create</c>.
/// </summary>
public sealed record CreateSafetyProjectCommand : ICommand<Guid>
{
    /// <inheritdoc/>
    public required Guid CommandId { get; init; }

    /// <inheritdoc/>
    public required Guid CorrelationId { get; init; }

    /// <inheritdoc/>
    public required Guid CausationId { get; init; }

    /// <inheritdoc/>
    public required DateTimeOffset RequestedAtUtc { get; init; }

    /// <inheritdoc/>
    public required string ApiVersionToken { get; init; }

    /// <inheritdoc/>
    public required Guid TenantId { get; init; }

    /// <inheritdoc/>
    public required Guid ActorId { get; init; }

    /// <summary>Primary site for this project.</summary>
    public required Guid SiteId { get; init; }

    /// <summary>Project name. Max 200 characters.</summary>
    public required string Name { get; init; }

    /// <summary>Description of scope and objectives. Max 4000 characters.</summary>
    public required string Description { get; init; }

    /// <summary>Priority (Low, Medium, High, Critical).</summary>
    public required string Priority { get; init; }

    /// <summary>Project owner.</summary>
    public required Guid ProjectOwnerId { get; init; }

    /// <summary>Project manager, if assigned at creation.</summary>
    public Guid? ProjectManagerId { get; init; }

    /// <summary>Planned start date (UTC).</summary>
    public required DateTimeOffset PlannedStartUtc { get; init; }

    /// <summary>Planned completion date (UTC).</summary>
    public required DateTimeOffset PlannedEndUtc { get; init; }

    /// <summary>Planned budget, if known.</summary>
    public decimal? PlannedBudget { get; init; }
}

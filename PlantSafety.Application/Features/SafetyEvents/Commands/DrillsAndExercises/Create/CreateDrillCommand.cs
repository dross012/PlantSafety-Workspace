// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Command to schedule a Safety Drill or Exercise.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;

namespace PlantSafety.Application.Features.SafetyEvents.Commands.DrillsAndExercises.Create;

/// <summary>
/// Schedules a Safety Drill or Exercise.
/// Caller must hold <c>SafetyEvents.Drills.Create</c>.
/// </summary>
public sealed record CreateDrillCommand : ICommand<Guid>
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

    /// <summary>Site where the drill will be conducted.</summary>
    public required Guid SiteId { get; init; }

    /// <summary>Short title. Max 200 characters.</summary>
    public required string Title { get; init; }

    /// <summary>Description of objectives and scope. Max 4000 characters.</summary>
    public required string Description { get; init; }

    /// <summary>Drill type (e.g., Evacuation, FireResponse, ChemicalSpill, MedicalEmergency).</summary>
    public required string DrillType { get; init; }

    /// <summary>UTC timestamp when the drill is scheduled to occur.</summary>
    public required DateTimeOffset ScheduledAtUtc { get; init; }

    /// <summary>Drill coordinator.</summary>
    public required Guid CoordinatorId { get; init; }
}

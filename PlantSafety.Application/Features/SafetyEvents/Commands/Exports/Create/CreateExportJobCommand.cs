// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Command to queue an asynchronous safety event data export job.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;

namespace PlantSafety.Application.Features.SafetyEvents.Commands.Exports.Create;

/// <summary>
/// Queues an asynchronous export job for safety event data.
/// Returns the <see cref="Guid"/> of the newly created export job.
/// Caller must hold <c>SafetyEvents.Exports.Create</c>.
/// </summary>
public sealed record CreateExportJobCommand : ICommand<Guid>
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

    /// <summary>Type of data to export (e.g., Incidents, NearMisses, Observations).</summary>
    public required string ExportType { get; init; }

    /// <summary>Export format: csv or json.</summary>
    public required string Format { get; init; }

    /// <summary>Inclusive lower bound on the occurrence date filter.</summary>
    public DateTimeOffset? FromUtc { get; init; }

    /// <summary>Inclusive upper bound on the occurrence date filter.</summary>
    public DateTimeOffset? ToUtc { get; init; }

    /// <summary>Optional site filter.</summary>
    public Guid? SiteId { get; init; }
}

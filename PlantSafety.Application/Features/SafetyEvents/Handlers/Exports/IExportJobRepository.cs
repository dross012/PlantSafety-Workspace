// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository interface for the ExportJob aggregate.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.SafetyEvents.Handlers.Exports;

/// <summary>Repository for ExportJob read/write operations.</summary>
public interface IExportJobRepository
{
    /// <summary>Creates a new ExportJob in Queued status; stages an async processing event in the outbox.</summary>
    Task CreateAsync(
        Guid jobId,
        Guid tenantId,
        string exportType,
        string format,
        string filterJson,
        Guid requestedByActorId,
        Guid correlationId,
        CancellationToken cancellationToken);

    /// <summary>Returns the read model, or null if not found.</summary>
    Task<ExportJobReadModel?> GetByIdAsync(Guid tenantId, Guid jobId, CancellationToken cancellationToken);
}

/// <summary>Infrastructure read model for ExportJob stored-procedure result sets.</summary>
public sealed record ExportJobReadModel
{
    /// <summary>Job identifier.</summary>
    public required Guid JobId { get; init; }
    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }
    /// <summary>Export type discriminator.</summary>
    public required string ExportType { get; init; }
    /// <summary>Output format: csv or json.</summary>
    public required string Format { get; init; }
    /// <summary>Job status: Queued/Processing/Completed/Failed.</summary>
    public required string Status { get; init; }
    /// <summary>Download URL; populated when Status is Completed.</summary>
    public required string? DownloadUrl { get; init; }
    /// <summary>Failure reason; populated when Status is Failed.</summary>
    public required string? FailureReason { get; init; }
    /// <summary>Job queued timestamp.</summary>
    public required DateTimeOffset QueuedAtUtc { get; init; }
    /// <summary>Job completion timestamp.</summary>
    public required DateTimeOffset? CompletedAtUtc { get; init; }
}

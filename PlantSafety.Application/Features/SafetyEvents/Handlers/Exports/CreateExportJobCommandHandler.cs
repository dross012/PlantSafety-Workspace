// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for CreateExportJobCommand.
//   Export processing is asynchronous; this handler creates the job in Queued status.
//   The ExportJobProcessor (worker) reads the outbox event and executes the export.
// SchemaVersion: 1

using System.Text.Json;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Clock;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Utilities;
using PlantSafety.Application.Features.SafetyEvents.Commands.Exports.Create;

namespace PlantSafety.Application.Features.SafetyEvents.Handlers.Exports;

/// <summary>Handles <see cref="CreateExportJobCommand"/>. Returns the new JobId.</summary>
public sealed class CreateExportJobCommandHandler : ICommandHandler<CreateExportJobCommand, Guid>
{
    private readonly IExportJobRepository _repository;
    private readonly IDateTimeProvider _clock;
    private readonly ILogger<CreateExportJobCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CreateExportJobCommandHandler(
        IExportJobRepository repository,
        IDateTimeProvider clock,
        ILogger<CreateExportJobCommandHandler> logger)
    {
        _repository = repository;
        _clock = clock;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Guid> Handle(CreateExportJobCommand request, CancellationToken cancellationToken)
    {
        var jobId = CorrelationIdGenerator.NewId(_clock.UtcNowOffset);

        var filterJson = JsonSerializer.Serialize(new
        {
            request.SiteId,
            request.Status,
            request.Severity,
            request.FromUtc,
            request.ToUtc,
            request.MaxRecords,
        });

        await _repository.CreateAsync(
            jobId: jobId,
            tenantId: request.TenantId,
            exportType: "SafetyEvents",
            format: request.Format,
            filterJson: filterJson,
            requestedByActorId: request.ActorId,
            correlationId: request.CorrelationId,
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "ExportJob {JobId} queued for Tenant {TenantId} Format {Format} by Actor {ActorId}",
            jobId, request.TenantId, request.Format, request.ActorId);

        return jobId;
    }
}

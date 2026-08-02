// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin
//  Layer:          Application
//  File:           ArchiveDigitalTwinCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for ArchiveDigitalTwinCommand. Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.DigitalTwin.Repositories;
using PlantSafety.Domain.DigitalTwin.ValueObjects;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.DigitalTwin.Commands;

/// <summary>Handles <see cref="ArchiveDigitalTwinCommand"/>.</summary>
public sealed class ArchiveDigitalTwinCommandHandler : IRequestHandler<ArchiveDigitalTwinCommand, Result<DigitalTwinStatusResult>>
{
    private readonly IDigitalTwinRepository _repository;
    private readonly ILogger<ArchiveDigitalTwinCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ArchiveDigitalTwinCommandHandler(IDigitalTwinRepository repository, ILogger<ArchiveDigitalTwinCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<DigitalTwinStatusResult>> Handle(ArchiveDigitalTwinCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = DigitalTwinId.FromGuid(request.DigitalTwinId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<DigitalTwinStatusResult>.Failure(new ApplicationError(
                    "DigitalTwin.NotFound",
                    "The requested digital-twin record could not be found.",
                    request.DigitalTwinId,
                    ErrorSeverity.NotFound));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.Archive(
                actorUserId: performedBy,
                atUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<DigitalTwinStatusResult>.Success(new DigitalTwinStatusResult(
                aggregate.DigitalTwinId.Value,
                aggregate.Status.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to archive DigitalTwin {DigitalTwinId}", request.DigitalTwinId);

            return Result<DigitalTwinStatusResult>.Failure(new ApplicationError(
                "DigitalTwin.ArchiveFailed",
                "Unable to archive the digital-twin record at this time.",
                Guid.NewGuid()));
        }
    }
}

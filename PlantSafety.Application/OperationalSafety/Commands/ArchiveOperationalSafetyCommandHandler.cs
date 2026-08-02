// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         OperationalSafety
//  Layer:          Application
//  File:           ArchiveOperationalSafetyCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for ArchiveOperationalSafetyCommand.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.OperationalSafety.Repositories;
using PlantSafety.Domain.OperationalSafety.ValueObjects;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.OperationalSafety.Commands;

/// <summary>Handles <see cref="ArchiveOperationalSafetyCommand"/>.</summary>
public sealed class ArchiveOperationalSafetyCommandHandler : IRequestHandler<ArchiveOperationalSafetyCommand, Result<OperationalSafetyStatusResult>>
{
    private readonly IOperationalSafetyRepository _repository;
    private readonly ILogger<ArchiveOperationalSafetyCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ArchiveOperationalSafetyCommandHandler(IOperationalSafetyRepository repository, ILogger<ArchiveOperationalSafetyCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<OperationalSafetyStatusResult>> Handle(ArchiveOperationalSafetyCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = OperationalSafetyId.FromGuid(request.OperationalSafetyId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<OperationalSafetyStatusResult>.Failure(new ApplicationError(
                    "OperationalSafety.NotFound",
                    "The requested operational-safety record could not be found.",
                    request.OperationalSafetyId,
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

            return Result<OperationalSafetyStatusResult>.Success(new OperationalSafetyStatusResult(
                aggregate.OperationalSafetyId.Value,
                aggregate.Status.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to archive OperationalSafety {OperationalSafetyId}", request.OperationalSafetyId);

            return Result<OperationalSafetyStatusResult>.Failure(new ApplicationError(
                "OperationalSafety.ArchiveFailed",
                "Unable to archive the operational-safety record at this time.",
                Guid.NewGuid()));
        }
    }
}

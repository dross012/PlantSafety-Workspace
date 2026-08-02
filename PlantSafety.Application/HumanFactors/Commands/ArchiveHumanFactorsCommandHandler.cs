// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors
//  Layer:          Application
//  File:           ArchiveHumanFactorsCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for ArchiveHumanFactorsCommand.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.HumanFactors.Repositories;
using PlantSafety.Domain.HumanFactors.ValueObjects;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.HumanFactors.Commands;

/// <summary>Handles <see cref="ArchiveHumanFactorsCommand"/>.</summary>
public sealed class ArchiveHumanFactorsCommandHandler : IRequestHandler<ArchiveHumanFactorsCommand, Result<HumanFactorsStatusResult>>
{
    private readonly IHumanFactorsRepository _repository;
    private readonly ILogger<ArchiveHumanFactorsCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ArchiveHumanFactorsCommandHandler(IHumanFactorsRepository repository, ILogger<ArchiveHumanFactorsCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<HumanFactorsStatusResult>> Handle(ArchiveHumanFactorsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = HumanFactorsId.FromGuid(request.HumanFactorsId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<HumanFactorsStatusResult>.Failure(new ApplicationError(
                    "HumanFactors.NotFound",
                    "The requested human-factors record could not be found.",
                    request.HumanFactorsId,
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

            return Result<HumanFactorsStatusResult>.Success(new HumanFactorsStatusResult(
                aggregate.HumanFactorsId.Value,
                aggregate.Status.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to archive HumanFactors {HumanFactorsId}", request.HumanFactorsId);

            return Result<HumanFactorsStatusResult>.Failure(new ApplicationError(
                "HumanFactors.ArchiveFailed",
                "Unable to archive the human-factors record at this time.",
                Guid.NewGuid()));
        }
    }
}

// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors
//  Layer:          Application
//  File:           ActivateHumanFactorsCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for ActivateHumanFactorsCommand. GetByIdAsync is tenant-scoped, the
//                  NotFound ApplicationError uses ErrorSeverity.NotFound, and
//                  ConcurrencyConflictException is re-thrown before the generic catch. Mirrors
//                  PlantSafety.Application.Compliance.Auditing.Commands.StartAuditingCommandHandler.
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

/// <summary>Handles <see cref="ActivateHumanFactorsCommand"/>.</summary>
public sealed class ActivateHumanFactorsCommandHandler : IRequestHandler<ActivateHumanFactorsCommand, Result<HumanFactorsStatusResult>>
{
    private readonly IHumanFactorsRepository _repository;
    private readonly ILogger<ActivateHumanFactorsCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ActivateHumanFactorsCommandHandler(IHumanFactorsRepository repository, ILogger<ActivateHumanFactorsCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<HumanFactorsStatusResult>> Handle(ActivateHumanFactorsCommand request, CancellationToken cancellationToken)
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

            aggregate.Activate(
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
            _logger.LogError(ex, "Failed to activate HumanFactors {HumanFactorsId}", request.HumanFactorsId);

            return Result<HumanFactorsStatusResult>.Failure(new ApplicationError(
                "HumanFactors.ActivateFailed",
                "Unable to activate the human-factors record at this time.",
                Guid.NewGuid()));
        }
    }
}

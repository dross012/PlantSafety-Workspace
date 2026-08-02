// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth
//  Layer:          Application
//  File:           CloseEnvironmentalHealthCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for CloseEnvironmentalHealthCommand.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.EnvironmentalHealth.Repositories;
using PlantSafety.Domain.EnvironmentalHealth.ValueObjects;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.EnvironmentalHealth.Commands;

/// <summary>Handles <see cref="CloseEnvironmentalHealthCommand"/>.</summary>
public sealed class CloseEnvironmentalHealthCommandHandler : IRequestHandler<CloseEnvironmentalHealthCommand, Result<EnvironmentalHealthStatusResult>>
{
    private readonly IEnvironmentalHealthRepository _repository;
    private readonly ILogger<CloseEnvironmentalHealthCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CloseEnvironmentalHealthCommandHandler(IEnvironmentalHealthRepository repository, ILogger<CloseEnvironmentalHealthCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<EnvironmentalHealthStatusResult>> Handle(CloseEnvironmentalHealthCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = EnvironmentalHealthId.FromGuid(request.EnvironmentalHealthId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<EnvironmentalHealthStatusResult>.Failure(new ApplicationError(
                    "EnvironmentalHealth.NotFound",
                    "The requested environmental-health record could not be found.",
                    request.EnvironmentalHealthId,
                    ErrorSeverity.NotFound));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.Close(
                actorUserId: performedBy,
                atUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<EnvironmentalHealthStatusResult>.Success(new EnvironmentalHealthStatusResult(
                aggregate.EnvironmentalHealthId.Value,
                aggregate.Status.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to close EnvironmentalHealth {EnvironmentalHealthId}", request.EnvironmentalHealthId);

            return Result<EnvironmentalHealthStatusResult>.Failure(new ApplicationError(
                "EnvironmentalHealth.CloseFailed",
                "Unable to close the environmental-health record at this time.",
                Guid.NewGuid()));
        }
    }
}

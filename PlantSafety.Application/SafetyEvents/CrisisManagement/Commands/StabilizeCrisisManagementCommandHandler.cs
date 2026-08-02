// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.CrisisManagement
//  Layer:          Application
//  File:           StabilizeCrisisManagementCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for StabilizeCrisisManagementCommand. Mirrors
//                  RespondToCrisisManagementCommandHandler's conventions.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.SafetyEvents.CrisisManagement.Interfaces;
using PlantSafety.Domain.SafetyEvents.CrisisManagement.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.SafetyEvents.CrisisManagement.Commands;

/// <summary>Handles <see cref="StabilizeCrisisManagementCommand"/>.</summary>
public sealed class StabilizeCrisisManagementCommandHandler : IRequestHandler<StabilizeCrisisManagementCommand, Result<CrisisManagementStatusResult>>
{
    private readonly ICrisisManagementRepository _repository;
    private readonly ILogger<StabilizeCrisisManagementCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public StabilizeCrisisManagementCommandHandler(ICrisisManagementRepository repository, ILogger<StabilizeCrisisManagementCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CrisisManagementStatusResult>> Handle(StabilizeCrisisManagementCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = CrisisManagementId.FromGuid(request.CrisisManagementId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<CrisisManagementStatusResult>.Failure(new ApplicationError(
                    "SafetyEvents.CrisisManagement.NotFound",
                    "The requested crisis could not be found.",
                    request.CrisisManagementId,
                    ErrorSeverity.NotFound));
            }

            var stabilizedBy = UserId.FromGuid(request.StabilizedByUserId);

            aggregate.Stabilize(
                stabilizedBy: stabilizedBy,
                stabilizedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(stabilizedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<CrisisManagementStatusResult>.Success(new CrisisManagementStatusResult(
                aggregate.CrisisManagementId.Value,
                aggregate.Status.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stabilize CrisisManagement crisis {CrisisManagementId}", request.CrisisManagementId);

            return Result<CrisisManagementStatusResult>.Failure(new ApplicationError(
                "SafetyEvents.CrisisManagement.StabilizeFailed",
                "Unable to record crisis stabilization at this time.",
                Guid.NewGuid()));
        }
    }
}

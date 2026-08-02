// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.CrisisManagement
//  Layer:          Application
//  File:           ResolveCrisisManagementCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for ResolveCrisisManagementCommand. Mirrors
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

/// <summary>Handles <see cref="ResolveCrisisManagementCommand"/>.</summary>
public sealed class ResolveCrisisManagementCommandHandler : IRequestHandler<ResolveCrisisManagementCommand, Result<CrisisManagementStatusResult>>
{
    private readonly ICrisisManagementRepository _repository;
    private readonly ILogger<ResolveCrisisManagementCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ResolveCrisisManagementCommandHandler(ICrisisManagementRepository repository, ILogger<ResolveCrisisManagementCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CrisisManagementStatusResult>> Handle(ResolveCrisisManagementCommand request, CancellationToken cancellationToken)
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

            var resolvedBy = UserId.FromGuid(request.ResolvedByUserId);

            aggregate.Resolve(
                resolvedBy: resolvedBy,
                resolvedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(resolvedBy),
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
            _logger.LogError(ex, "Failed to resolve CrisisManagement crisis {CrisisManagementId}", request.CrisisManagementId);

            return Result<CrisisManagementStatusResult>.Failure(new ApplicationError(
                "SafetyEvents.CrisisManagement.ResolveFailed",
                "Unable to resolve the crisis at this time.",
                Guid.NewGuid()));
        }
    }
}

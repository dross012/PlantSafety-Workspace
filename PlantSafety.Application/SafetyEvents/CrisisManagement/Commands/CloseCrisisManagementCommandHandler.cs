// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.CrisisManagement
//  Layer:          Application
//  File:           CloseCrisisManagementCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for CloseCrisisManagementCommand. Mirrors
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

/// <summary>Handles <see cref="CloseCrisisManagementCommand"/>.</summary>
public sealed class CloseCrisisManagementCommandHandler : IRequestHandler<CloseCrisisManagementCommand, Result<CrisisManagementStatusResult>>
{
    private readonly ICrisisManagementRepository _repository;
    private readonly ILogger<CloseCrisisManagementCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CloseCrisisManagementCommandHandler(ICrisisManagementRepository repository, ILogger<CloseCrisisManagementCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CrisisManagementStatusResult>> Handle(CloseCrisisManagementCommand request, CancellationToken cancellationToken)
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

            var closedBy = UserId.FromGuid(request.ClosedByUserId);

            aggregate.Close(
                closedBy: closedBy,
                closedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(closedBy),
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
            _logger.LogError(ex, "Failed to close CrisisManagement crisis {CrisisManagementId}", request.CrisisManagementId);

            return Result<CrisisManagementStatusResult>.Failure(new ApplicationError(
                "SafetyEvents.CrisisManagement.CloseFailed",
                "Unable to close the crisis at this time.",
                Guid.NewGuid()));
        }
    }
}

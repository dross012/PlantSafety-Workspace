// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.CrisisManagement
//  Layer:          Application
//  File:           RespondToCrisisManagementCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for RespondToCrisisManagementCommand. GetByIdAsync is tenant-scoped,
//                  the NotFound ApplicationError uses ErrorSeverity.NotFound, and
//                  ConcurrencyConflictException is re-thrown before the generic catch -- all from
//                  day one. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.ApprovePermitsCommandHandler.
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

/// <summary>Handles <see cref="RespondToCrisisManagementCommand"/>.</summary>
public sealed class RespondToCrisisManagementCommandHandler : IRequestHandler<RespondToCrisisManagementCommand, Result<CrisisManagementStatusResult>>
{
    private readonly ICrisisManagementRepository _repository;
    private readonly ILogger<RespondToCrisisManagementCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RespondToCrisisManagementCommandHandler(ICrisisManagementRepository repository, ILogger<RespondToCrisisManagementCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CrisisManagementStatusResult>> Handle(RespondToCrisisManagementCommand request, CancellationToken cancellationToken)
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

            var respondedBy = UserId.FromGuid(request.RespondedByUserId);

            aggregate.RespondTo(
                respondedBy: respondedBy,
                respondedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(respondedBy),
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
            _logger.LogError(ex, "Failed to respond to CrisisManagement crisis {CrisisManagementId}", request.CrisisManagementId);

            return Result<CrisisManagementStatusResult>.Failure(new ApplicationError(
                "SafetyEvents.CrisisManagement.RespondToFailed",
                "Unable to record a response to the crisis at this time.",
                Guid.NewGuid()));
        }
    }
}

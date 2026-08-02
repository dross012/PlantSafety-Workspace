// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.CrisisManagement
//  Layer:          Application
//  File:           ArchiveCrisisManagementCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for ArchiveCrisisManagementCommand. Mirrors
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

/// <summary>Handles <see cref="ArchiveCrisisManagementCommand"/>.</summary>
public sealed class ArchiveCrisisManagementCommandHandler : IRequestHandler<ArchiveCrisisManagementCommand, Result<CrisisManagementStatusResult>>
{
    private readonly ICrisisManagementRepository _repository;
    private readonly ILogger<ArchiveCrisisManagementCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ArchiveCrisisManagementCommandHandler(ICrisisManagementRepository repository, ILogger<ArchiveCrisisManagementCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CrisisManagementStatusResult>> Handle(ArchiveCrisisManagementCommand request, CancellationToken cancellationToken)
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

            var archivedBy = UserId.FromGuid(request.ArchivedByUserId);

            aggregate.Archive(
                archivedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(archivedBy),
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
            _logger.LogError(ex, "Failed to archive CrisisManagement crisis {CrisisManagementId}", request.CrisisManagementId);

            return Result<CrisisManagementStatusResult>.Failure(new ApplicationError(
                "SafetyEvents.CrisisManagement.ArchiveFailed",
                "Unable to archive the crisis at this time.",
                Guid.NewGuid()));
        }
    }
}

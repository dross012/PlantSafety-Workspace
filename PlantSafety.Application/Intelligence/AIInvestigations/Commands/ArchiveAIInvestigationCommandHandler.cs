// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.AIInvestigations
//  Layer:          Application
//  File:           ArchiveAIInvestigationCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for ArchiveAIInvestigationCommand.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Intelligence.AIInvestigations.Interfaces;
using PlantSafety.Domain.Intelligence.AIInvestigations.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Intelligence.AIInvestigations.Commands;

/// <summary>Handles <see cref="ArchiveAIInvestigationCommand"/>.</summary>
public sealed class ArchiveAIInvestigationCommandHandler : IRequestHandler<ArchiveAIInvestigationCommand, Result<AIInvestigationStatusResult>>
{
    private readonly IAIInvestigationRepository _repository;
    private readonly ILogger<ArchiveAIInvestigationCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ArchiveAIInvestigationCommandHandler(IAIInvestigationRepository repository, ILogger<ArchiveAIInvestigationCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<AIInvestigationStatusResult>> Handle(ArchiveAIInvestigationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = AIInvestigationId.FromGuid(request.AIInvestigationId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<AIInvestigationStatusResult>.Failure(new ApplicationError(
                    "Intelligence.AIInvestigation.NotFound",
                    "The requested AI investigation could not be found.",
                    request.AIInvestigationId,
                    ErrorSeverity.NotFound));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.Archive(
                archivedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<AIInvestigationStatusResult>.Success(new AIInvestigationStatusResult(
                aggregate.AIInvestigationId.Value,
                aggregate.Status.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to archive AIInvestigation {AIInvestigationId}", request.AIInvestigationId);

            return Result<AIInvestigationStatusResult>.Failure(new ApplicationError(
                "Intelligence.AIInvestigation.ArchiveFailed",
                "Unable to archive the AI investigation at this time.",
                Guid.NewGuid()));
        }
    }
}

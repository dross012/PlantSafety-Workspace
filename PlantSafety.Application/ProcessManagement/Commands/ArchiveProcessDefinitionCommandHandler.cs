// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement
//  Layer:          Application
//  File:           ArchiveProcessDefinitionCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for ArchiveProcessDefinitionCommand. Mirrors
//                  SubmitProcessDefinitionForReviewCommandHandler. Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.ProcessManagement.Repositories;
using PlantSafety.Domain.ProcessManagement.ValueObjects;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.ProcessManagement.Commands;

/// <summary>Handles <see cref="ArchiveProcessDefinitionCommand"/>.</summary>
public sealed class ArchiveProcessDefinitionCommandHandler : IRequestHandler<ArchiveProcessDefinitionCommand, Result<ProcessDefinitionStatusResult>>
{
    private readonly IProcessDefinitionRepository _repository;
    private readonly ILogger<ArchiveProcessDefinitionCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ArchiveProcessDefinitionCommandHandler(IProcessDefinitionRepository repository, ILogger<ArchiveProcessDefinitionCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ProcessDefinitionStatusResult>> Handle(ArchiveProcessDefinitionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = ProcessDefinitionId.FromGuid(request.ProcessDefinitionId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<ProcessDefinitionStatusResult>.Failure(new ApplicationError(
                    "ProcessManagement.ProcessDefinition.NotFound",
                    "The requested process definition could not be found.",
                    request.ProcessDefinitionId,
                    ErrorSeverity.NotFound));
            }

            var actorUserId = UserId.FromGuid(request.PerformedByUserId);

            aggregate.Archive(
                archivedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(actorUserId),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<ProcessDefinitionStatusResult>.Success(new ProcessDefinitionStatusResult(
                aggregate.ProcessDefinitionId.Value, aggregate.Status.ToString(), aggregate.Version));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to archive ProcessDefinition {ProcessDefinitionId}", request.ProcessDefinitionId);

            return Result<ProcessDefinitionStatusResult>.Failure(new ApplicationError(
                "ProcessManagement.ProcessDefinition.ArchiveFailed",
                "Unable to archive the process definition at this time.",
                Guid.NewGuid()));
        }
    }
}

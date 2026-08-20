// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.ModelRegistry
//  Layer:          Application
//  File:           ApproveModelCommandHandler.cs
//  Created:        2026-08-20
//  Author:         PlantSafety Platform Team (Claude Code file-and-folder-agent)
//  SchemaVersion:  2026.1
//  Description:    SAFETY-CRITICAL (Safety Rule 4.5) handler for ApproveModelCommand. Constructs
//                  the domain actor as PerformedByActor.User(...) -- never System()/AiAgent() --
//                  so the ModelRegistryEntry aggregate's human-approver invariant
//                  (ModelRegistryEntryApprovalRequiresHumanActorException) is always satisfied for
//                  this API-driven command path. GetByIdAsync is tenant-scoped, the NotFound
//                  ApplicationError uses ErrorSeverity.NotFound, and ConcurrencyConflictException is
//                  re-thrown before the generic catch.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Intelligence.ModelRegistry.Interfaces;
using PlantSafety.Domain.Intelligence.ModelRegistry.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Intelligence.ModelRegistry.Commands;

/// <summary>SAFETY-CRITICAL: Handles <see cref="ApproveModelCommand"/> (Safety Rule 4.5).</summary>
public sealed class ApproveModelCommandHandler : IRequestHandler<ApproveModelCommand, Result<ModelRegistryEntryStatusResult>>
{
    private readonly IModelRegistryEntryRepository _repository;
    private readonly ILogger<ApproveModelCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ApproveModelCommandHandler(IModelRegistryEntryRepository repository, ILogger<ApproveModelCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ModelRegistryEntryStatusResult>> Handle(ApproveModelCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = ModelRegistryEntryId.FromGuid(request.ModelRegistryEntryId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<ModelRegistryEntryStatusResult>.Failure(new ApplicationError(
                    "Intelligence.ModelRegistry.NotFound",
                    "The requested model registry entry could not be found.",
                    request.ModelRegistryEntryId,
                    ErrorSeverity.NotFound));
            }

            var approvedBy = UserId.FromGuid(request.ApprovedByUserId);

            aggregate.Approve(
                approvedBy: approvedBy,
                actor: PerformedByActor.User(approvedBy),
                approvedAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<ModelRegistryEntryStatusResult>.Success(new ModelRegistryEntryStatusResult(
                aggregate.ModelRegistryEntryId.Value,
                aggregate.Status.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to approve ModelRegistryEntry {ModelRegistryEntryId}", request.ModelRegistryEntryId);

            return Result<ModelRegistryEntryStatusResult>.Failure(new ApplicationError(
                "Intelligence.ModelRegistry.ApproveFailed",
                "Unable to approve the model at this time.",
                Guid.NewGuid()));
        }
    }
}

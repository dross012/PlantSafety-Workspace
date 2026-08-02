// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AiGovernance
//  Layer:          Application
//  File:           ActivateAiGovernanceCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for ActivateAiGovernanceCommand. Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.AiGovernance.Repositories;
using PlantSafety.Domain.AiGovernance.ValueObjects;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.AiGovernance.Commands;

/// <summary>Handles <see cref="ActivateAiGovernanceCommand"/>.</summary>
public sealed class ActivateAiGovernanceCommandHandler : IRequestHandler<ActivateAiGovernanceCommand, Result<AiGovernanceStatusResult>>
{
    private readonly IAiGovernanceRepository _repository;
    private readonly ILogger<ActivateAiGovernanceCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ActivateAiGovernanceCommandHandler(IAiGovernanceRepository repository, ILogger<ActivateAiGovernanceCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<AiGovernanceStatusResult>> Handle(ActivateAiGovernanceCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = AiGovernanceId.FromGuid(request.AiGovernanceId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<AiGovernanceStatusResult>.Failure(new ApplicationError(
                    "AiGovernance.NotFound",
                    "The requested AI governance record could not be found.",
                    request.AiGovernanceId,
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

            return Result<AiGovernanceStatusResult>.Success(new AiGovernanceStatusResult(
                aggregate.AiGovernanceId.Value,
                aggregate.Status.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to activate AiGovernance {AiGovernanceId}", request.AiGovernanceId);

            return Result<AiGovernanceStatusResult>.Failure(new ApplicationError(
                "AiGovernance.ActivateFailed",
                "Unable to activate the AI governance record at this time.",
                Guid.NewGuid()));
        }
    }
}

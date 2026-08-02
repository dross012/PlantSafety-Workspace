// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Governance
//  Layer:          Application
//  File:           CompleteGovernanceCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for CompleteGovernanceCommand.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Governance.Repositories;
using PlantSafety.Domain.Governance.ValueObjects;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Governance.Commands;

/// <summary>Handles <see cref="CompleteGovernanceCommand"/>.</summary>
public sealed class CompleteGovernanceCommandHandler : IRequestHandler<CompleteGovernanceCommand, Result<GovernanceStatusResult>>
{
    private readonly IGovernanceRepository _repository;
    private readonly ILogger<CompleteGovernanceCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CompleteGovernanceCommandHandler(IGovernanceRepository repository, ILogger<CompleteGovernanceCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<GovernanceStatusResult>> Handle(CompleteGovernanceCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = GovernanceId.FromGuid(request.GovernanceId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<GovernanceStatusResult>.Failure(new ApplicationError(
                    "Governance.NotFound",
                    "The requested governance record could not be found.",
                    request.GovernanceId,
                    ErrorSeverity.NotFound));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.Complete(
                actorUserId: performedBy,
                atUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<GovernanceStatusResult>.Success(new GovernanceStatusResult(
                aggregate.GovernanceId.Value,
                aggregate.Status.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to complete Governance {GovernanceId}", request.GovernanceId);

            return Result<GovernanceStatusResult>.Failure(new ApplicationError(
                "Governance.CompleteFailed",
                "Unable to complete the governance record at this time.",
                Guid.NewGuid()));
        }
    }
}

// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Governance
//  Layer:          Application
//  File:           ActivateGovernanceCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for ActivateGovernanceCommand. GetByIdAsync is tenant-scoped, the
//                  NotFound ApplicationError uses ErrorSeverity.NotFound, and
//                  ConcurrencyConflictException is re-thrown before the generic catch. Mirrors
//                  PlantSafety.Application.Compliance.Auditing.Commands.StartAuditingCommandHandler.
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

/// <summary>Handles <see cref="ActivateGovernanceCommand"/>.</summary>
public sealed class ActivateGovernanceCommandHandler : IRequestHandler<ActivateGovernanceCommand, Result<GovernanceStatusResult>>
{
    private readonly IGovernanceRepository _repository;
    private readonly ILogger<ActivateGovernanceCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ActivateGovernanceCommandHandler(IGovernanceRepository repository, ILogger<ActivateGovernanceCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<GovernanceStatusResult>> Handle(ActivateGovernanceCommand request, CancellationToken cancellationToken)
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

            aggregate.Activate(
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
            _logger.LogError(ex, "Failed to activate Governance {GovernanceId}", request.GovernanceId);

            return Result<GovernanceStatusResult>.Failure(new ApplicationError(
                "Governance.ActivateFailed",
                "Unable to activate the governance record at this time.",
                Guid.NewGuid()));
        }
    }
}

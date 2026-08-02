// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Governance
//  Layer:          Application
//  File:           ArchiveGovernanceCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for ArchiveGovernanceCommand.
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

/// <summary>Handles <see cref="ArchiveGovernanceCommand"/>.</summary>
public sealed class ArchiveGovernanceCommandHandler : IRequestHandler<ArchiveGovernanceCommand, Result<GovernanceStatusResult>>
{
    private readonly IGovernanceRepository _repository;
    private readonly ILogger<ArchiveGovernanceCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ArchiveGovernanceCommandHandler(IGovernanceRepository repository, ILogger<ArchiveGovernanceCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<GovernanceStatusResult>> Handle(ArchiveGovernanceCommand request, CancellationToken cancellationToken)
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

            aggregate.Archive(
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
            _logger.LogError(ex, "Failed to archive Governance {GovernanceId}", request.GovernanceId);

            return Result<GovernanceStatusResult>.Failure(new ApplicationError(
                "Governance.ArchiveFailed",
                "Unable to archive the governance record at this time.",
                Guid.NewGuid()));
        }
    }
}

// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Permits
//  Layer:          Application
//  File:           ApprovePermitsCommandHandler.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Handler for ApprovePermitsCommand. GetByIdAsync is tenant-scoped, the NotFound
//                  ApplicationError uses ErrorSeverity.NotFound, and ConcurrencyConflictException
//                  is re-thrown before the generic catch -- all from day one (this aggregate had
//                  no write path to retrofit). Mirrors
//                  PlantSafety.Application.RiskManagement.Commands.ScoreRiskCommandHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Compliance.Permits.Interfaces;
using PlantSafety.Domain.Compliance.Permits.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Compliance.Permits.Commands;

/// <summary>Handles <see cref="ApprovePermitsCommand"/>.</summary>
public sealed class ApprovePermitsCommandHandler : IRequestHandler<ApprovePermitsCommand, Result<PermitsStatusResult>>
{
    private readonly IPermitsRepository _repository;
    private readonly ILogger<ApprovePermitsCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ApprovePermitsCommandHandler(IPermitsRepository repository, ILogger<ApprovePermitsCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PermitsStatusResult>> Handle(ApprovePermitsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = PermitsId.FromGuid(request.PermitsId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<PermitsStatusResult>.Failure(new ApplicationError(
                    "Compliance.Permits.NotFound",
                    "The requested permit could not be found.",
                    request.PermitsId,
                    ErrorSeverity.NotFound));
            }

            var approvedBy = UserId.FromGuid(request.ApprovedByUserId);

            aggregate.Approve(
                approvedByUserId: approvedBy,
                approvedAtUtc: UtcTimestamp.Now(),
                validityHours: request.ValidityHours,
                actor: PerformedByActor.User(approvedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<PermitsStatusResult>.Success(new PermitsStatusResult(
                aggregate.PermitsId.Value,
                aggregate.Status.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            // Let this propagate to ExceptionHandlingMiddleware, which maps it to HTTP 409 --
            // swallowing it here as a generic ApplicationError would misreport a stale-ETag
            // conflict as an ordinary 400 failure.
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to approve Permits {PermitsId}", request.PermitsId);

            return Result<PermitsStatusResult>.Failure(new ApplicationError(
                "Compliance.Permits.ApproveFailed",
                "Unable to approve the permit at this time.",
                Guid.NewGuid()));
        }
    }
}

// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContinuousImprovement.RCA
//  Layer:          Application
//  File:           SubmitForApprovalRCACommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for SubmitForApprovalRCACommand.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.ContinuousImprovement.RCA.Interfaces;
using PlantSafety.Domain.ContinuousImprovement.RCA.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.ContinuousImprovement.RCA.Commands;

/// <summary>Handles <see cref="SubmitForApprovalRCACommand"/>.</summary>
public sealed class SubmitForApprovalRCACommandHandler : IRequestHandler<SubmitForApprovalRCACommand, Result<RCAStatusResult>>
{
    private readonly IRCARepository _repository;
    private readonly ILogger<SubmitForApprovalRCACommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public SubmitForApprovalRCACommandHandler(IRCARepository repository, ILogger<SubmitForApprovalRCACommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<RCAStatusResult>> Handle(SubmitForApprovalRCACommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = RCAId.FromGuid(request.RcaId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<RCAStatusResult>.Failure(new ApplicationError(
                    "ContinuousImprovement.RCA.NotFound",
                    "The requested root cause analysis could not be found.",
                    request.RcaId,
                    ErrorSeverity.NotFound));
            }

            var submittedBy = UserId.FromGuid(request.SubmittedByUserId);

            aggregate.SubmitForApproval(
                submittedBy: submittedBy,
                findings: request.Findings,
                submittedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(submittedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<RCAStatusResult>.Success(new RCAStatusResult(
                aggregate.RcaId.Value,
                aggregate.Status.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to submit RCA {RcaId} for approval", request.RcaId);

            return Result<RCAStatusResult>.Failure(new ApplicationError(
                "ContinuousImprovement.RCA.SubmitForApprovalFailed",
                "Unable to submit the root cause analysis for approval at this time.",
                Guid.NewGuid()));
        }
    }
}

// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.ManagementOfChange
//  Layer:          Application
//  File:           ApproveManagementOfChangeCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for ApproveManagementOfChangeCommand. GetByIdAsync is tenant-scoped, the
//                  NotFound ApplicationError uses ErrorSeverity.NotFound, and
//                  ConcurrencyConflictException is re-thrown before the generic catch -- all from
//                  day one. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.ApprovePermitsCommandHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Compliance.ManagementOfChange.Interfaces;
using PlantSafety.Domain.Compliance.ManagementOfChange.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Compliance.ManagementOfChange.Commands;

/// <summary>Handles <see cref="ApproveManagementOfChangeCommand"/>.</summary>
public sealed class ApproveManagementOfChangeCommandHandler : IRequestHandler<ApproveManagementOfChangeCommand, Result<ManagementOfChangeStatusResult>>
{
    private readonly IManagementOfChangeRepository _repository;
    private readonly ILogger<ApproveManagementOfChangeCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ApproveManagementOfChangeCommandHandler(IManagementOfChangeRepository repository, ILogger<ApproveManagementOfChangeCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ManagementOfChangeStatusResult>> Handle(ApproveManagementOfChangeCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = ManagementOfChangeId.FromGuid(request.ManagementOfChangeId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<ManagementOfChangeStatusResult>.Failure(new ApplicationError(
                    "Compliance.ManagementOfChange.NotFound",
                    "The requested change record could not be found.",
                    request.ManagementOfChangeId,
                    ErrorSeverity.NotFound));
            }

            var approvedBy = UserId.FromGuid(request.ApprovedByUserId);

            aggregate.Approve(
                approvedBy: approvedBy,
                approvedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(approvedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<ManagementOfChangeStatusResult>.Success(new ManagementOfChangeStatusResult(
                aggregate.ManagementOfChangeId.Value,
                aggregate.Status.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to approve ManagementOfChange {ManagementOfChangeId}", request.ManagementOfChangeId);

            return Result<ManagementOfChangeStatusResult>.Failure(new ApplicationError(
                "Compliance.ManagementOfChange.ApproveFailed",
                "Unable to approve the change request at this time.",
                Guid.NewGuid()));
        }
    }
}

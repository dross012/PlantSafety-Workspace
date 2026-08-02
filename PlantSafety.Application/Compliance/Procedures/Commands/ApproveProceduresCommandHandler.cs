// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Procedures
//  Layer:          Application
//  File:           ApproveProceduresCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for ApproveProceduresCommand. GetByIdAsync is tenant-scoped, the
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
using PlantSafety.Domain.Compliance.Procedures.Interfaces;
using PlantSafety.Domain.Compliance.Procedures.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Compliance.Procedures.Commands;

/// <summary>Handles <see cref="ApproveProceduresCommand"/>.</summary>
public sealed class ApproveProceduresCommandHandler : IRequestHandler<ApproveProceduresCommand, Result<ProceduresStatusResult>>
{
    private readonly IProceduresRepository _repository;
    private readonly ILogger<ApproveProceduresCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ApproveProceduresCommandHandler(IProceduresRepository repository, ILogger<ApproveProceduresCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ProceduresStatusResult>> Handle(ApproveProceduresCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = ProceduresId.FromGuid(request.ProceduresId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<ProceduresStatusResult>.Failure(new ApplicationError(
                    "Compliance.Procedures.NotFound",
                    "The requested procedure could not be found.",
                    request.ProceduresId,
                    ErrorSeverity.NotFound));
            }

            var approvedBy = UserId.FromGuid(request.ApprovedByUserId);

            aggregate.Approve(
                approvedBy: approvedBy,
                effectiveAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(approvedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<ProceduresStatusResult>.Success(new ProceduresStatusResult(
                aggregate.ProceduresId.Value,
                aggregate.Status.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to approve Procedures {ProceduresId}", request.ProceduresId);

            return Result<ProceduresStatusResult>.Failure(new ApplicationError(
                "Compliance.Procedures.ApproveFailed",
                "Unable to approve the procedure at this time.",
                Guid.NewGuid()));
        }
    }
}

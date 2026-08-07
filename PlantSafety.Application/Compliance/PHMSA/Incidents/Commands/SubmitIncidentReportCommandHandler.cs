// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Incidents
//  Layer:          Application
//  File:           SubmitIncidentReportCommandHandler.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Handler for SubmitIncidentReportCommand. GetByIdAsync is tenant-scoped, the
//                  NotFound ApplicationError uses ErrorSeverity.NotFound, and
//                  ConcurrencyConflictException is re-thrown before the generic catch. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.ApprovePermitsCommandHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Compliance.PHMSA.Incidents.Interfaces;
using PlantSafety.Domain.Compliance.PHMSA.Incidents.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Compliance.PHMSA.Incidents.Commands;

/// <summary>Handles <see cref="SubmitIncidentReportCommand"/>.</summary>
public sealed class SubmitIncidentReportCommandHandler : IRequestHandler<SubmitIncidentReportCommand, Result<IncidentReportStatusResult>>
{
    private readonly IIncidentReportRepository _repository;
    private readonly ILogger<SubmitIncidentReportCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public SubmitIncidentReportCommandHandler(IIncidentReportRepository repository, ILogger<SubmitIncidentReportCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<IncidentReportStatusResult>> Handle(SubmitIncidentReportCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = IncidentReportId.FromGuid(request.IncidentReportId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<IncidentReportStatusResult>.Failure(new ApplicationError(
                    "Compliance.PHMSA.Incidents.NotFound",
                    "The requested incident report could not be found.",
                    request.IncidentReportId,
                    ErrorSeverity.NotFound));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.Submit(
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(performedBy));

            await _repository.UpdateAsync(aggregate, request.TenantId, request.ETag, cancellationToken);

            return Result<IncidentReportStatusResult>.Success(new IncidentReportStatusResult(
                aggregate.Id.Value,
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
            _logger.LogError(ex, "Failed to submit IncidentReport {IncidentReportId}", request.IncidentReportId);

            return Result<IncidentReportStatusResult>.Failure(new ApplicationError(
                "Compliance.PHMSA.Incidents.SubmitFailed",
                "Unable to submit the incident report at this time.",
                Guid.NewGuid()));
        }
    }
}

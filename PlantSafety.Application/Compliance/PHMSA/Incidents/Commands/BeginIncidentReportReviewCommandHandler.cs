// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Incidents
//  Layer:          Application
//  File:           BeginIncidentReportReviewCommandHandler.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Handler for BeginIncidentReportReviewCommand. Mirrors
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

/// <summary>Handles <see cref="BeginIncidentReportReviewCommand"/>.</summary>
public sealed class BeginIncidentReportReviewCommandHandler : IRequestHandler<BeginIncidentReportReviewCommand, Result<IncidentReportStatusResult>>
{
    private readonly IIncidentReportRepository _repository;
    private readonly ILogger<BeginIncidentReportReviewCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public BeginIncidentReportReviewCommandHandler(IIncidentReportRepository repository, ILogger<BeginIncidentReportReviewCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<IncidentReportStatusResult>> Handle(BeginIncidentReportReviewCommand request, CancellationToken cancellationToken)
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

            aggregate.BeginReview(
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
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to begin review for IncidentReport {IncidentReportId}", request.IncidentReportId);

            return Result<IncidentReportStatusResult>.Failure(new ApplicationError(
                "Compliance.PHMSA.Incidents.BeginReviewFailed",
                "Unable to begin review of the incident report at this time.",
                Guid.NewGuid()));
        }
    }
}

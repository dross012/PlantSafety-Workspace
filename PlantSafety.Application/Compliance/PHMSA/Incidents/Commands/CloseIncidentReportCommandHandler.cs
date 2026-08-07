// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Incidents
//  Layer:          Application
//  File:           CloseIncidentReportCommandHandler.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Handler for CloseIncidentReportCommand. Mirrors
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

/// <summary>Handles <see cref="CloseIncidentReportCommand"/>.</summary>
public sealed class CloseIncidentReportCommandHandler : IRequestHandler<CloseIncidentReportCommand, Result<IncidentReportStatusResult>>
{
    private readonly IIncidentReportRepository _repository;
    private readonly ILogger<CloseIncidentReportCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CloseIncidentReportCommandHandler(IIncidentReportRepository repository, ILogger<CloseIncidentReportCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<IncidentReportStatusResult>> Handle(CloseIncidentReportCommand request, CancellationToken cancellationToken)
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

            aggregate.Close(
                resolutionNotes: request.ResolutionNotes,
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
            _logger.LogError(ex, "Failed to close IncidentReport {IncidentReportId}", request.IncidentReportId);

            return Result<IncidentReportStatusResult>.Failure(new ApplicationError(
                "Compliance.PHMSA.Incidents.CloseFailed",
                "Unable to close the incident report at this time.",
                Guid.NewGuid()));
        }
    }
}

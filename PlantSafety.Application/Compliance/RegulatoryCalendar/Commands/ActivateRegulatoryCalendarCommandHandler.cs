// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.RegulatoryCalendar
//  Layer:          Application
//  File:           ActivateRegulatoryCalendarCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for ActivateRegulatoryCalendarCommand. GetByIdAsync is tenant-scoped,
//                  the NotFound ApplicationError uses ErrorSeverity.NotFound, and
//                  ConcurrencyConflictException is re-thrown before the generic catch -- all from
//                  day one. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.ApprovePermitsCommandHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Compliance.RegulatoryCalendar.Interfaces;
using PlantSafety.Domain.Compliance.RegulatoryCalendar.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Compliance.RegulatoryCalendar.Commands;

/// <summary>Handles <see cref="ActivateRegulatoryCalendarCommand"/>.</summary>
public sealed class ActivateRegulatoryCalendarCommandHandler : IRequestHandler<ActivateRegulatoryCalendarCommand, Result<RegulatoryCalendarStatusResult>>
{
    private readonly IRegulatoryCalendarRepository _repository;
    private readonly ILogger<ActivateRegulatoryCalendarCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ActivateRegulatoryCalendarCommandHandler(IRegulatoryCalendarRepository repository, ILogger<ActivateRegulatoryCalendarCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<RegulatoryCalendarStatusResult>> Handle(ActivateRegulatoryCalendarCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = RegulatoryCalendarId.FromGuid(request.RegulatoryCalendarId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<RegulatoryCalendarStatusResult>.Failure(new ApplicationError(
                    "Compliance.RegulatoryCalendar.NotFound",
                    "The requested regulatory calendar record could not be found.",
                    request.RegulatoryCalendarId,
                    ErrorSeverity.NotFound));
            }

            var activatedBy = UserId.FromGuid(request.ActivatedByUserId);

            aggregate.Activate(
                activatedBy: activatedBy,
                activatedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(activatedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<RegulatoryCalendarStatusResult>.Success(new RegulatoryCalendarStatusResult(
                aggregate.RegulatoryCalendarId.Value,
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
            _logger.LogError(ex, "Failed to activate RegulatoryCalendar {RegulatoryCalendarId}", request.RegulatoryCalendarId);

            return Result<RegulatoryCalendarStatusResult>.Failure(new ApplicationError(
                "Compliance.RegulatoryCalendar.ActivateFailed",
                "Unable to activate the regulatory calendar record at this time.",
                Guid.NewGuid()));
        }
    }
}

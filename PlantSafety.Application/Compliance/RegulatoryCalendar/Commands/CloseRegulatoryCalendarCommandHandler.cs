// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.RegulatoryCalendar
//  Layer:          Application
//  File:           CloseRegulatoryCalendarCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for CloseRegulatoryCalendarCommand.
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

/// <summary>Handles <see cref="CloseRegulatoryCalendarCommand"/>.</summary>
public sealed class CloseRegulatoryCalendarCommandHandler : IRequestHandler<CloseRegulatoryCalendarCommand, Result<RegulatoryCalendarStatusResult>>
{
    private readonly IRegulatoryCalendarRepository _repository;
    private readonly ILogger<CloseRegulatoryCalendarCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CloseRegulatoryCalendarCommandHandler(IRegulatoryCalendarRepository repository, ILogger<CloseRegulatoryCalendarCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<RegulatoryCalendarStatusResult>> Handle(CloseRegulatoryCalendarCommand request, CancellationToken cancellationToken)
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

            var closedBy = UserId.FromGuid(request.ClosedByUserId);

            aggregate.Close(
                closedBy: closedBy,
                closedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(closedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<RegulatoryCalendarStatusResult>.Success(new RegulatoryCalendarStatusResult(
                aggregate.RegulatoryCalendarId.Value,
                aggregate.Status.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to close RegulatoryCalendar {RegulatoryCalendarId}", request.RegulatoryCalendarId);

            return Result<RegulatoryCalendarStatusResult>.Failure(new ApplicationError(
                "Compliance.RegulatoryCalendar.CloseFailed",
                "Unable to close the regulatory calendar record at this time.",
                Guid.NewGuid()));
        }
    }
}

// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.RegulatoryCalendar
//  Layer:          Application
//  File:           ArchiveRegulatoryCalendarCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for ArchiveRegulatoryCalendarCommand.
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

/// <summary>Handles <see cref="ArchiveRegulatoryCalendarCommand"/>.</summary>
public sealed class ArchiveRegulatoryCalendarCommandHandler : IRequestHandler<ArchiveRegulatoryCalendarCommand, Result<RegulatoryCalendarStatusResult>>
{
    private readonly IRegulatoryCalendarRepository _repository;
    private readonly ILogger<ArchiveRegulatoryCalendarCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ArchiveRegulatoryCalendarCommandHandler(IRegulatoryCalendarRepository repository, ILogger<ArchiveRegulatoryCalendarCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<RegulatoryCalendarStatusResult>> Handle(ArchiveRegulatoryCalendarCommand request, CancellationToken cancellationToken)
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

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.Archive(
                archivedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
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
            _logger.LogError(ex, "Failed to archive RegulatoryCalendar {RegulatoryCalendarId}", request.RegulatoryCalendarId);

            return Result<RegulatoryCalendarStatusResult>.Failure(new ApplicationError(
                "Compliance.RegulatoryCalendar.ArchiveFailed",
                "Unable to archive the regulatory calendar record at this time.",
                Guid.NewGuid()));
        }
    }
}

// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Dashboard
//  Layer:          Application
//  File:           ArchiveDashboardCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for ArchiveDashboardCommand. Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Analytics.Dashboard.Interfaces;
using PlantSafety.Domain.Analytics.Dashboard.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Analytics.Dashboard.Commands;

/// <summary>Handles <see cref="ArchiveDashboardCommand"/>.</summary>
public sealed class ArchiveDashboardCommandHandler : IRequestHandler<ArchiveDashboardCommand, Result<DashboardStatusResult>>
{
    private readonly IDashboardRepository _repository;
    private readonly ILogger<ArchiveDashboardCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ArchiveDashboardCommandHandler(IDashboardRepository repository, ILogger<ArchiveDashboardCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<DashboardStatusResult>> Handle(ArchiveDashboardCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = DashboardId.FromGuid(request.DashboardId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<DashboardStatusResult>.Failure(new ApplicationError(
                    "Analytics.Dashboard.NotFound",
                    "The requested dashboard could not be found.",
                    request.DashboardId,
                    ErrorSeverity.NotFound));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.Archive(
                archivedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<DashboardStatusResult>.Success(new DashboardStatusResult(
                aggregate.DashboardId.Value,
                aggregate.Status.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to archive Dashboard {DashboardId}", request.DashboardId);

            return Result<DashboardStatusResult>.Failure(new ApplicationError(
                "Analytics.Dashboard.ArchiveFailed",
                "Unable to archive the dashboard at this time.",
                Guid.NewGuid()));
        }
    }
}

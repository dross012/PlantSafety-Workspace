// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Dashboard
//  Layer:          Application
//  File:           RefreshDashboardDataCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for RefreshDashboardDataCommand. Additive-only evolution.
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

/// <summary>Handles <see cref="RefreshDashboardDataCommand"/>.</summary>
public sealed class RefreshDashboardDataCommandHandler : IRequestHandler<RefreshDashboardDataCommand, Result<DashboardStatusResult>>
{
    private readonly IDashboardRepository _repository;
    private readonly ILogger<RefreshDashboardDataCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RefreshDashboardDataCommandHandler(IDashboardRepository repository, ILogger<RefreshDashboardDataCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<DashboardStatusResult>> Handle(RefreshDashboardDataCommand request, CancellationToken cancellationToken)
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

            aggregate.RefreshData(
                refreshedAtUtc: UtcTimestamp.Now(),
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
            _logger.LogError(ex, "Failed to refresh Dashboard {DashboardId}", request.DashboardId);

            return Result<DashboardStatusResult>.Failure(new ApplicationError(
                "Analytics.Dashboard.RefreshFailed",
                "Unable to refresh the dashboard at this time.",
                Guid.NewGuid()));
        }
    }
}

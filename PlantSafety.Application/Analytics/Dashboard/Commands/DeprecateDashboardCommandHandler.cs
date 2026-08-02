// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Dashboard
//  Layer:          Application
//  File:           DeprecateDashboardCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for DeprecateDashboardCommand. Additive-only evolution.
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

/// <summary>Handles <see cref="DeprecateDashboardCommand"/>.</summary>
public sealed class DeprecateDashboardCommandHandler : IRequestHandler<DeprecateDashboardCommand, Result<DashboardStatusResult>>
{
    private readonly IDashboardRepository _repository;
    private readonly ILogger<DeprecateDashboardCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public DeprecateDashboardCommandHandler(IDashboardRepository repository, ILogger<DeprecateDashboardCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<DashboardStatusResult>> Handle(DeprecateDashboardCommand request, CancellationToken cancellationToken)
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

            var deprecatedBy = UserId.FromGuid(request.DeprecatedByUserId);

            aggregate.Deprecate(
                deprecatedBy: deprecatedBy,
                deprecatedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(deprecatedBy),
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
            _logger.LogError(ex, "Failed to deprecate Dashboard {DashboardId}", request.DashboardId);

            return Result<DashboardStatusResult>.Failure(new ApplicationError(
                "Analytics.Dashboard.DeprecateFailed",
                "Unable to deprecate the dashboard at this time.",
                Guid.NewGuid()));
        }
    }
}

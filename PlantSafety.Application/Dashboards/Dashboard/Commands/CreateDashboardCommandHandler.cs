// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for CreateDashboardCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Dashboards.Dashboard.Repositories;
using PlantSafety.Domain.Dashboards.Dashboard.Enums;
using PlantSafety.Domain.Dashboards.Dashboard.ValueObjects;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;

namespace PlantSafety.Application.Dashboards.Dashboard.Commands;

/// <summary>Handles <see cref="CreateDashboardCommand"/>.</summary>
public sealed class CreateDashboardCommandHandler : IRequestHandler<CreateDashboardCommand, Result<CreateDashboardResult>>
{
    private readonly IDashboardRepository _repository;
    private readonly ILogger<CreateDashboardCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CreateDashboardCommandHandler(IDashboardRepository repository, ILogger<CreateDashboardCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CreateDashboardResult>> Handle(CreateDashboardCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (!DashboardCategory.TryParse(request.CategoryCode, out var category) || category is null)
            {
                return Result<CreateDashboardResult>.Failure(new ApplicationError(
                    "Dashboards.Dashboard.InvalidCategory", $"Unknown dashboard category code '{request.CategoryCode}'.", request.CorrelationId, ErrorSeverity.Validation));
            }

            var filters = request.DefaultFilters?.Select(kv => DashboardFilter.Create(kv.Key, kv.Value));

            var aggregate = PlantSafety.Domain.Dashboards.Dashboard.Aggregates.Dashboard.Create(
                UtcTimestamp.From(request.RequestedAtUtc),
                new CorrelationId(request.CorrelationId),
                request.CausationId.HasValue ? new CausationId(request.CausationId.Value) : CausationId.New(),
                PerformedByActor.User(UserId.FromGuid(request.CreatedByUserId)),
                request.TenantId,
                request.SiteId,
                request.Name,
                request.Description,
                category,
                UserId.FromGuid(request.CreatedByUserId),
                filters);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<CreateDashboardResult>.Success(new CreateDashboardResult(aggregate.DashboardId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Dashboard for Tenant {TenantId}", request.TenantId);
            return Result<CreateDashboardResult>.Failure(new ApplicationError(
                "Dashboards.Dashboard.CreateFailed", "Unable to create the dashboard at this time.", request.CorrelationId));
        }
    }
}

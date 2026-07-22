// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for CreateWidgetCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Dashboards.Widget.Repositories;
using PlantSafety.Domain.Dashboards.Widget.Enums;
using PlantSafety.Domain.Dashboards.Widget.ValueObjects;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;

namespace PlantSafety.Application.Dashboards.Widget.Commands;

/// <summary>Handles <see cref="CreateWidgetCommand"/>.</summary>
public sealed class CreateWidgetCommandHandler : IRequestHandler<CreateWidgetCommand, Result<CreateWidgetResult>>
{
    private readonly IWidgetRepository _repository;
    private readonly ILogger<CreateWidgetCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CreateWidgetCommandHandler(IWidgetRepository repository, ILogger<CreateWidgetCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CreateWidgetResult>> Handle(CreateWidgetCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (!WidgetType.TryParse(request.TypeCode, out var type) || type is null)
            {
                return Result<CreateWidgetResult>.Failure(new ApplicationError(
                    "Dashboards.Widget.InvalidType", $"Unknown widget type code '{request.TypeCode}'.", request.CorrelationId, ErrorSeverity.Validation));
            }

            var dataSource = WidgetDataSource.Create(request.SourceModule, request.QueryOrEndpoint);

            var refreshInterval = request.RefreshIntervalSeconds.HasValue
                ? WidgetRefreshInterval.FromSeconds(request.RefreshIntervalSeconds.Value)
                : (WidgetRefreshInterval?)null;

            var parameters = request.Parameters?.Select(kv => WidgetParameter.Create(kv.Key, kv.Value));

            var aggregate = PlantSafety.Domain.Dashboards.Widget.Aggregates.Widget.Create(
                UtcTimestamp.From(request.RequestedAtUtc),
                new CorrelationId(request.CorrelationId),
                request.CausationId.HasValue ? new CausationId(request.CausationId.Value) : CausationId.New(),
                PerformedByActor.User(UserId.FromGuid(request.CreatedByUserId)),
                request.TenantId,
                request.Name,
                type,
                dataSource,
                UserId.FromGuid(request.CreatedByUserId),
                refreshInterval,
                parameters,
                request.PermittedRoles);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<CreateWidgetResult>.Success(new CreateWidgetResult(aggregate.WidgetId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Widget for Tenant {TenantId}", request.TenantId);
            return Result<CreateWidgetResult>.Failure(new ApplicationError(
                "Dashboards.Widget.CreateFailed", "Unable to create the widget at this time.", request.CorrelationId));
        }
    }
}

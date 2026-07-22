// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for ConfigureWidgetCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Dashboards.Widget.Repositories;
using PlantSafety.Domain.Dashboards.Widget.ValueObjects;
using PlantSafety.Domain.Dashboards.Widget.ValueObjects.Ids;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;

namespace PlantSafety.Application.Dashboards.Widget.Commands;

/// <summary>Handles <see cref="ConfigureWidgetCommand"/>.</summary>
public sealed class ConfigureWidgetCommandHandler : IRequestHandler<ConfigureWidgetCommand, Result<ConfigureWidgetResult>>
{
    private readonly IWidgetRepository _repository;
    private readonly ILogger<ConfigureWidgetCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ConfigureWidgetCommandHandler(IWidgetRepository repository, ILogger<ConfigureWidgetCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ConfigureWidgetResult>> Handle(ConfigureWidgetCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var aggregate = await _repository.GetByIdAsync(new WidgetId(request.WidgetId), cancellationToken);
            if (aggregate is null)
            {
                return Result<ConfigureWidgetResult>.Failure(new ApplicationError(
                    "Dashboards.Widget.NotFound", $"Widget '{request.WidgetId}' was not found.", request.CorrelationId, ErrorSeverity.NotFound));
            }

            WidgetDataSource? dataSource = !string.IsNullOrWhiteSpace(request.SourceModule) && !string.IsNullOrWhiteSpace(request.QueryOrEndpoint)
                ? WidgetDataSource.Create(request.SourceModule, request.QueryOrEndpoint)
                : null;

            WidgetRefreshInterval? refreshInterval = request.RefreshIntervalSeconds.HasValue
                ? WidgetRefreshInterval.FromSeconds(request.RefreshIntervalSeconds.Value)
                : null;

            var parameters = request.Parameters?.Select(kv => WidgetParameter.Create(kv.Key, kv.Value));

            aggregate.Configure(
                dataSource,
                refreshInterval,
                parameters,
                UtcTimestamp.From(request.RequestedAtUtc),
                PerformedByActor.User(UserId.FromGuid(request.ConfiguredByUserId)),
                new CorrelationId(request.CorrelationId),
                request.CausationId.HasValue ? new CausationId(request.CausationId.Value) : CausationId.New());

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<ConfigureWidgetResult>.Success(new ConfigureWidgetResult(aggregate.WidgetId.Value));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to configure Widget {WidgetId}", request.WidgetId);
            return Result<ConfigureWidgetResult>.Failure(new ApplicationError(
                "Dashboards.Widget.ConfigureFailed", "Unable to configure the widget at this time.", request.CorrelationId));
        }
    }
}

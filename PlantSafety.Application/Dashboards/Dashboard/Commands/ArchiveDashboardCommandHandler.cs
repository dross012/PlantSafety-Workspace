// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for ArchiveDashboardCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Dashboards.Dashboard.Repositories;
using PlantSafety.Domain.Dashboards.Dashboard.ValueObjects.Ids;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;

namespace PlantSafety.Application.Dashboards.Dashboard.Commands;

/// <summary>Handles <see cref="ArchiveDashboardCommand"/>.</summary>
public sealed class ArchiveDashboardCommandHandler : IRequestHandler<ArchiveDashboardCommand, Result<ArchiveDashboardResult>>
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
    public async Task<Result<ArchiveDashboardResult>> Handle(ArchiveDashboardCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var aggregate = await _repository.GetByIdAsync(new DashboardId(request.DashboardId), cancellationToken);
            if (aggregate is null)
            {
                return Result<ArchiveDashboardResult>.Failure(new ApplicationError(
                    "Dashboards.Dashboard.NotFound", $"Dashboard '{request.DashboardId}' was not found.", request.CorrelationId, ErrorSeverity.NotFound));
            }

            aggregate.Archive(
                UtcTimestamp.From(request.RequestedAtUtc),
                PerformedByActor.User(UserId.FromGuid(request.ArchivedByUserId)),
                new CorrelationId(request.CorrelationId),
                request.CausationId.HasValue ? new CausationId(request.CausationId.Value) : CausationId.New());

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<ArchiveDashboardResult>.Success(new ArchiveDashboardResult(aggregate.DashboardId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to archive Dashboard {DashboardId}", request.DashboardId);
            return Result<ArchiveDashboardResult>.Failure(new ApplicationError(
                "Dashboards.Dashboard.ArchiveFailed", "Unable to archive the dashboard at this time.", request.CorrelationId));
        }
    }
}

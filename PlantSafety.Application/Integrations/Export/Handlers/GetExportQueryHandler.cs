// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-12
// FILE DESCRIPTION: Handler for GetExportQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Integrations.Export.Queries;
using PlantSafety.Application.Integrations.Export.Repositories;

namespace PlantSafety.Application.Integrations.Export.Handlers;

/// <summary>Handles <see cref="GetExportQuery"/>.</summary>
public sealed class GetExportQueryHandler : IRequestHandler<GetExportQuery, Result<ExportQueryResult>>
{
    private readonly IExportRepository _repository;
    private readonly ILogger<GetExportQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetExportQueryHandler(IExportRepository repository, ILogger<GetExportQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ExportQueryResult>> Handle(GetExportQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetExportRunsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<ExportQueryResult>.Success(new ExportQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Integrations.Export data for Tenant {TenantId}", request.TenantId);

            return Result<ExportQueryResult>.Failure(new ApplicationError(
                "Integrations.Export.RetrievalFailed",
                "Unable to retrieve export run data at this time.",
                Guid.NewGuid()));
        }
    }
}

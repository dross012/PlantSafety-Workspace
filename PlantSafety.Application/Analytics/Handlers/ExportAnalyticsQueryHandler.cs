// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-13
// FILE DESCRIPTION: Handler for ExportAnalyticsQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Analytics.Queries;
using PlantSafety.Application.Analytics.Repositories;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Analytics.Handlers;

/// <summary>Handles <see cref="ExportAnalyticsQuery"/>.</summary>
public sealed class ExportAnalyticsQueryHandler : IRequestHandler<ExportAnalyticsQuery, Result<ExportAnalyticsResult>>
{
    private readonly IExportAnalyticsRepository _repository;
    private readonly ILogger<ExportAnalyticsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ExportAnalyticsQueryHandler(IExportAnalyticsRepository repository, ILogger<ExportAnalyticsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ExportAnalyticsResult>> Handle(ExportAnalyticsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _repository.GenerateExportAsync(
                request.TenantId, request.SiteId, request.Format, request.From, request.To, request.Status, request.MaxRecords, cancellationToken);

            return Result<ExportAnalyticsResult>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate Analytics export for Tenant {TenantId}", request.TenantId);

            return Result<ExportAnalyticsResult>.Failure(new ApplicationError(
                "Analytics.Export.GenerationFailed",
                "Unable to generate the export at this time.",
                Guid.NewGuid()));
        }
    }
}

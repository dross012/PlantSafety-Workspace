// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-13
// FILE DESCRIPTION: Handler for ExportOperationalQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Operational.Queries;
using PlantSafety.Application.Operational.Repositories;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Operational.Handlers;

/// <summary>Handles <see cref="ExportOperationalQuery"/>.</summary>
public sealed class ExportOperationalQueryHandler : IRequestHandler<ExportOperationalQuery, Result<ExportOperationalResult>>
{
    private readonly IExportOperationalRepository _repository;
    private readonly ILogger<ExportOperationalQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ExportOperationalQueryHandler(IExportOperationalRepository repository, ILogger<ExportOperationalQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ExportOperationalResult>> Handle(ExportOperationalQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _repository.GenerateExportAsync(
                request.TenantId, request.SiteId, request.Format, request.From, request.To, request.Status, request.MaxRecords, cancellationToken);

            return Result<ExportOperationalResult>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate Operational export for Tenant {TenantId}", request.TenantId);

            return Result<ExportOperationalResult>.Failure(new ApplicationError(
                "Operational.Export.GenerationFailed",
                "Unable to generate the export at this time.",
                Guid.NewGuid()));
        }
    }
}

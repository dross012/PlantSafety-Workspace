// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-13
// FILE DESCRIPTION: Handler for ExportComplianceQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Compliance.Queries;
using PlantSafety.Application.Compliance.Repositories;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Compliance.Handlers;

/// <summary>Handles <see cref="ExportComplianceQuery"/>.</summary>
public sealed class ExportComplianceQueryHandler : IRequestHandler<ExportComplianceQuery, Result<ExportComplianceResult>>
{
    private readonly IExportComplianceRepository _repository;
    private readonly ILogger<ExportComplianceQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ExportComplianceQueryHandler(IExportComplianceRepository repository, ILogger<ExportComplianceQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ExportComplianceResult>> Handle(ExportComplianceQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _repository.GenerateExportAsync(
                request.TenantId, request.SiteId, request.Format, request.From, request.To, request.Status, request.ResourceType, request.MaxRecords, cancellationToken);

            return Result<ExportComplianceResult>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate Compliance export for Tenant {TenantId}", request.TenantId);

            return Result<ExportComplianceResult>.Failure(new ApplicationError(
                "Compliance.Export.GenerationFailed",
                "Unable to generate the export at this time.",
                Guid.NewGuid()));
        }
    }
}

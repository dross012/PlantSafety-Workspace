// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-13
// FILE DESCRIPTION: Handler for ExportContinuousImprovementQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.ContinuousImprovement.Queries;
using PlantSafety.Application.ContinuousImprovement.Repositories;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.ContinuousImprovement.Handlers;

/// <summary>Handles <see cref="ExportContinuousImprovementQuery"/>.</summary>
public sealed class ExportContinuousImprovementQueryHandler : IRequestHandler<ExportContinuousImprovementQuery, Result<ExportContinuousImprovementResult>>
{
    private readonly IExportContinuousImprovementRepository _repository;
    private readonly ILogger<ExportContinuousImprovementQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ExportContinuousImprovementQueryHandler(IExportContinuousImprovementRepository repository, ILogger<ExportContinuousImprovementQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ExportContinuousImprovementResult>> Handle(ExportContinuousImprovementQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _repository.GenerateExportAsync(
                request.TenantId, request.SiteId, request.Format, request.From, request.To, request.Status, request.MaxRecords, cancellationToken);

            return Result<ExportContinuousImprovementResult>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate ContinuousImprovement export for Tenant {TenantId}", request.TenantId);

            return Result<ExportContinuousImprovementResult>.Failure(new ApplicationError(
                "ContinuousImprovement.Export.GenerationFailed",
                "Unable to generate the export at this time.",
                Guid.NewGuid()));
        }
    }
}

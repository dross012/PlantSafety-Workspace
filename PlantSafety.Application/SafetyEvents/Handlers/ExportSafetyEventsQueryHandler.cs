// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-13
// FILE DESCRIPTION: Handler for ExportSafetyEventsQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.SafetyEvents.Queries;
using PlantSafety.Application.SafetyEvents.Repositories;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.SafetyEvents.Handlers;

/// <summary>Handles <see cref="ExportSafetyEventsQuery"/>.</summary>
public sealed class ExportSafetyEventsQueryHandler : IRequestHandler<ExportSafetyEventsQuery, Result<ExportSafetyEventsResult>>
{
    private readonly IExportSafetyEventsRepository _repository;
    private readonly ILogger<ExportSafetyEventsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ExportSafetyEventsQueryHandler(IExportSafetyEventsRepository repository, ILogger<ExportSafetyEventsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ExportSafetyEventsResult>> Handle(ExportSafetyEventsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _repository.GenerateExportAsync(
                request.TenantId, request.SiteId, request.Format, request.From, request.To, request.Status, request.Severity, request.MaxRecords, cancellationToken);

            return Result<ExportSafetyEventsResult>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate SafetyEvents export for Tenant {TenantId}", request.TenantId);

            return Result<ExportSafetyEventsResult>.Failure(new ApplicationError(
                "SafetyEvents.Export.GenerationFailed",
                "Unable to generate the export at this time.",
                Guid.NewGuid()));
        }
    }
}

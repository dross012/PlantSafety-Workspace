// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-12
// FILE DESCRIPTION: Handler for GetImportQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Integrations.Import.Queries;
using PlantSafety.Application.Integrations.Import.Repositories;

namespace PlantSafety.Application.Integrations.Import.Handlers;

/// <summary>Handles <see cref="GetImportQuery"/>.</summary>
public sealed class GetImportQueryHandler : IRequestHandler<GetImportQuery, Result<ImportQueryResult>>
{
    private readonly IImportRepository _repository;
    private readonly ILogger<GetImportQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetImportQueryHandler(IImportRepository repository, ILogger<GetImportQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ImportQueryResult>> Handle(GetImportQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetImportRunsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<ImportQueryResult>.Success(new ImportQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Integrations.Import data for Tenant {TenantId}", request.TenantId);

            return Result<ImportQueryResult>.Failure(new ApplicationError(
                "Integrations.Import.RetrievalFailed",
                "Unable to retrieve import run data at this time.",
                Guid.NewGuid()));
        }
    }
}

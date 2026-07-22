// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetErgonomicsAndMaterialHandlingQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.EmployeeSafety.ErgonomicsAndMaterialHandling.Queries;
using PlantSafety.Application.EmployeeSafety.ErgonomicsAndMaterialHandling.Repositories;

namespace PlantSafety.Application.EmployeeSafety.ErgonomicsAndMaterialHandling.Handlers;

/// <summary>Handles <see cref="GetErgonomicsAndMaterialHandlingQuery"/>.</summary>
public sealed class GetErgonomicsAndMaterialHandlingQueryHandler : IRequestHandler<GetErgonomicsAndMaterialHandlingQuery, Result<ErgonomicsAndMaterialHandlingQueryResult>>
{
    private readonly IErgonomicsAndMaterialHandlingRepository _repository;
    private readonly ILogger<GetErgonomicsAndMaterialHandlingQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetErgonomicsAndMaterialHandlingQueryHandler(IErgonomicsAndMaterialHandlingRepository repository, ILogger<GetErgonomicsAndMaterialHandlingQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ErgonomicsAndMaterialHandlingQueryResult>> Handle(GetErgonomicsAndMaterialHandlingQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetErgonomicsAndMaterialHandlingItemsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<ErgonomicsAndMaterialHandlingQueryResult>.Success(new ErgonomicsAndMaterialHandlingQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve ErgonomicsAndMaterialHandling records for Tenant {TenantId}", request.TenantId);

            return Result<ErgonomicsAndMaterialHandlingQueryResult>.Failure(new ApplicationError(
                "EmployeeSafety.ErgonomicsAndMaterialHandling.RetrievalFailed",
                "Unable to retrieve ErgonomicsAndMaterialHandling records at this time.",
                Guid.NewGuid()));
        }
    }
}
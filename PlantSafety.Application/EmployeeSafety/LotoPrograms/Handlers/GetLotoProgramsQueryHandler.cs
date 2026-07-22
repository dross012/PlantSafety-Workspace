// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetLotoProgramsQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.EmployeeSafety.LotoPrograms.Queries;
using PlantSafety.Application.EmployeeSafety.LotoPrograms.Repositories;

namespace PlantSafety.Application.EmployeeSafety.LotoPrograms.Handlers;

/// <summary>Handles <see cref="GetLotoProgramsQuery"/>.</summary>
public sealed class GetLotoProgramsQueryHandler : IRequestHandler<GetLotoProgramsQuery, Result<LotoProgramsQueryResult>>
{
    private readonly ILotoProgramsRepository _repository;
    private readonly ILogger<GetLotoProgramsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetLotoProgramsQueryHandler(ILotoProgramsRepository repository, ILogger<GetLotoProgramsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<LotoProgramsQueryResult>> Handle(GetLotoProgramsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetLotoProgramsItemsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<LotoProgramsQueryResult>.Success(new LotoProgramsQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve LotoPrograms records for Tenant {TenantId}", request.TenantId);

            return Result<LotoProgramsQueryResult>.Failure(new ApplicationError(
                "EmployeeSafety.LotoPrograms.RetrievalFailed",
                "Unable to retrieve LotoPrograms records at this time.",
                Guid.NewGuid()));
        }
    }
}
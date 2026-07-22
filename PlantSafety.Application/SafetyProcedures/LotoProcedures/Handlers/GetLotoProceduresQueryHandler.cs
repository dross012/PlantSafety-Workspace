// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetLotoProceduresQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.SafetyProcedures.LotoProcedures.Queries;
using PlantSafety.Application.SafetyProcedures.LotoProcedures.Repositories;

namespace PlantSafety.Application.SafetyProcedures.LotoProcedures.Handlers;

/// <summary>Handles <see cref="GetLotoProceduresQuery"/>.</summary>
public sealed class GetLotoProceduresQueryHandler : IRequestHandler<GetLotoProceduresQuery, Result<LotoProceduresQueryResult>>
{
    private readonly ILotoProceduresRepository _repository;
    private readonly ILogger<GetLotoProceduresQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetLotoProceduresQueryHandler(ILotoProceduresRepository repository, ILogger<GetLotoProceduresQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<LotoProceduresQueryResult>> Handle(GetLotoProceduresQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetLotoProceduresItemsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<LotoProceduresQueryResult>.Success(new LotoProceduresQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve LotoProcedures records for Tenant {TenantId}", request.TenantId);

            return Result<LotoProceduresQueryResult>.Failure(new ApplicationError(
                "SafetyProcedures.LotoProcedures.RetrievalFailed",
                "Unable to retrieve LotoProcedures records at this time.",
                Guid.NewGuid()));
        }
    }
}
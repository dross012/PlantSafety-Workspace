// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetHazMatProceduresQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.Procedures.HazMatProcedures.Queries;
using PlantSafety.Application.Compliance.Procedures.HazMatProcedures.Repositories;

namespace PlantSafety.Application.Compliance.Procedures.HazMatProcedures.Handlers;

/// <summary>Handles <see cref="GetHazMatProceduresQuery"/>.</summary>
public sealed class GetHazMatProceduresQueryHandler : IRequestHandler<GetHazMatProceduresQuery, Result<HazMatProceduresQueryResult>>
{
    private readonly IHazMatProceduresRepository _repository;
    private readonly ILogger<GetHazMatProceduresQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetHazMatProceduresQueryHandler(IHazMatProceduresRepository repository, ILogger<GetHazMatProceduresQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<HazMatProceduresQueryResult>> Handle(GetHazMatProceduresQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetHazMatProceduresAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<HazMatProceduresQueryResult>.Success(new HazMatProceduresQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve HazMatProcedures for Tenant {TenantId}", request.TenantId);

            return Result<HazMatProceduresQueryResult>.Failure(new ApplicationError(
                "Compliance.Procedures.HazMatProcedures.RetrievalFailed",
                "Unable to retrieve HazMatProcedures data at this time.",
                Guid.NewGuid()));
        }
    }
}

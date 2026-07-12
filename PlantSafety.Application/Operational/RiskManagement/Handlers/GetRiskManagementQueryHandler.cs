// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetRiskManagementQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operational.RiskManagement.Queries;
using PlantSafety.Application.Operational.RiskManagement.Repositories;

namespace PlantSafety.Application.Operational.RiskManagement.Handlers;

/// <summary>Handles <see cref="GetRiskManagementQuery"/>.</summary>
public sealed class GetRiskManagementQueryHandler : IRequestHandler<GetRiskManagementQuery, Result<RiskManagementQueryResult>>
{
    private readonly IRiskManagementRepository _repository;
    private readonly ILogger<GetRiskManagementQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetRiskManagementQueryHandler(IRiskManagementRepository repository, ILogger<GetRiskManagementQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<RiskManagementQueryResult>> Handle(GetRiskManagementQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetRiskManagementAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<RiskManagementQueryResult>.Success(new RiskManagementQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve RiskManagement for Tenant {TenantId}", request.TenantId);

            return Result<RiskManagementQueryResult>.Failure(new ApplicationError(
                "Operational.RiskManagement.RetrievalFailed",
                "Unable to retrieve RiskManagement data at this time.",
                Guid.NewGuid()));
        }
    }
}

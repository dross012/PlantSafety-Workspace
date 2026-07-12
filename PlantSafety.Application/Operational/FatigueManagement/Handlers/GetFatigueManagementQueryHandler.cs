// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetFatigueManagementQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operational.FatigueManagement.Queries;
using PlantSafety.Application.Operational.FatigueManagement.Repositories;

namespace PlantSafety.Application.Operational.FatigueManagement.Handlers;

/// <summary>Handles <see cref="GetFatigueManagementQuery"/>.</summary>
public sealed class GetFatigueManagementQueryHandler : IRequestHandler<GetFatigueManagementQuery, Result<FatigueManagementQueryResult>>
{
    private readonly IFatigueManagementRepository _repository;
    private readonly ILogger<GetFatigueManagementQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetFatigueManagementQueryHandler(IFatigueManagementRepository repository, ILogger<GetFatigueManagementQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<FatigueManagementQueryResult>> Handle(GetFatigueManagementQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetFatigueManagementAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<FatigueManagementQueryResult>.Success(new FatigueManagementQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve FatigueManagement for Tenant {TenantId}", request.TenantId);

            return Result<FatigueManagementQueryResult>.Failure(new ApplicationError(
                "Operational.FatigueManagement.RetrievalFailed",
                "Unable to retrieve FatigueManagement data at this time.",
                Guid.NewGuid()));
        }
    }
}

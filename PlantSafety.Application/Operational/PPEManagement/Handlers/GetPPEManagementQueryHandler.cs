// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetPPEManagementQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operational.PPEManagement.Queries;
using PlantSafety.Application.Operational.PPEManagement.Repositories;

namespace PlantSafety.Application.Operational.PPEManagement.Handlers;

/// <summary>Handles <see cref="GetPPEManagementQuery"/>.</summary>
public sealed class GetPPEManagementQueryHandler : IRequestHandler<GetPPEManagementQuery, Result<PPEManagementQueryResult>>
{
    private readonly IPPEManagementRepository _repository;
    private readonly ILogger<GetPPEManagementQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetPPEManagementQueryHandler(IPPEManagementRepository repository, ILogger<GetPPEManagementQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PPEManagementQueryResult>> Handle(GetPPEManagementQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetPPEManagementAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<PPEManagementQueryResult>.Success(new PPEManagementQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve PPEManagement for Tenant {TenantId}", request.TenantId);

            return Result<PPEManagementQueryResult>.Failure(new ApplicationError(
                "Operational.PPEManagement.RetrievalFailed",
                "Unable to retrieve PPEManagement data at this time.",
                Guid.NewGuid()));
        }
    }
}

// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetApprovalsQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.Policy.Approvals.Queries;
using PlantSafety.Application.Compliance.Policy.Approvals.Repositories;

namespace PlantSafety.Application.Compliance.Policy.Approvals.Handlers;

/// <summary>Handles <see cref="GetApprovalsQuery"/>.</summary>
public sealed class GetApprovalsQueryHandler : IRequestHandler<GetApprovalsQuery, Result<ApprovalsQueryResult>>
{
    private readonly IApprovalsRepository _repository;
    private readonly ILogger<GetApprovalsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetApprovalsQueryHandler(IApprovalsRepository repository, ILogger<GetApprovalsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ApprovalsQueryResult>> Handle(GetApprovalsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetApprovalsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<ApprovalsQueryResult>.Success(new ApprovalsQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Approvals for Tenant {TenantId}", request.TenantId);

            return Result<ApprovalsQueryResult>.Failure(new ApplicationError(
                "Compliance.Policy.Approvals.RetrievalFailed",
                "Unable to retrieve Approvals data at this time.",
                Guid.NewGuid()));
        }
    }
}

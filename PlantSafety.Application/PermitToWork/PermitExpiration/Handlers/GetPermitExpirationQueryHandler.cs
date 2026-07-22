// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetPermitExpirationQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.PermitToWork.PermitExpiration.Queries;
using PlantSafety.Application.PermitToWork.PermitExpiration.Repositories;

namespace PlantSafety.Application.PermitToWork.PermitExpiration.Handlers;

/// <summary>Handles <see cref="GetPermitExpirationQuery"/>.</summary>
public sealed class GetPermitExpirationQueryHandler : IRequestHandler<GetPermitExpirationQuery, Result<PermitExpirationQueryResult>>
{
    private readonly IPermitExpirationRepository _repository;
    private readonly ILogger<GetPermitExpirationQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetPermitExpirationQueryHandler(IPermitExpirationRepository repository, ILogger<GetPermitExpirationQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PermitExpirationQueryResult>> Handle(GetPermitExpirationQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetPermitExpirationAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<PermitExpirationQueryResult>.Success(new PermitExpirationQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve PermitExpiration for Tenant {TenantId}", request.TenantId);

            return Result<PermitExpirationQueryResult>.Failure(new ApplicationError(
                "PermitToWork.PermitExpiration.RetrievalFailed",
                "Unable to retrieve PermitExpiration data at this time.",
                Guid.NewGuid()));
        }
    }
}
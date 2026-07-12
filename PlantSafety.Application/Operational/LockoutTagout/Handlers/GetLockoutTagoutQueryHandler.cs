// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetLockoutTagoutQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operational.LockoutTagout.Queries;
using PlantSafety.Application.Operational.LockoutTagout.Repositories;

namespace PlantSafety.Application.Operational.LockoutTagout.Handlers;

/// <summary>Handles <see cref="GetLockoutTagoutQuery"/>.</summary>
public sealed class GetLockoutTagoutQueryHandler : IRequestHandler<GetLockoutTagoutQuery, Result<LockoutTagoutQueryResult>>
{
    private readonly ILockoutTagoutRepository _repository;
    private readonly ILogger<GetLockoutTagoutQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetLockoutTagoutQueryHandler(ILockoutTagoutRepository repository, ILogger<GetLockoutTagoutQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<LockoutTagoutQueryResult>> Handle(GetLockoutTagoutQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetLockoutTagoutAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<LockoutTagoutQueryResult>.Success(new LockoutTagoutQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve LockoutTagout for Tenant {TenantId}", request.TenantId);

            return Result<LockoutTagoutQueryResult>.Failure(new ApplicationError(
                "Operational.LockoutTagout.RetrievalFailed",
                "Unable to retrieve LockoutTagout data at this time.",
                Guid.NewGuid()));
        }
    }
}

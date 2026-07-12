// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetUsersQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.CrossDomain.Users.Queries;
using PlantSafety.Application.CrossDomain.Users.Repositories;

namespace PlantSafety.Application.CrossDomain.Users.Handlers;

/// <summary>Handles <see cref="GetUsersQuery"/>.</summary>
public sealed class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, Result<UsersQueryResult>>
{
    private readonly IUsersRepository _repository;
    private readonly ILogger<GetUsersQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetUsersQueryHandler(IUsersRepository repository, ILogger<GetUsersQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<UsersQueryResult>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetUsersAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<UsersQueryResult>.Success(new UsersQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Users for Tenant {TenantId}", request.TenantId);

            return Result<UsersQueryResult>.Failure(new ApplicationError(
                "CrossDomain.Users.RetrievalFailed",
                "Unable to retrieve users data at this time.",
                Guid.NewGuid()));
        }
    }
}

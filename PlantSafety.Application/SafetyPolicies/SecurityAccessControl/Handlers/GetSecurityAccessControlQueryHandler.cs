// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetSecurityAccessControlQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.SafetyPolicies.SecurityAccessControl.Queries;
using PlantSafety.Application.SafetyPolicies.SecurityAccessControl.Repositories;

namespace PlantSafety.Application.SafetyPolicies.SecurityAccessControl.Handlers;

/// <summary>Handles <see cref="GetSecurityAccessControlQuery"/>.</summary>
public sealed class GetSecurityAccessControlQueryHandler : IRequestHandler<GetSecurityAccessControlQuery, Result<SecurityAccessControlQueryResult>>
{
    private readonly ISecurityAccessControlRepository _repository;
    private readonly ILogger<GetSecurityAccessControlQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetSecurityAccessControlQueryHandler(ISecurityAccessControlRepository repository, ILogger<GetSecurityAccessControlQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<SecurityAccessControlQueryResult>> Handle(GetSecurityAccessControlQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetSecurityAccessControlAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<SecurityAccessControlQueryResult>.Success(new SecurityAccessControlQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve SecurityAccessControl for Tenant {TenantId}", request.TenantId);

            return Result<SecurityAccessControlQueryResult>.Failure(new ApplicationError(
                "SafetyPolicies.SecurityAccessControl.RetrievalFailed",
                "Unable to retrieve SecurityAccessControl data at this time.",
                Guid.NewGuid()));
        }
    }
}
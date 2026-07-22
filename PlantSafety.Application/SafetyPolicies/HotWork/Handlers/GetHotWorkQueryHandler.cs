// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetHotWorkQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.SafetyPolicies.HotWork.Queries;
using PlantSafety.Application.SafetyPolicies.HotWork.Repositories;

namespace PlantSafety.Application.SafetyPolicies.HotWork.Handlers;

/// <summary>Handles <see cref="GetHotWorkQuery"/>.</summary>
public sealed class GetHotWorkQueryHandler : IRequestHandler<GetHotWorkQuery, Result<HotWorkQueryResult>>
{
    private readonly IHotWorkRepository _repository;
    private readonly ILogger<GetHotWorkQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetHotWorkQueryHandler(IHotWorkRepository repository, ILogger<GetHotWorkQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<HotWorkQueryResult>> Handle(GetHotWorkQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetHotWorkAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<HotWorkQueryResult>.Success(new HotWorkQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve HotWork for Tenant {TenantId}", request.TenantId);

            return Result<HotWorkQueryResult>.Failure(new ApplicationError(
                "SafetyPolicies.HotWork.RetrievalFailed",
                "Unable to retrieve HotWork data at this time.",
                Guid.NewGuid()));
        }
    }
}
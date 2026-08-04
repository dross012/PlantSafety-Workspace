// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for ListActiveSabotageIndicatorsQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.CyberPhysicalSecurity.SabotageIndicator.Models;
using PlantSafety.Application.CyberPhysicalSecurity.SabotageIndicator.Queries;
using PlantSafety.Application.CyberPhysicalSecurity.SabotageIndicator.Repositories;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.CyberPhysicalSecurity.SabotageIndicator.Handlers;

/// <summary>Handles <see cref="ListActiveSabotageIndicatorsQuery"/>.</summary>
public sealed class ListActiveSabotageIndicatorsQueryHandler : IRequestHandler<ListActiveSabotageIndicatorsQuery, Result<IReadOnlyList<SabotageIndicatorDto>>>
{
    private readonly ISabotageIndicatorReadRepository _repository;
    private readonly ILogger<ListActiveSabotageIndicatorsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ListActiveSabotageIndicatorsQueryHandler(ISabotageIndicatorReadRepository repository, ILogger<ListActiveSabotageIndicatorsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<SabotageIndicatorDto>>> Handle(ListActiveSabotageIndicatorsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.ListActiveAsync(request.TenantId, cancellationToken);
            return Result<IReadOnlyList<SabotageIndicatorDto>>.Success(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list active SabotageIndicators for Tenant {TenantId}", request.TenantId);

            return Result<IReadOnlyList<SabotageIndicatorDto>>.Failure(new ApplicationError(
                "CyberPhysicalSecurity.SabotageIndicator.ListFailed",
                "Unable to list active sabotage indicators at this time.",
                Guid.NewGuid()));
        }
    }
}

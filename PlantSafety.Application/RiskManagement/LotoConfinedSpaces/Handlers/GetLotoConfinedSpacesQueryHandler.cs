// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetLotoConfinedSpacesQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.RiskManagement.LotoConfinedSpaces.Queries;
using PlantSafety.Application.RiskManagement.LotoConfinedSpaces.Repositories;

namespace PlantSafety.Application.RiskManagement.LotoConfinedSpaces.Handlers;

/// <summary>Handles <see cref="GetLotoConfinedSpacesQuery"/>.</summary>
public sealed class GetLotoConfinedSpacesQueryHandler : IRequestHandler<GetLotoConfinedSpacesQuery, Result<LotoConfinedSpacesQueryResult>>
{
    private readonly ILotoConfinedSpacesRepository _repository;
    private readonly ILogger<GetLotoConfinedSpacesQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetLotoConfinedSpacesQueryHandler(ILotoConfinedSpacesRepository repository, ILogger<GetLotoConfinedSpacesQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<LotoConfinedSpacesQueryResult>> Handle(GetLotoConfinedSpacesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetLotoConfinedSpacesAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<LotoConfinedSpacesQueryResult>.Success(new LotoConfinedSpacesQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve LotoConfinedSpaces for Tenant {TenantId}", request.TenantId);

            return Result<LotoConfinedSpacesQueryResult>.Failure(new ApplicationError(
                "RiskManagement.LotoConfinedSpaces.RetrievalFailed",
                "Unable to retrieve LotoConfinedSpaces data at this time.",
                Guid.NewGuid()));
        }
    }
}
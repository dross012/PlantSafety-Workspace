// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetErgonomicsMaterialHandlingQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.RiskManagement.ErgonomicsMaterialHandling.Queries;
using PlantSafety.Application.RiskManagement.ErgonomicsMaterialHandling.Repositories;

namespace PlantSafety.Application.RiskManagement.ErgonomicsMaterialHandling.Handlers;

/// <summary>Handles <see cref="GetErgonomicsMaterialHandlingQuery"/>.</summary>
public sealed class GetErgonomicsMaterialHandlingQueryHandler : IRequestHandler<GetErgonomicsMaterialHandlingQuery, Result<ErgonomicsMaterialHandlingQueryResult>>
{
    private readonly IErgonomicsMaterialHandlingRepository _repository;
    private readonly ILogger<GetErgonomicsMaterialHandlingQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetErgonomicsMaterialHandlingQueryHandler(IErgonomicsMaterialHandlingRepository repository, ILogger<GetErgonomicsMaterialHandlingQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ErgonomicsMaterialHandlingQueryResult>> Handle(GetErgonomicsMaterialHandlingQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetErgonomicsMaterialHandlingAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<ErgonomicsMaterialHandlingQueryResult>.Success(new ErgonomicsMaterialHandlingQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve ErgonomicsMaterialHandling for Tenant {TenantId}", request.TenantId);

            return Result<ErgonomicsMaterialHandlingQueryResult>.Failure(new ApplicationError(
                "RiskManagement.ErgonomicsMaterialHandling.RetrievalFailed",
                "Unable to retrieve ErgonomicsMaterialHandling data at this time.",
                Guid.NewGuid()));
        }
    }
}
// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetSafetyValvesRelaysQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.MachineSafety.SafetyValvesRelays.Queries;
using PlantSafety.Application.MachineSafety.SafetyValvesRelays.Repositories;

namespace PlantSafety.Application.MachineSafety.SafetyValvesRelays.Handlers;

/// <summary>Handles <see cref="GetSafetyValvesRelaysQuery"/>.</summary>
public sealed class GetSafetyValvesRelaysQueryHandler : IRequestHandler<GetSafetyValvesRelaysQuery, Result<SafetyValvesRelaysQueryResult>>
{
    private readonly ISafetyValvesRelaysRepository _repository;
    private readonly ILogger<GetSafetyValvesRelaysQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetSafetyValvesRelaysQueryHandler(ISafetyValvesRelaysRepository repository, ILogger<GetSafetyValvesRelaysQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<SafetyValvesRelaysQueryResult>> Handle(GetSafetyValvesRelaysQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetSafetyValvesRelaysItemsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<SafetyValvesRelaysQueryResult>.Success(new SafetyValvesRelaysQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve SafetyValvesRelays records for Tenant {TenantId}", request.TenantId);

            return Result<SafetyValvesRelaysQueryResult>.Failure(new ApplicationError(
                "MachineSafety.SafetyValvesRelays.RetrievalFailed",
                "Unable to retrieve SafetyValvesRelays records at this time.",
                Guid.NewGuid()));
        }
    }
}
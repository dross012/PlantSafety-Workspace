// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetPMSchedulingQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Scheduling.PMScheduling.Queries;
using PlantSafety.Application.Scheduling.PMScheduling.Repositories;

namespace PlantSafety.Application.Scheduling.PMScheduling.Handlers;

/// <summary>Handles <see cref="GetPMSchedulingQuery"/>.</summary>
public sealed class GetPMSchedulingQueryHandler : IRequestHandler<GetPMSchedulingQuery, Result<PMSchedulingQueryResult>>
{
    private readonly IPMSchedulingRepository _repository;
    private readonly ILogger<GetPMSchedulingQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetPMSchedulingQueryHandler(IPMSchedulingRepository repository, ILogger<GetPMSchedulingQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PMSchedulingQueryResult>> Handle(GetPMSchedulingQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetPMSchedulingAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<PMSchedulingQueryResult>.Success(new PMSchedulingQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve PMScheduling for Tenant {TenantId}", request.TenantId);

            return Result<PMSchedulingQueryResult>.Failure(new ApplicationError(
                "Scheduling.PMScheduling.RetrievalFailed",
                "Unable to retrieve PMScheduling data at this time.",
                Guid.NewGuid()));
        }
    }
}
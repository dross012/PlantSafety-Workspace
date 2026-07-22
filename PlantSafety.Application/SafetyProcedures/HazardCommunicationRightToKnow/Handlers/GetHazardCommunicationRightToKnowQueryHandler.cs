// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetHazardCommunicationRightToKnowQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.SafetyProcedures.HazardCommunicationRightToKnow.Queries;
using PlantSafety.Application.SafetyProcedures.HazardCommunicationRightToKnow.Repositories;

namespace PlantSafety.Application.SafetyProcedures.HazardCommunicationRightToKnow.Handlers;

/// <summary>Handles <see cref="GetHazardCommunicationRightToKnowQuery"/>.</summary>
public sealed class GetHazardCommunicationRightToKnowQueryHandler : IRequestHandler<GetHazardCommunicationRightToKnowQuery, Result<HazardCommunicationRightToKnowQueryResult>>
{
    private readonly IHazardCommunicationRightToKnowRepository _repository;
    private readonly ILogger<GetHazardCommunicationRightToKnowQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetHazardCommunicationRightToKnowQueryHandler(IHazardCommunicationRightToKnowRepository repository, ILogger<GetHazardCommunicationRightToKnowQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<HazardCommunicationRightToKnowQueryResult>> Handle(GetHazardCommunicationRightToKnowQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetHazardCommunicationRightToKnowItemsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<HazardCommunicationRightToKnowQueryResult>.Success(new HazardCommunicationRightToKnowQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve HazardCommunicationRightToKnow records for Tenant {TenantId}", request.TenantId);

            return Result<HazardCommunicationRightToKnowQueryResult>.Failure(new ApplicationError(
                "SafetyProcedures.HazardCommunicationRightToKnow.RetrievalFailed",
                "Unable to retrieve HazardCommunicationRightToKnow records at this time.",
                Guid.NewGuid()));
        }
    }
}
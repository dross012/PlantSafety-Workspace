// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetRegulatoryRequirementsQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.RegulatoryRequirements.Queries;
using PlantSafety.Application.Compliance.RegulatoryRequirements.Repositories;

namespace PlantSafety.Application.Compliance.RegulatoryRequirements.Handlers;

/// <summary>Handles <see cref="GetRegulatoryRequirementsQuery"/>.</summary>
public sealed class GetRegulatoryRequirementsQueryHandler : IRequestHandler<GetRegulatoryRequirementsQuery, Result<RegulatoryRequirementsQueryResult>>
{
    private readonly IRegulatoryRequirementsRepository _repository;
    private readonly ILogger<GetRegulatoryRequirementsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetRegulatoryRequirementsQueryHandler(IRegulatoryRequirementsRepository repository, ILogger<GetRegulatoryRequirementsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<RegulatoryRequirementsQueryResult>> Handle(GetRegulatoryRequirementsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetRegulatoryRequirementsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<RegulatoryRequirementsQueryResult>.Success(new RegulatoryRequirementsQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve RegulatoryRequirements for Tenant {TenantId}", request.TenantId);

            return Result<RegulatoryRequirementsQueryResult>.Failure(new ApplicationError(
                "Compliance.RegulatoryRequirements.RetrievalFailed",
                "Unable to retrieve regulatory requirements data at this time.",
                Guid.NewGuid()));
        }
    }
}

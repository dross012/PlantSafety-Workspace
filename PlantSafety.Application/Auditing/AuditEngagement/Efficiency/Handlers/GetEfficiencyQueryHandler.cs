// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetEfficiencyQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Auditing.AuditEngagement.Efficiency.Queries;
using PlantSafety.Application.Auditing.AuditEngagement.Efficiency.Repositories;

namespace PlantSafety.Application.Auditing.AuditEngagement.Efficiency.Handlers;

/// <summary>Handles <see cref="GetEfficiencyQuery"/>.</summary>
public sealed class GetEfficiencyQueryHandler : IRequestHandler<GetEfficiencyQuery, Result<EfficiencyQueryResult>>
{
    private readonly IEfficiencyRepository _repository;
    private readonly ILogger<GetEfficiencyQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetEfficiencyQueryHandler(IEfficiencyRepository repository, ILogger<GetEfficiencyQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<EfficiencyQueryResult>> Handle(GetEfficiencyQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetItemsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<EfficiencyQueryResult>.Success(new EfficiencyQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Efficiency for Tenant {TenantId}", request.TenantId);

            return Result<EfficiencyQueryResult>.Failure(new ApplicationError(
                "Auditing.AuditEngagement.Efficiency.RetrievalFailed",
                "Unable to retrieve Efficiency Auditing data at this time.",
                Guid.NewGuid()));
        }
    }
}
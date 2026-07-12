// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetSOPsQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.Procedures.SOPs.Queries;
using PlantSafety.Application.Compliance.Procedures.SOPs.Repositories;

namespace PlantSafety.Application.Compliance.Procedures.SOPs.Handlers;

/// <summary>Handles <see cref="GetSOPsQuery"/>.</summary>
public sealed class GetSOPsQueryHandler : IRequestHandler<GetSOPsQuery, Result<SOPsQueryResult>>
{
    private readonly ISOPsRepository _repository;
    private readonly ILogger<GetSOPsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetSOPsQueryHandler(ISOPsRepository repository, ILogger<GetSOPsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<SOPsQueryResult>> Handle(GetSOPsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetSOPsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<SOPsQueryResult>.Success(new SOPsQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve SOPs for Tenant {TenantId}", request.TenantId);

            return Result<SOPsQueryResult>.Failure(new ApplicationError(
                "Compliance.Procedures.SOPs.RetrievalFailed",
                "Unable to retrieve SOPs data at this time.",
                Guid.NewGuid()));
        }
    }
}

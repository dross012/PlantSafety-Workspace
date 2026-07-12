// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetDOTQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.Certification.DOT.Queries;
using PlantSafety.Application.Compliance.Certification.DOT.Repositories;

namespace PlantSafety.Application.Compliance.Certification.DOT.Handlers;

/// <summary>Handles <see cref="GetDOTQuery"/>.</summary>
public sealed class GetDOTQueryHandler : IRequestHandler<GetDOTQuery, Result<DOTQueryResult>>
{
    private readonly IDOTRepository _repository;
    private readonly ILogger<GetDOTQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetDOTQueryHandler(IDOTRepository repository, ILogger<GetDOTQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<DOTQueryResult>> Handle(GetDOTQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetDOTAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<DOTQueryResult>.Success(new DOTQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve DOT for Tenant {TenantId}", request.TenantId);

            return Result<DOTQueryResult>.Failure(new ApplicationError(
                "Compliance.Certification.DOT.RetrievalFailed",
                "Unable to retrieve DOT certification data at this time.",
                Guid.NewGuid()));
        }
    }
}

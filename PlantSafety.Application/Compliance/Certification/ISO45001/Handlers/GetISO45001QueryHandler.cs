// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetISO45001Query.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.Certification.ISO45001.Queries;
using PlantSafety.Application.Compliance.Certification.ISO45001.Repositories;

namespace PlantSafety.Application.Compliance.Certification.ISO45001.Handlers;

/// <summary>Handles <see cref="GetISO45001Query"/>.</summary>
public sealed class GetISO45001QueryHandler : IRequestHandler<GetISO45001Query, Result<ISO45001QueryResult>>
{
    private readonly IISO45001Repository _repository;
    private readonly ILogger<GetISO45001QueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetISO45001QueryHandler(IISO45001Repository repository, ILogger<GetISO45001QueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ISO45001QueryResult>> Handle(GetISO45001Query request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetISO45001Async(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<ISO45001QueryResult>.Success(new ISO45001QueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve ISO45001 for Tenant {TenantId}", request.TenantId);

            return Result<ISO45001QueryResult>.Failure(new ApplicationError(
                "Compliance.Certification.ISO45001.RetrievalFailed",
                "Unable to retrieve ISO 45001 certification data at this time.",
                Guid.NewGuid()));
        }
    }
}

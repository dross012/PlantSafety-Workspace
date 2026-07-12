// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetISO14001Query.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.Certification.ISO14001.Queries;
using PlantSafety.Application.Compliance.Certification.ISO14001.Repositories;

namespace PlantSafety.Application.Compliance.Certification.ISO14001.Handlers;

/// <summary>Handles <see cref="GetISO14001Query"/>.</summary>
public sealed class GetISO14001QueryHandler : IRequestHandler<GetISO14001Query, Result<ISO14001QueryResult>>
{
    private readonly IISO14001Repository _repository;
    private readonly ILogger<GetISO14001QueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetISO14001QueryHandler(IISO14001Repository repository, ILogger<GetISO14001QueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ISO14001QueryResult>> Handle(GetISO14001Query request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetISO14001Async(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<ISO14001QueryResult>.Success(new ISO14001QueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve ISO14001 for Tenant {TenantId}", request.TenantId);

            return Result<ISO14001QueryResult>.Failure(new ApplicationError(
                "Compliance.Certification.ISO14001.RetrievalFailed",
                "Unable to retrieve ISO 14001 certification data at this time.",
                Guid.NewGuid()));
        }
    }
}

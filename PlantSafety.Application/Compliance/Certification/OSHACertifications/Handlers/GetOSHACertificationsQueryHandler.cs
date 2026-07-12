// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetOSHACertificationsQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.Certification.OSHACertifications.Queries;
using PlantSafety.Application.Compliance.Certification.OSHACertifications.Repositories;

namespace PlantSafety.Application.Compliance.Certification.OSHACertifications.Handlers;

/// <summary>Handles <see cref="GetOSHACertificationsQuery"/>.</summary>
public sealed class GetOSHACertificationsQueryHandler : IRequestHandler<GetOSHACertificationsQuery, Result<OSHACertificationsQueryResult>>
{
    private readonly IOSHACertificationsRepository _repository;
    private readonly ILogger<GetOSHACertificationsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetOSHACertificationsQueryHandler(IOSHACertificationsRepository repository, ILogger<GetOSHACertificationsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<OSHACertificationsQueryResult>> Handle(GetOSHACertificationsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetOSHACertificationsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<OSHACertificationsQueryResult>.Success(new OSHACertificationsQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve OSHACertifications for Tenant {TenantId}", request.TenantId);

            return Result<OSHACertificationsQueryResult>.Failure(new ApplicationError(
                "Compliance.Certification.OSHACertifications.RetrievalFailed",
                "Unable to retrieve OSHA certification data at this time.",
                Guid.NewGuid()));
        }
    }
}

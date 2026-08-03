// AUTHOR: Enterprise Integration & Ecosystem Expansion Agent
// CREATED: 2026-08-02
// FILE DESCRIPTION: Handler for ResolvePartnerIdentityMappingQuery. Read-only
//   orchestration: calls the repository's read-side lookup only.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Identity.DTOs;
using PlantSafety.Application.Identity.Mappers;
using PlantSafety.Application.Identity.Queries;
using PlantSafety.Domain.Identity.Repositories;

namespace PlantSafety.Application.Identity.QueryHandlers;

/// <summary>Handles <see cref="ResolvePartnerIdentityMappingQuery"/>.</summary>
public sealed class ResolvePartnerIdentityMappingQueryHandler
    : IRequestHandler<ResolvePartnerIdentityMappingQuery, PartnerIdentityMappingDto?>
{
    private readonly IPartnerIdentityMappingRepository _repository;
    private readonly ILogger<ResolvePartnerIdentityMappingQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    /// <param name="repository">The partner identity mapping repository.</param>
    /// <param name="logger">The structured logger.</param>
    public ResolvePartnerIdentityMappingQueryHandler(
        IPartnerIdentityMappingRepository repository,
        ILogger<ResolvePartnerIdentityMappingQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<PartnerIdentityMappingDto?> Handle(
        ResolvePartnerIdentityMappingQuery request,
        CancellationToken cancellationToken)
    {
        var mapping = await _repository.FindActiveAsync(
            request.TenantId, request.PartnerIssuer, request.PartnerExternalId, cancellationToken);

        if (mapping is null)
        {
            _logger.LogDebug(
                "No active partner identity mapping found for Tenant {TenantId}, Issuer {PartnerIssuer}, CorrelationId {CorrelationId}",
                request.TenantId, request.PartnerIssuer, request.CorrelationId);
            return null;
        }

        return PartnerIdentityMappingMapper.ToDto(mapping);
    }
}

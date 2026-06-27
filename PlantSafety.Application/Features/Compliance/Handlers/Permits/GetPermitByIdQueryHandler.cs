// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetPermitByIdQuery. Returns null when not found.
// SchemaVersion: 1

using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Features.Compliance.Models;
using PlantSafety.Application.Features.Compliance.Queries.Permits.Get;

namespace PlantSafety.Application.Features.Compliance.Handlers.Permits;

/// <summary>Handles <see cref="GetPermitByIdQuery"/>.</summary>
public sealed class GetPermitByIdQueryHandler : IQueryHandler<GetPermitByIdQuery, PermitDto?>
{
    private readonly IPermitRepository _repository;
    private readonly ILogger<GetPermitByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetPermitByIdQueryHandler(IPermitRepository repository, ILogger<GetPermitByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<PermitDto?> Handle(GetPermitByIdQuery request, CancellationToken cancellationToken)
    {
        var model = await _repository.GetByIdAsync(request.TenantId, request.PermitId, cancellationToken);

        if (model is null)
        {
            _logger.LogDebug("Permit {PermitId} not found for Tenant {TenantId}", request.PermitId, request.TenantId);
            return null;
        }

        return new PermitDto
        {
            PermitId = model.PermitId,
            TenantId = model.TenantId,
            SiteId = model.SiteId,
            PermitType = model.PermitType,
            Title = model.Title,
            Description = model.Description,
            WorkLeadId = model.WorkLeadId,
            ValidFromUtc = model.ValidFromUtc,
            ValidToUtc = model.ValidToUtc,
            Status = model.Status,
            ApprovedByActorId = model.ApprovedByActorId,
            ApprovedAtUtc = model.ApprovedAtUtc,
            LastModifiedAtUtc = model.LastModifiedAtUtc,
            ETag = model.ETag,
        };
    }
}

// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for ApprovePermitCommand.
// SchemaVersion: 1

using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Caching;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Features.Compliance.Commands.Permits.Approve;

namespace PlantSafety.Application.Features.Compliance.Handlers.Permits;

/// <summary>Handles <see cref="ApprovePermitCommand"/>. Transitions the permit to Active status.</summary>
public sealed class ApprovePermitCommandHandler : ICommandHandler<ApprovePermitCommand, Guid>
{
    private readonly IPermitRepository _repository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<ApprovePermitCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ApprovePermitCommandHandler(IPermitRepository repository, ICacheService cacheService, ILogger<ApprovePermitCommandHandler> logger)
    {
        _repository = repository;
        _cacheService = cacheService;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Guid> Handle(ApprovePermitCommand request, CancellationToken cancellationToken)
    {
        await _repository.ApproveAsync(
            tenantId: request.TenantId,
            permitId: request.PermitId,
            approvalNotes: request.ApprovalNotes ?? string.Empty,
            eTag: request.ETag!,
            actorId: request.ActorId,
            correlationId: request.CorrelationId,
            cancellationToken: cancellationToken);

        // GetPermitByIdQuery implements ICacheableQuery with CacheKey "permits:{TenantId}:{PermitId}"
        // (see PlantSafety.Application.Features.Compliance.Queries.Permits.Get.GetPermitByIdQuery).
        // Without this invalidation a read-immediately-after-approve would return the stale,
        // pre-approval cached DTO -- the same class of bug fixed for
        // UpdateIncidentCommandHandler/DeleteIncidentCommandHandler.
        await _cacheService.RemoveAsync($"permits:{request.TenantId}:{request.PermitId}", cancellationToken);

        _logger.LogInformation(
            "Permit {PermitId} approved for Tenant {TenantId} by Actor {ActorId}",
            request.PermitId, request.TenantId, request.ActorId);

        return request.PermitId;
    }
}

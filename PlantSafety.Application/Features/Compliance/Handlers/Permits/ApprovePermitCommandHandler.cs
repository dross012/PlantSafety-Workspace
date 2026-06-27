// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for ApprovePermitCommand.
// SchemaVersion: 1

using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Features.Compliance.Commands.Permits.Approve;

namespace PlantSafety.Application.Features.Compliance.Handlers.Permits;

/// <summary>Handles <see cref="ApprovePermitCommand"/>. Transitions the permit to Active status.</summary>
public sealed class ApprovePermitCommandHandler : ICommandHandler<ApprovePermitCommand, Guid>
{
    private readonly IPermitRepository _repository;
    private readonly ILogger<ApprovePermitCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ApprovePermitCommandHandler(IPermitRepository repository, ILogger<ApprovePermitCommandHandler> logger)
    {
        _repository = repository;
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

        _logger.LogInformation(
            "Permit {PermitId} approved for Tenant {TenantId} by Actor {ActorId}",
            request.PermitId, request.TenantId, request.ActorId);

        return request.PermitId;
    }
}

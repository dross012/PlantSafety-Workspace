// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Handler for LinkAuditFindingToOshaRequirementsCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Auditing.AuditFinding.OshaCoverage.Repositories;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Auditing.AuditFinding.OshaCoverage.Commands;

/// <summary>Handles <see cref="LinkAuditFindingToOshaRequirementsCommand"/>.</summary>
public sealed class LinkAuditFindingToOshaRequirementsCommandHandler(
    IAuditFindingOshaCoverageRepository repository,
    ILogger<LinkAuditFindingToOshaRequirementsCommandHandler> logger)
    : IRequestHandler<LinkAuditFindingToOshaRequirementsCommand, Result<int>>
{
    /// <inheritdoc/>
    public async Task<Result<int>> Handle(LinkAuditFindingToOshaRequirementsCommand request, CancellationToken cancellationToken)
    {
        if (request.OSHARequirementIds.Count == 0)
        {
            return Result<int>.Failure(new ApplicationError(
                "Auditing.AuditFindingOshaCoverage.NoRequirementsSpecified",
                "At least one OSHA requirement id is required.",
                Guid.NewGuid()));
        }

        try
        {
            var linkedCount = await repository.LinkAsync(
                request.TenantId, request.AuditFindingId, request.OSHARequirementIds, request.ActorId, cancellationToken);

            return Result<int>.Success(linkedCount);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to link AuditFinding {AuditFindingId} to OSHA requirements for Tenant {TenantId}",
                request.AuditFindingId, request.TenantId);

            return Result<int>.Failure(new ApplicationError(
                "Auditing.AuditFindingOshaCoverage.LinkFailed",
                "Unable to link the audit finding to OSHA requirements at this time.",
                Guid.NewGuid()));
        }
    }
}

// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Handler for LinkAuditFindingToNfpaHazardsCommand. Loops HazardIds, calling
//   IAuditFindingNfpaCoverageRepository.LinkAsync once per hazard (the repository's
//   usp_AuditFindingNfpaCoverage_Link stored procedure is single-hazard-per-call, unlike OSHA's
//   equivalent which accepts the whole collection at the repository boundary and loops
//   internally) -- functionally identical idempotent per-pair linking, just with the loop moved
//   up one layer to match this repository's narrower LinkAsync signature.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Auditing.AuditFinding.NfpaCoverage.Repositories;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Auditing.AuditFinding.NfpaCoverage.Commands;

/// <summary>Handles <see cref="LinkAuditFindingToNfpaHazardsCommand"/>.</summary>
public sealed class LinkAuditFindingToNfpaHazardsCommandHandler(
    IAuditFindingNfpaCoverageRepository repository,
    ILogger<LinkAuditFindingToNfpaHazardsCommandHandler> logger)
    : IRequestHandler<LinkAuditFindingToNfpaHazardsCommand, Result<int>>
{
    /// <inheritdoc/>
    public async Task<Result<int>> Handle(LinkAuditFindingToNfpaHazardsCommand request, CancellationToken cancellationToken)
    {
        if (request.HazardIds.Count == 0)
        {
            return Result<int>.Failure(new ApplicationError(
                "Auditing.AuditFindingNfpaCoverage.NoHazardsSpecified",
                "At least one NFPA hazard id is required.",
                Guid.NewGuid()));
        }

        try
        {
            var linkedCount = 0;

            foreach (var hazardId in request.HazardIds.Distinct())
            {
                var linked = await repository.LinkAsync(
                    request.TenantId, request.AuditFindingId, hazardId, request.CreatedBy, cancellationToken);

                if (linked) linkedCount++;
            }

            return Result<int>.Success(linkedCount);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to link AuditFinding {AuditFindingId} to NFPA hazards for Tenant {TenantId}",
                request.AuditFindingId, request.TenantId);

            return Result<int>.Failure(new ApplicationError(
                "Auditing.AuditFindingNfpaCoverage.LinkFailed",
                "Unable to link the audit finding to NFPA hazards at this time.",
                Guid.NewGuid()));
        }
    }
}

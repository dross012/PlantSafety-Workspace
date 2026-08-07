// ============================================================================
// File: LinkAuditFindingNfpaHazardsRequest.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-08-07
// Description: Request body for linking an AuditFinding to the specific NFPA hazard profiles its
//              evidence covers.
// ============================================================================

namespace PlantSafety.Api.Auditing.Contracts.Requests;

/// <summary>Request body for linking an AuditFinding to NFPA hazard profiles.</summary>
public sealed record LinkAuditFindingNfpaHazardsRequest
{
    /// <summary>Identities of the NFPA hazard profiles this finding's evidence covers.</summary>
    public IReadOnlyCollection<Guid> HazardIds { get; init; } = [];
}

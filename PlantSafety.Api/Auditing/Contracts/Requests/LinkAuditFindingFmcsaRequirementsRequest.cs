// ============================================================================
// File: LinkAuditFindingFmcsaRequirementsRequest.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-08-07
// Description: Request body for linking an AuditFinding to the specific FMCSA requirements its
//              evidence covers. Mirrors LinkAuditFindingOshaRequirementsRequest exactly.
// ============================================================================

namespace PlantSafety.Api.Auditing.Contracts.Requests;

/// <summary>Request body for linking an AuditFinding to FMCSA requirements.</summary>
public sealed record LinkAuditFindingFmcsaRequirementsRequest
{
    /// <summary>Identities of the FMCSA requirements this finding's evidence covers.</summary>
    public IReadOnlyCollection<Guid> RequirementIds { get; init; } = [];
}

// ============================================================================
// File: LinkAuditFindingOshaRequirementsRequest.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-08-07
// Description: Request body for linking an AuditFinding to the specific OSHA requirements its
//              evidence covers.
// ============================================================================

namespace PlantSafety.Api.Auditing.Contracts.Requests;

/// <summary>Request body for linking an AuditFinding to OSHA requirements.</summary>
public sealed record LinkAuditFindingOshaRequirementsRequest
{
    /// <summary>Identities of the OSHA requirements this finding's evidence covers.</summary>
    public IReadOnlyCollection<Guid> OSHARequirementIds { get; init; } = [];
}

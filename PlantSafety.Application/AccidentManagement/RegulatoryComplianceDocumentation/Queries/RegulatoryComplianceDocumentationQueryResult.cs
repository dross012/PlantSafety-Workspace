// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetRegulatoryComplianceDocumentationQuery.
// SchemaVersion: 1

using PlantSafety.Application.AccidentManagement.RegulatoryComplianceDocumentation.Models;

namespace PlantSafety.Application.AccidentManagement.RegulatoryComplianceDocumentation.Queries;

/// <summary>Result of <see cref="GetRegulatoryComplianceDocumentationQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Regulatory Compliance &amp; Documentation items.</param>
public sealed record RegulatoryComplianceDocumentationQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<RegulatoryComplianceDocumentationItemDto> Items);
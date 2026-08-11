// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the Regulatory Compliance & Documentation read-model.
// SchemaVersion: 1

using PlantSafety.Application.AccidentManagement.RegulatoryComplianceDocumentation.Models;

namespace PlantSafety.Application.AccidentManagement.RegulatoryComplianceDocumentation.Repositories;

/// <summary>Read-only repository for Regulatory Compliance &amp; Documentation data. Implemented in the Infrastructure layer.</summary>
public interface IRegulatoryComplianceDocumentationRepository
{
    /// <summary>Returns Regulatory Compliance &amp; Documentation items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<RegulatoryComplianceDocumentationItemDto>> GetRegulatoryComplianceDocumentationAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Read-only Application-layer repository abstraction for the OSHA standards
//   reference data (Compliance.OSHAStandard/OSHARequirement/OSHAStandardChangeLog). Global data
//   -- every method is deliberately un-tenant-scoped, unlike every other repository in this
//   codebase. Implemented in Infrastructure.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.OSHAStandards.Models;

namespace PlantSafety.Application.Compliance.OSHAStandards.Repositories;

/// <summary>Read-only repository for OSHA standards reference data. Implemented in Infrastructure.</summary>
public interface IOSHAStandardsRepository
{
    /// <summary>Returns a single standard by (PartNumber, Section), or null.</summary>
    Task<OSHAStandardItemDto?> GetBySectionAsync(int partNumber, string section, CancellationToken cancellationToken);

    /// <summary>
    /// Free-text/part/status search across Compliance.OSHAStandard. <paramref name="keyword"/>
    /// matches against SectionTitle/SectionText (case-insensitive contains); all filters are
    /// optional and combine with AND. Results are ordered by PartNumber, Section.
    /// </summary>
    Task<IReadOnlyList<OSHAStandardItemDto>> SearchAsync(
        string? keyword,
        int? partNumber,
        string? status,
        int skip,
        int take,
        CancellationToken cancellationToken);

    /// <summary>Returns every requirement derived from a given standard.</summary>
    Task<IReadOnlyList<OSHARequirementItemDto>> GetRequirementsForStandardAsync(Guid oshaStandardId, CancellationToken cancellationToken);

    /// <summary>Returns every requirement mapped to a given PlantSafety module (AppliesToModule exact match).</summary>
    Task<IReadOnlyList<OSHARequirementItemDto>> GetRequirementsByModuleAsync(string appliesToModule, CancellationToken cancellationToken);

    /// <summary>Free-text search across Compliance.OSHARequirement.Notes (case-insensitive contains).</summary>
    Task<IReadOnlyList<OSHARequirementItemDto>> SearchRequirementsAsync(string keyword, int skip, int take, CancellationToken cancellationToken);

    /// <summary>Returns every change-log entry detected at or after <paramref name="sinceUtc"/>, most recent first.</summary>
    Task<IReadOnlyList<OSHAStandardChangeDto>> GetChangesSinceAsync(DateTime sinceUtc, CancellationToken cancellationToken);
}

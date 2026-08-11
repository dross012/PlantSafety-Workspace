// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the Lockout/Tagout (LOTO) & Confined Spaces read-model.
// SchemaVersion: 1

using PlantSafety.Application.RiskManagement.LotoConfinedSpaces.Models;

namespace PlantSafety.Application.RiskManagement.LotoConfinedSpaces.Repositories;

/// <summary>Read-only repository for Lockout/Tagout (LOTO) &amp; Confined Spaces data. Implemented in the Infrastructure layer.</summary>
public interface ILotoConfinedSpacesRepository
{
    /// <summary>Returns Lockout/Tagout (LOTO) &amp; Confined Spaces items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<LotoConfinedSpacesItemDto>> GetLotoConfinedSpacesAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
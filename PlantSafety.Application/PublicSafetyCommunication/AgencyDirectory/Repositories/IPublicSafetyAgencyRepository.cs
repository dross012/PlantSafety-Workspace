// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Repository abstraction for the PublicSafetyAgency read-model.
// SchemaVersion: 1

using PlantSafety.Application.PublicSafetyCommunication.AgencyDirectory.Models;

namespace PlantSafety.Application.PublicSafetyCommunication.AgencyDirectory.Repositories;

/// <summary>Read-only repository for PublicSafetyAgency data. Implemented in the Infrastructure layer.</summary>
public interface IPublicSafetyAgencyRepository
{
    /// <summary>Returns PublicSafetyAgency items for the tenant/site, optionally filtered by agency type and active-only.</summary>
    Task<IReadOnlyList<PublicSafetyAgencyItemDto>> ListPublicSafetyAgenciesAsync(
        Guid tenantId,
        Guid? siteId,
        string? agencyType,
        bool activeOnly,
        CancellationToken cancellationToken);
}

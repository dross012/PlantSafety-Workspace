// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the OSHACertifications read-model.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.Certification.OSHACertifications.Models;

namespace PlantSafety.Application.Compliance.Certification.OSHACertifications.Repositories;

/// <summary>Read-only repository for OSHACertifications data. Implemented in the Infrastructure layer.</summary>
public interface IOSHACertificationsRepository
{
    /// <summary>Returns OSHACertifications items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<OSHACertificationsItemDto>> GetOSHACertificationsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}

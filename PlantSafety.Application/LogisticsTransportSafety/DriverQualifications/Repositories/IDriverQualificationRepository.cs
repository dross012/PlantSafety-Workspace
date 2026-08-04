// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Repository abstraction for the DriverQualification read-model.
// SchemaVersion: 1

using PlantSafety.Application.LogisticsTransportSafety.DriverQualifications.Models;

namespace PlantSafety.Application.LogisticsTransportSafety.DriverQualifications.Repositories;

/// <summary>Read-only repository for DriverQualification data. Implemented in the Infrastructure layer.</summary>
public interface IDriverQualificationRepository
{
    /// <summary>Returns DriverQualification items for the given driver within the tenant.</summary>
    Task<IReadOnlyList<DriverQualificationItemDto>> GetDriverQualificationStatusAsync(
        Guid tenantId,
        Guid driverUserId,
        CancellationToken cancellationToken);
}

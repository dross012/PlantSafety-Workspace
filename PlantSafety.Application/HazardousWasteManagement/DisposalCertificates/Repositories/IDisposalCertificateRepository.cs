// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Repository abstraction for the DisposalCertificate read-model.
// SchemaVersion: 1

using PlantSafety.Application.HazardousWasteManagement.DisposalCertificates.Models;

namespace PlantSafety.Application.HazardousWasteManagement.DisposalCertificates.Repositories;

/// <summary>Read-only repository for DisposalCertificate data. Implemented in the Infrastructure layer.</summary>
public interface IDisposalCertificateRepository
{
    /// <summary>Returns the full disposal certificate history for a given WasteStream, ordered most-recent-first.</summary>
    Task<IReadOnlyList<DisposalCertificateItemDto>> GetDisposalHistoryByStreamAsync(
        Guid tenantId, Guid wasteStreamId, CancellationToken cancellationToken);
}

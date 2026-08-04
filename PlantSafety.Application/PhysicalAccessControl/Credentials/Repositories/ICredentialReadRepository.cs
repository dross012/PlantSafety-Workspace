// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Repository abstraction for the Credential status read-model.
// SchemaVersion: 1

using PlantSafety.Application.PhysicalAccessControl.Credentials.Models;

namespace PlantSafety.Application.PhysicalAccessControl.Credentials.Repositories;

/// <summary>Read-only repository for Credential status data. Implemented in the Infrastructure layer.</summary>
public interface ICredentialReadRepository
{
    /// <summary>Returns the status projection for a single Credential, scoped to the tenant, or null if not found.</summary>
    Task<CredentialStatusDto?> GetStatusAsync(Guid tenantId, Guid credentialId, CancellationToken cancellationToken);
}

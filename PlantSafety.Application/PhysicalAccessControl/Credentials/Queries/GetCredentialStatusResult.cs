// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Result payload for GetCredentialStatusQuery.
// SchemaVersion: 1

using PlantSafety.Application.PhysicalAccessControl.Credentials.Models;

namespace PlantSafety.Application.PhysicalAccessControl.Credentials.Queries;

/// <summary>Result of <see cref="GetCredentialStatusQuery"/>.</summary>
/// <param name="Credential">The matching Credential status projection, or null if not found.</param>
public sealed record GetCredentialStatusResult(CredentialStatusDto? Credential);

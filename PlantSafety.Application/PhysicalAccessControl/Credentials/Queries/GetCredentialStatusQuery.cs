// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Query to retrieve a Credential's current lifecycle status.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.PhysicalAccessControl.Credentials.Queries;

/// <summary>Retrieves the current lifecycle status of a single Credential, scoped to the tenant.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="CredentialId">Identity of the Credential to retrieve.</param>
public sealed record GetCredentialStatusQuery(
    Guid TenantId,
    Guid CredentialId) : IRequest<Result<GetCredentialStatusResult>>;

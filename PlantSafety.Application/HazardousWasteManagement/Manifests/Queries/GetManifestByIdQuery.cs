// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Query to retrieve a single HazardousWasteManifest by identifier.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.HazardousWasteManagement.Manifests.Queries;

/// <summary>Retrieves a single manifest read-model item by identifier.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="HazardousWasteManifestId">Identity of the manifest to retrieve.</param>
public sealed record GetManifestByIdQuery(
    Guid TenantId,
    Guid HazardousWasteManifestId) : IRequest<Result<GetManifestByIdResult>>;

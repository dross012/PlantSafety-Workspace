// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Result payload for GetManifestByIdQuery.
// SchemaVersion: 1

using PlantSafety.Application.HazardousWasteManagement.Manifests.Models;

namespace PlantSafety.Application.HazardousWasteManagement.Manifests.Queries;

/// <summary>Result of <see cref="GetManifestByIdQuery"/>.</summary>
/// <param name="Item">The matching manifest item, or null when not found.</param>
public sealed record GetManifestByIdResult(HazardousWasteManifestItemDto? Item);

// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Result payload for ListOpenManifestsQuery.
// SchemaVersion: 1

using PlantSafety.Application.HazardousWasteManagement.Manifests.Models;

namespace PlantSafety.Application.HazardousWasteManagement.Manifests.Queries;

/// <summary>Result of <see cref="ListOpenManifestsQuery"/>.</summary>
/// <param name="Items">Matching open manifest items.</param>
public sealed record ListOpenManifestsResult(IReadOnlyList<HazardousWasteManifestItemDto> Items);

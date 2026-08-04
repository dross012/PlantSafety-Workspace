// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-08-04
// FILE DESCRIPTION: Client-side DTO mirroring PlantSafety.Application.HazardousWasteManagement.
//   Manifests.Queries.ListOpenManifestsResult — the payload nested inside the server's
//   ApiResponse&lt;T&gt; envelope for GET api/v1/hazardouswastemanagement/manifests/open.
// SchemaVersion: 2026.2
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.4
// ============================================================

using System.Collections.Generic;

namespace PlantSafety.UI.Modules.HazardousWasteManagement.Models;

/// <summary>Response payload for the open-manifests read-model query.</summary>
public sealed class ListOpenManifestsResponseDto
{
    /// <summary>Gets the matching open manifest records. Never null; empty when no results.</summary>
    public IReadOnlyList<HazardousWasteManifestDto> Items { get; init; } = [];
}

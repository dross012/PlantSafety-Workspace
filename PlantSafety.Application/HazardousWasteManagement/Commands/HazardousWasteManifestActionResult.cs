// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Shared result payload for HazardousWasteManifest commands ("HazardousWasteManagement.Manifests").
// SchemaVersion: 1

namespace PlantSafety.Application.HazardousWasteManagement.Commands;

/// <summary>Result of a successful HazardousWasteManifest command execution.</summary>
/// <param name="HazardousWasteManifestId">Identity of the affected HazardousWasteManifest aggregate.</param>
/// <param name="Status">Lifecycle status of the record after the command was applied.</param>
public sealed record HazardousWasteManifestActionResult(
    Guid HazardousWasteManifestId,
    string Status);

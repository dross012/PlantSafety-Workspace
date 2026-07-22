// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Shared result payload for SiteOrientation lifecycle commands ("ContractorSafety.SiteOrientation").
// SchemaVersion: 1

namespace PlantSafety.Application.ContractorSafety.Commands;

/// <summary>Result of a successful SiteOrientation lifecycle command execution.</summary>
/// <param name="SiteOrientationId">Identity of the affected SiteOrientation aggregate.</param>
/// <param name="Status">Lifecycle status of the record after the command was applied.</param>
public sealed record SiteOrientationActionResult(
    Guid SiteOrientationId,
    string Status);
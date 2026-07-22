// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Shared result payload for PreQualificationVetting lifecycle commands ("ContractorSafety.PreQualificationVetting").
// SchemaVersion: 1

namespace PlantSafety.Application.ContractorSafety.Commands;

/// <summary>Result of a successful PreQualificationVetting lifecycle command execution.</summary>
/// <param name="PreQualificationVettingId">Identity of the affected PreQualificationVetting aggregate.</param>
/// <param name="Status">Lifecycle status of the record after the command was applied.</param>
public sealed record PreQualificationVettingActionResult(
    Guid PreQualificationVettingId,
    string Status);
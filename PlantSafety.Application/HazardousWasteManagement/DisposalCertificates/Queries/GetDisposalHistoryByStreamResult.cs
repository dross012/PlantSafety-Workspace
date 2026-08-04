// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Result payload for GetDisposalHistoryByStreamQuery.
// SchemaVersion: 1

using PlantSafety.Application.HazardousWasteManagement.DisposalCertificates.Models;

namespace PlantSafety.Application.HazardousWasteManagement.DisposalCertificates.Queries;

/// <summary>Result of <see cref="GetDisposalHistoryByStreamQuery"/>.</summary>
/// <param name="Items">Matching disposal certificate items, most-recent-first.</param>
public sealed record GetDisposalHistoryByStreamResult(IReadOnlyList<DisposalCertificateItemDto> Items);

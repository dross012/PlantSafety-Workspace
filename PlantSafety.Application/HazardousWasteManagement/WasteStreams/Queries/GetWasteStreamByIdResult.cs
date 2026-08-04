// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Result payload for GetWasteStreamByIdQuery.
// SchemaVersion: 1

using PlantSafety.Application.HazardousWasteManagement.WasteStreams.Models;

namespace PlantSafety.Application.HazardousWasteManagement.WasteStreams.Queries;

/// <summary>Result of <see cref="GetWasteStreamByIdQuery"/>.</summary>
/// <param name="Item">The matching WasteStream item, or null when not found.</param>
public sealed record GetWasteStreamByIdResult(WasteStreamItemDto? Item);

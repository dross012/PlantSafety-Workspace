// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Result payload for GetAccessPointByIdQuery.
// SchemaVersion: 1

using PlantSafety.Application.PhysicalAccessControl.AccessPoints.Models;

namespace PlantSafety.Application.PhysicalAccessControl.AccessPoints.Queries;

/// <summary>Result of <see cref="GetAccessPointByIdQuery"/>.</summary>
/// <param name="AccessPoint">The matching AccessPoint record, or null if not found.</param>
public sealed record GetAccessPointByIdResult(AccessPointDto? AccessPoint);

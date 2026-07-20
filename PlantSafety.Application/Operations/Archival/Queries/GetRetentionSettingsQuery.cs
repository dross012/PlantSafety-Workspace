// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Query to retrieve the current retention windows from Archival.TierPolicy.
// SchemaVersion: 1

using System.Collections.Generic;

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operations.Archival.Models;

namespace PlantSafety.Application.Operations.Archival.Queries;

/// <summary>Retrieves the current retention windows (read-only).</summary>
public sealed record GetRetentionSettingsQuery() : IRequest<Result<IReadOnlyList<TierPolicyDto>>>;

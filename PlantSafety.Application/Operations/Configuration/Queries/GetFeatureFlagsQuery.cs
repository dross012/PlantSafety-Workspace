// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Query to retrieve every feature flag from a single database's
//   [System].[FeatureFlag] table.
// SchemaVersion: 1

using System.Collections.Generic;

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operations.Configuration.Models;

namespace PlantSafety.Application.Operations.Configuration.Queries;

/// <summary>Retrieves every feature flag from a single database.</summary>
/// <param name="DatabaseKey">The logical database key (e.g. "ArchiveDB", "IntegrationDB").</param>
public sealed record GetFeatureFlagsQuery(string DatabaseKey) : IRequest<Result<IReadOnlyList<FeatureFlagDto>>>;

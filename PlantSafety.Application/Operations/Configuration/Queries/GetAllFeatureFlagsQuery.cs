// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Query to retrieve every feature flag across all known databases
//   (currently PlantSafety.ArchiveDB and PlantSafety.IntegrationDB).
// SchemaVersion: 1

using System.Collections.Generic;

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operations.Configuration.Models;

namespace PlantSafety.Application.Operations.Configuration.Queries;

/// <summary>Retrieves every feature flag across every known database.</summary>
public sealed record GetAllFeatureFlagsQuery : IRequest<Result<IReadOnlyList<FeatureFlagDto>>>;

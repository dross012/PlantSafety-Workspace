// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Command to toggle a single feature flag's IsEnabled value in a
//   specific database. A normal, safe, reversible UPDATE.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operations.Configuration.Models;

namespace PlantSafety.Application.Operations.Configuration.Commands;

/// <summary>Toggles a single feature flag's IsEnabled value in the given database.</summary>
/// <param name="DatabaseKey">The logical database key the flag lives in.</param>
/// <param name="FlagName">The flag to toggle.</param>
/// <param name="IsEnabled">The new IsEnabled value.</param>
public sealed record SetFeatureFlagEnabledCommand(string DatabaseKey, string FlagName, bool IsEnabled)
    : IRequest<Result<FeatureFlagDto>>;

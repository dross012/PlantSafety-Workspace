// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Phase 7 (§7.1) query to resolve a feature flag's final on/off outcome
//   across all 4 precedence levels (user > tenant > region > global) for a given evaluation
//   context.
// SchemaVersion: 1

using System;

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operations.Configuration.Models;

namespace PlantSafety.Application.Operations.Configuration.Queries;

/// <summary>Resolves a feature flag's final outcome for a given user/tenant/region context.</summary>
/// <param name="DatabaseKey">The logical database key the global flag lives in (e.g. "ArchiveDB").</param>
/// <param name="FeatureName">The feature flag name to resolve.</param>
/// <param name="TenantId">The tenant context to evaluate, or null if none.</param>
/// <param name="UserId">The user context to evaluate, or null if none.</param>
/// <param name="Region">The region context to evaluate, or null if none.</param>
public sealed record ResolveFeatureFlagQuery(
    string DatabaseKey,
    string FeatureName,
    Guid? TenantId,
    Guid? UserId,
    string? Region) : IRequest<Result<FeatureFlagResolutionResult>>;

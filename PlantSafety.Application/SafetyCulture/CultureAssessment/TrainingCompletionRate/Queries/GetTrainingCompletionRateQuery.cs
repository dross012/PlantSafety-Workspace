// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Query to retrieve TrainingCompletionRate (Training Completion Rate) read-model data for a tenant/site within a date range.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.SafetyCulture.CultureAssessment.TrainingCompletionRate.Queries;

/// <summary>
/// Retrieves TrainingCompletionRate data for the given tenant, optionally scoped to a single site,
/// within the inclusive UTC date range [<paramref name="From"/>, <paramref name="To"/>].
/// </summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site filter. Null returns data for all accessible sites.</param>
/// <param name="From">Inclusive start of the date range (UTC).</param>
/// <param name="To">Inclusive end of the date range (UTC).</param>
public sealed record GetTrainingCompletionRateQuery(
    Guid TenantId,
    Guid? SiteId,
    DateTime From,
    DateTime To) : IRequest<Result<TrainingCompletionRateQueryResult>>;
// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Query to retrieve a single consensus-standards requirement by its identity.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.Standards.Models;

namespace PlantSafety.Application.Compliance.Standards.Queries;

/// <summary>Retrieves a single consensus-standards requirement by its identity.</summary>
/// <param name="RequirementId">Identity of the requirement.</param>
public sealed record GetStandardsRequirementByIdQuery(
    Guid RequirementId) : IRequest<Result<StandardsRequirementDto>>;

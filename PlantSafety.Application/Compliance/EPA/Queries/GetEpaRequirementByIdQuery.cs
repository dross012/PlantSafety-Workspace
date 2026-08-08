// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Query to retrieve a single EPA regulatory requirement by its identity.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.EPA.Models;

namespace PlantSafety.Application.Compliance.EPA.Queries;

/// <summary>Retrieves a single EPA regulatory requirement by its identity.</summary>
/// <param name="RequirementId">Identity of the requirement.</param>
public sealed record GetEpaRequirementByIdQuery(
    Guid RequirementId) : IRequest<Result<EpaRegulatoryRequirementDto>>;

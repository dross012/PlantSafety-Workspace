// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Query to list consensus-standards equipment requirements, optionally
//   filtered by equipment type and/or parent requirement.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.Standards.Models;

namespace PlantSafety.Application.Compliance.Standards.Queries;

/// <summary>Lists Compliance.StandardsEquipmentRequirement rows, optionally filtered by equipment type and/or parent requirement.</summary>
/// <param name="EquipmentType">Optional exact equipment type filter.</param>
/// <param name="RequirementId">Optional exact parent requirement filter.</param>
public sealed record ListEquipmentRequirementsQuery(
    string? EquipmentType,
    Guid? RequirementId) : IRequest<Result<IReadOnlyList<EquipmentRequirementDto>>>;

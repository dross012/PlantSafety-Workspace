// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Query to retrieve every consensus-standards family row (ANSI, ASME, ISO).
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.Standards.Models;

namespace PlantSafety.Application.Compliance.Standards.Queries;

/// <summary>Retrieves every Compliance.StandardsFamily row.</summary>
public sealed record GetStandardsFamiliesQuery : IRequest<Result<IReadOnlyList<StandardsFamilyDto>>>;

// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Query to retrieve every EPA program row (RCRA, TSCA, CERCLA, CAA, CWA).
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.EPA.Models;

namespace PlantSafety.Application.Compliance.EPA.Queries;

/// <summary>Retrieves every Compliance.EpaProgram row.</summary>
public sealed record GetEpaProgramsQuery : IRequest<Result<IReadOnlyList<EpaProgramDto>>>;

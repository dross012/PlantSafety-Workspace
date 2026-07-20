// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Query to retrieve the unified System Health rollup report, fanning
//   out to every other Operations.* module's existing queries plus app version, DB
//   version, and Task Scheduler status.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operations.SystemHealth.Models;

namespace PlantSafety.Application.Operations.SystemHealth.Queries;

/// <summary>Retrieves the unified System Health rollup report.</summary>
public sealed record GetSystemHealthReportQuery : IRequest<Result<SystemHealthReportDto>>;

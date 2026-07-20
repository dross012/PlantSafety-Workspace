// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Query to retrieve the aggregated /health/ready readiness report via
//   the existing HealthCheckService, for the Operations.Monitoring dashboard.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operations.Monitoring.Models;

namespace PlantSafety.Application.Operations.Monitoring.Queries;

/// <summary>Retrieves the aggregated /health/ready readiness report.</summary>
public sealed record GetDatabaseHealthReportQuery : IRequest<Result<DatabaseHealthReportDto>>;

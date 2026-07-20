// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Query to derive per-database schema-drift status by comparing the
//   migration-script manifest (filesystem) against [System].[SchemaMigrations] (per DB).
// SchemaVersion: 1

using System.Collections.Generic;

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operations.SchemaGuard.Models;

namespace PlantSafety.Application.Operations.SchemaGuard.Queries;

/// <summary>Retrieves schema-drift status for every known PlantSafety.* database.</summary>
public sealed record GetSchemaHealthQuery : IRequest<Result<IReadOnlyList<SchemaDatabaseHealthDto>>>;

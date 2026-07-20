// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Query backing the "Run Migration" UI affordance. Returns deployment
//   instructions for every script missing from a database's [System].[SchemaMigrations]
//   table — it does NOT execute anything. See MigrationInstructionDto for the safety
//   rationale (mirrors Operations.Backup's restore-verify-only constraint).
// SchemaVersion: 1

using System.Collections.Generic;

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operations.SchemaGuard.Models;

namespace PlantSafety.Application.Operations.SchemaGuard.Queries;

/// <summary>Retrieves manual deployment instructions for every migration script missing from a database.</summary>
/// <param name="DatabaseKey">The database to check (e.g. "CoreDB").</param>
public sealed record GetMigrationInstructionsQuery(string DatabaseKey) : IRequest<Result<IReadOnlyList<MigrationInstructionDto>>>;

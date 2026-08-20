// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetMigrationInstructionsQuery. Read-only — never executes
//   any script. See MigrationInstructionDto for the safety rationale.
// SchemaVersion: 1

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operations.SchemaGuard.Models;
using PlantSafety.Application.Operations.SchemaGuard.Queries;
using PlantSafety.Application.Operations.SchemaGuard.Repositories;
using PlantSafety.Application.Operations.SchemaGuard.Services;

namespace PlantSafety.Application.Operations.SchemaGuard.Handlers;

/// <summary>Handles <see cref="GetMigrationInstructionsQuery"/>.</summary>
public sealed class GetMigrationInstructionsQueryHandler : IRequestHandler<GetMigrationInstructionsQuery, Result<IReadOnlyList<MigrationInstructionDto>>>
{
    private readonly ISchemaVersionRepository _schemaVersionRepository;
    private readonly IMigrationManifestProvider _manifestProvider;
    private readonly ILogger<GetMigrationInstructionsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetMigrationInstructionsQueryHandler(
        ISchemaVersionRepository schemaVersionRepository,
        IMigrationManifestProvider manifestProvider,
        ILogger<GetMigrationInstructionsQueryHandler> logger)
    {
        _schemaVersionRepository = schemaVersionRepository;
        _manifestProvider = manifestProvider;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<MigrationInstructionDto>>> Handle(GetMigrationInstructionsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var expectedScripts = await _manifestProvider.GetExpectedScriptNamesAsync(cancellationToken);
            var applied = await _schemaVersionRepository.GetAppliedMigrationsAsync(request.DatabaseKey, cancellationToken);

            var health = Services.SchemaDriftEvaluator.Evaluate(request.DatabaseKey, expectedScripts, applied);

            var instructions = new List<MigrationInstructionDto>();
            foreach (var scriptName in health.MissingScripts)
            {
                var path = await _manifestProvider.ResolveScriptPathAsync(scriptName, cancellationToken);

                var command = path is not null
                    ? $"sqlcmd -S (local)\\PLANTSAFETY -d PlantSafety.{request.DatabaseKey} -i \"{path}\""
                    : $"Script '{scriptName}' referenced in the manifest but not found on disk — verify the Migrations\\ folder tree.";

                instructions.Add(new MigrationInstructionDto(scriptName, path, command));
            }

            return Result<IReadOnlyList<MigrationInstructionDto>>.Success(instructions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to build migration instructions for {DatabaseKey}.", request.DatabaseKey);

            return Result<IReadOnlyList<MigrationInstructionDto>>.Failure(new ApplicationError(
                "Operations.SchemaGuard.InstructionsRetrievalFailed",
                "Unable to retrieve migration instructions at this time.",
                Guid.NewGuid()));
        }
    }
}

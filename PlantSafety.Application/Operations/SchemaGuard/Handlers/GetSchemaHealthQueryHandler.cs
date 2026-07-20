// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetSchemaHealthQuery.
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

/// <summary>Handles <see cref="GetSchemaHealthQuery"/>.</summary>
public sealed class GetSchemaHealthQueryHandler : IRequestHandler<GetSchemaHealthQuery, Result<IReadOnlyList<SchemaDatabaseHealthDto>>>
{
    private readonly ISchemaVersionRepository _schemaVersionRepository;
    private readonly IMigrationManifestProvider _manifestProvider;
    private readonly ILogger<GetSchemaHealthQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetSchemaHealthQueryHandler(
        ISchemaVersionRepository schemaVersionRepository,
        IMigrationManifestProvider manifestProvider,
        ILogger<GetSchemaHealthQueryHandler> logger)
    {
        _schemaVersionRepository = schemaVersionRepository;
        _manifestProvider = manifestProvider;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<SchemaDatabaseHealthDto>>> Handle(GetSchemaHealthQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var expectedScripts = await _manifestProvider.GetExpectedScriptNamesAsync(cancellationToken);

            var reports = new List<SchemaDatabaseHealthDto>();
            foreach (var databaseKey in _schemaVersionRepository.GetDatabaseKeys())
            {
                var applied = await _schemaVersionRepository.GetAppliedMigrationsAsync(databaseKey, cancellationToken);
                reports.Add(SchemaDriftEvaluator.Evaluate(databaseKey, expectedScripts, applied));

                if (applied is null)
                {
                    _logger.LogWarning("Schema drift check could not reach database {DatabaseKey}; reporting Unknown.", databaseKey);
                }
                else if (reports[^1].Status == "Drifted")
                {
                    _logger.LogWarning(
                        "Schema drift detected on {DatabaseKey}: {MissingCount} script(s) not applied: {MissingScripts}",
                        databaseKey, reports[^1].MissingScripts.Count, string.Join(", ", reports[^1].MissingScripts));
                }
            }

            return Result<IReadOnlyList<SchemaDatabaseHealthDto>>.Success(reports);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to evaluate schema health.");

            return Result<IReadOnlyList<SchemaDatabaseHealthDto>>.Failure(new ApplicationError(
                "Operations.SchemaGuard.HealthRetrievalFailed",
                "Unable to retrieve schema health at this time.",
                Guid.NewGuid()));
        }
    }
}

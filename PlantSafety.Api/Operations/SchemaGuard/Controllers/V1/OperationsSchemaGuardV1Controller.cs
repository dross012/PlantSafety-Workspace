// ============================================================================
// File: OperationsSchemaGuardV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-20
// Description: V1 controller surfacing schema-drift status by comparing the Migrations\
//   manifest against each database's [System].[SchemaMigrations] table (Phase 7
//   Governance). Routes all requests through MediatR ISender. No domain logic here.
//
//   SAFETY NOTE: There is deliberately no endpoint that executes a .sql migration script
//   against a live database from the API process. GET .../migration-instructions returns
//   the exact sqlcmd command an operator runs manually -- see MigrationInstructionDto.
// ============================================================================
using PlantSafety.Application.Operations.SchemaGuard.Models;
using PlantSafety.Application.Operations.SchemaGuard.Queries;

namespace PlantSafety.Api.Operations.SchemaGuard.Controllers.V1;

/// <summary>V1 controller for the Operations.SchemaGuard module.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/operations/schema-guard")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("Operations.SchemaGuard")]
public sealed class OperationsSchemaGuardV1Controller : ControllerBase
{
    private readonly ISender _sender;

    /// <summary>Initialises a new instance of <see cref="OperationsSchemaGuardV1Controller"/>.</summary>
    public OperationsSchemaGuardV1Controller(ISender sender) => _sender = sender;

    private string CorrelationId =>
        HttpContext.Items[PlantSafety.Api.Middleware.CorrelationIdMiddleware.ItemsKey]?.ToString() ?? string.Empty;

    /// <summary>Returns schema-drift status for every known PlantSafety.* database.</summary>
    [HttpGet("health")]
    [Authorize(Policy = SafetyTierPolicies.Admin)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<SchemaDatabaseHealthDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHealthAsync(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetSchemaHealthQuery(), cancellationToken);

        if (result.IsFailure)
            return BadRequest(ApiResponse<IReadOnlyList<SchemaDatabaseHealthDto>>.Fail(result.Error.Message, CorrelationId));

        return Ok(ApiResponse<IReadOnlyList<SchemaDatabaseHealthDto>>.Ok(result.Value, CorrelationId));
    }

    /// <summary>
    /// Returns manual deployment instructions (sqlcmd command, resolved script path) for
    /// every migration script missing on the given database. Read-only -- executes nothing.
    /// </summary>
    [HttpGet("{databaseKey}/migration-instructions")]
    [Authorize(Policy = SafetyTierPolicies.Admin)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<MigrationInstructionDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<MigrationInstructionDto>>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetMigrationInstructionsAsync(string databaseKey, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetMigrationInstructionsQuery(databaseKey), cancellationToken);

        if (result.IsFailure)
            return BadRequest(ApiResponse<IReadOnlyList<MigrationInstructionDto>>.Fail(result.Error.Message, CorrelationId));

        return Ok(ApiResponse<IReadOnlyList<MigrationInstructionDto>>.Ok(result.Value, CorrelationId));
    }
}

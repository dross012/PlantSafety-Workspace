// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-20
// FILE DESCRIPTION: Typed API client for the Operations.SchemaGuard module. Wraps HTTP
//   communication for PlantSafety.Api.Operations.SchemaGuard.Controllers.V1
//   .OperationsSchemaGuardV1Controller. GetMigrationInstructionsAsync returns manual
//   deployment instructions only -- there is no "run migration" call that executes a
//   script from the UI/API.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using PlantSafety.UI.Infrastructure.ApiClients;
using PlantSafety.UI.Models.Common;
using PlantSafety.UI.Modules.SchemaGuard.Models;

namespace PlantSafety.UI.Modules.SchemaGuard.ApiClients;

/// <summary>Typed API client for the Operations.SchemaGuard module.</summary>
public sealed class SchemaGuardApiClient : SafetyApiClientBase
{
    /// <summary>Initializes a new instance of <see cref="SchemaGuardApiClient"/>.</summary>
    public SchemaGuardApiClient(HttpClient http, ILogger<SchemaGuardApiClient> logger)
        : base(http, logger)
    {
    }

    /// <summary>Retrieves schema-drift status for every known PlantSafety.* database.</summary>
    public Task<ApiResponseEnvelope<IReadOnlyList<SchemaDatabaseHealthDto>>> GetHealthAsync(
        CancellationToken cancellationToken = default)
        => GetAsync<IReadOnlyList<SchemaDatabaseHealthDto>>("v1/operations/schema-guard/health", cancellationToken);

    /// <summary>Retrieves manual deployment instructions for every migration script missing on the given database.</summary>
    public Task<ApiResponseEnvelope<IReadOnlyList<MigrationInstructionDto>>> GetMigrationInstructionsAsync(
        string databaseKey,
        CancellationToken cancellationToken = default)
        => GetAsync<IReadOnlyList<MigrationInstructionDto>>(
            $"v1/operations/schema-guard/{Uri.EscapeDataString(databaseKey)}/migration-instructions",
            cancellationToken);
}

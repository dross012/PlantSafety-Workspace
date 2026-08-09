// ============================================================================
//  Project:        PlantSafety.UI
//  Module:         Modules.DataLake.Services
//  File:           DataLakeUiService.cs
//  Created:        2026-08-09
//  Description:    Calls PlantSafety.UI.Api's DataLakeController.
// ============================================================================
using PlantSafety.UI.Services;

namespace PlantSafety.UI.Modules.DataLake.Services;

/// <summary>The request body for writing a new data lake record.</summary>
public sealed record DataLakeWriteRequest(Guid SourceId, string Payload);

/// <summary>Calls PlantSafety.UI.Api's Data Lake endpoints.</summary>
public sealed class DataLakeUiService(PlantSafetyApiClient apiClient)
{
    /// <summary>Fetches the per-record-type count summary.</summary>
    public Task<Dictionary<string, int>?> GetCountsAsync(CancellationToken cancellationToken = default) =>
        apiClient.GetAsync<Dictionary<string, int>>("api/v1/ui/datalake/counts", cancellationToken);

    /// <summary>Writes a new record of the given type.</summary>
    public Task<string> WriteAsync(string recordType, Guid sourceId, string payload, CancellationToken cancellationToken = default) =>
        apiClient.PostRawJsonAsync($"api/v1/ui/datalake/{recordType}",
            System.Text.Json.JsonSerializer.Serialize(new DataLakeWriteRequest(sourceId, payload)), cancellationToken);
}

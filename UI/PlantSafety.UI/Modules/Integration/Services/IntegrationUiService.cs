// ============================================================================
//  Project:        PlantSafety.UI
//  Module:         Modules.Integration.Services
//  File:           IntegrationUiService.cs
//  Created:        2026-08-09
//  Description:    Calls Enterprise.Integration's reused
//                  EnterpriseIntegrationController (mounted as an
//                  ApplicationPart in PlantSafety.UI.Api).
// ============================================================================
using PlantSafety.UI.Services;

namespace PlantSafety.UI.Modules.Integration.Services;

/// <summary>Calls PlantSafety.UI.Api's reused Enterprise Integration ingest endpoint.</summary>
public sealed class IntegrationUiService(PlantSafetyApiClient apiClient)
{
    /// <summary>Ingests one inbound event from the given external system.</summary>
    public Task<string> IngestAsync(string system, string requestJson, CancellationToken cancellationToken = default) =>
        apiClient.PostRawJsonAsync($"api/v1/enterprise/integration/{system}/events", requestJson, cancellationToken);
}

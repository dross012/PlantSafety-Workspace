// ============================================================
// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Typed API client for the CyberPhysicalSecurity module. Wraps HTTP
//   communication with the real PlantSafety.Api endpoints under
//   api/v1/cyberphysicalsecurity/... (OtAssetsV1Controller, ControlChangeEventsV1Controller,
//   SabotageIndicatorsV1Controller). Calls gracefully fault through ApiResponseEnvelope so the UI
//   degrades to its Error state on backend failure.
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
using PlantSafety.UI.Modules.CyberPhysicalSecurity.Models;

namespace PlantSafety.UI.Modules.CyberPhysicalSecurity.ApiClients;

/// <summary>Typed API client for the CyberPhysicalSecurity module.</summary>
public sealed class CyberPhysicalSecurityApiClient : SafetyApiClientBase
{
    /// <summary>Initializes a new instance of <see cref="CyberPhysicalSecurityApiClient"/>.</summary>
    /// <param name="http">Typed HttpClient with correlation and auth handler pipeline.</param>
    /// <param name="logger">SIEM-safe structured logger.</param>
    public CyberPhysicalSecurityApiClient(HttpClient http, ILogger<CyberPhysicalSecurityApiClient> logger)
        : base(http, logger)
    {
    }

    /// <summary>Retrieves a single OT asset's current inventory record, feeding the OT Asset Security Status dashboard.</summary>
    public Task<ApiResponseEnvelope<OtAssetDto>> GetOtAssetByIdAsync(
        Guid otAssetId, CancellationToken cancellationToken = default)
        => GetAsync<OtAssetDto>($"v1/cyberphysicalsecurity/ot-assets/{otAssetId:D}", cancellationToken);

    /// <summary>Lists control-change events currently flagged unauthorized, feeding the Control-Change Audit Feed dashboard.</summary>
    public Task<ApiResponseEnvelope<IReadOnlyList<ControlChangeEventDto>>> ListUnauthorizedControlChangesAsync(
        CancellationToken cancellationToken = default)
        => GetAsync<IReadOnlyList<ControlChangeEventDto>>("v1/cyberphysicalsecurity/control-change-events/unauthorized", cancellationToken);

    /// <summary>Lists currently active (Open/UnderInvestigation) sabotage indicators, feeding the Active Sabotage Indicator Board.</summary>
    public Task<ApiResponseEnvelope<IReadOnlyList<SabotageIndicatorDto>>> ListActiveSabotageIndicatorsAsync(
        CancellationToken cancellationToken = default)
        => GetAsync<IReadOnlyList<SabotageIndicatorDto>>("v1/cyberphysicalsecurity/sabotage-indicators/active", cancellationToken);
}

// ============================================================================
//  Project:        Enterprise.Integration
//  Module:         Controllers
//  File:           EnterpriseIntegrationController.cs
//  Created:        2026-08-08
//  Description:    Phase D.9: receives inbound events from external
//                  enterprise systems (HR/ERP/MES/CMMS/EHS) and adapts
//                  them via the matching IEnterpriseIntegrationAdapter.
//                  IngestRequest is a small, controller-local DTO (not in
//                  Path D's original file list, but required to bind the
//                  POST body explicitly rather than accepting an untyped
//                  payload).
// ============================================================================
using Microsoft.AspNetCore.Mvc;

using Enterprise.Integration.Adapters;
using Enterprise.Integration.Events;

namespace Enterprise.Integration.Controllers;

/// <summary>The request body for an inbound integration event.</summary>
public sealed record EnterpriseIntegrationIngestRequest(
    string EventType,
    IReadOnlyDictionary<string, string> RawPayload);

/// <summary>Receives and adapts inbound events from external enterprise systems.</summary>
[ApiController]
[Route("api/v1/enterprise/integration")]
public sealed class EnterpriseIntegrationController(IReadOnlyDictionary<EnterpriseIntegrationSystem, IEnterpriseIntegrationAdapter> adapters) : ControllerBase
{
    /// <summary>POST api/v1/enterprise/integration/{system}/events -- ingests and adapts one inbound event from the given external system.</summary>
    [HttpPost("{system}/events")]
    public ActionResult<EnterpriseIntegrationEvent> IngestEvent(EnterpriseIntegrationSystem system, [FromBody] EnterpriseIntegrationIngestRequest request)
    {
        if (!adapters.TryGetValue(system, out IEnterpriseIntegrationAdapter? adapter))
        {
            return NotFound($"No integration adapter registered for system '{system}'.");
        }

        EnterpriseIntegrationEvent adapted = adapter.Adapt(request.EventType, request.RawPayload, DateTime.UtcNow);
        return Ok(adapted);
    }
}

// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Request body for investigating a StopWorkEvent over HTTP.

namespace PlantSafety.Api.SafetyCulture.Contracts.Requests;

/// <summary>Request body for <c>POST /api/v1/safetyculture/stop-work-events/{stopWorkEventId}/investigate</c>.</summary>
/// <param name="Findings">Narrative findings from the investigation.</param>
public sealed record InvestigateStopWorkEventRequest(string Findings);

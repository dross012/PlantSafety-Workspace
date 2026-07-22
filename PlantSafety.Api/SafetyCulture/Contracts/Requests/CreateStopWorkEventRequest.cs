// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Request body for creating a new StopWorkEvent over HTTP.

namespace PlantSafety.Api.SafetyCulture.Contracts.Requests;

/// <summary>Request body for <c>POST /api/v1/safetyculture/stop-work-events</c>.</summary>
/// <param name="SiteId">Optional site scope. Null applies tenant-wide.</param>
/// <param name="WorkArea">Work area where the stop-work event occurred.</param>
/// <param name="HazardDescription">Narrative description of the hazard that triggered the stop.</param>
public sealed record CreateStopWorkEventRequest(
    Guid? SiteId,
    string WorkArea,
    string HazardDescription);

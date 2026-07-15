// ============================================================================
// File: WorkerWellBeingResponse.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-15
// Description: External-facing response DTO for a single Worker WellBeing record.
//              Field names mirror PlantSafety.UI.Modules.WorkerWellBeing.Models.WorkerWellBeingDto
//              so the UI can deserialize the payload without remapping.
// ============================================================================

namespace PlantSafety.Api.Features.WorkerWellBeing.Records.Contracts.Responses;

/// <summary>Response payload for a single Worker WellBeing record.</summary>
public sealed record WorkerWellBeingResponse(
    Guid           WorkerWellBeingId,
    Guid           TenantId,
    Guid           WorkerId,
    string         WellBeingRiskLevel,
    string         Status,
    DateTimeOffset RecordedAtUtc);

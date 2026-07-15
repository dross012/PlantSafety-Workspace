// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Client-side DTO for a Worker WellBeing record. No
//   Application/Api layer exists yet for Worker WellBeing — only the
//   PlantSafety.Domain.WorkerWellBeing aggregate (WorkerWellBeingCreatedEvent)
//   has been scaffolded. This DTO is modeled against that aggregate's
//   identity/lifecycle shape so the UI is ready the moment a read-model
//   ships, and degrades gracefully (Loading/Error/Empty/Ready) until then.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.4
// ============================================================

using System;

namespace PlantSafety.UI.Modules.WorkerWellBeing.Models;

/// <summary>Read-side projection of a Worker WellBeing record (best-inferable shape — no backend read-model exists yet).</summary>
public sealed class WorkerWellBeingDto
{
    /// <summary>Gets the unique Worker WellBeing record identifier.</summary>
    public Guid WorkerWellBeingId { get; init; }

    /// <summary>Gets the owning tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the worker this record relates to.</summary>
    public Guid WorkerId { get; init; }

    /// <summary>Gets the wellbeing risk classification (e.g., Low/Medium/High).</summary>
    public string WellBeingRiskLevel { get; init; } = string.Empty;

    /// <summary>Gets the current workflow status.</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp when this record was last actioned.</summary>
    public DateTimeOffset RecordedAtUtc { get; init; }
}

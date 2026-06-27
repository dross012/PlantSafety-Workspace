// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-06-17
// FILE DESCRIPTION: Lightweight list-view DTO for Alert entities. Returned by
//   the paginated /api/v1/alerts list endpoint. Contains only the fields
//   required for list rendering — not the full alert detail.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.4
// ============================================================

using System;

namespace PlantSafety.UI.Modules.Alerts.Models;

/// <summary>
/// Lightweight alert list item for paginated list views.
/// </summary>
public sealed class AlertListItemDto
{
    /// <summary>Gets the unique alert identifier (UUIDv7).</summary>
    public Guid AlertId { get; init; }

    /// <summary>Gets the alert title.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Gets the severity code.</summary>
    public string Severity { get; init; } = string.Empty;

    /// <summary>Gets the status code.</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the source module code.</summary>
    public string SourceModule { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp when this alert was raised.</summary>
    public DateTimeOffset RaisedAtUtc { get; init; }

    /// <summary>Gets a value indicating whether this alert has been acknowledged.</summary>
    public bool IsAcknowledged { get; init; }
}

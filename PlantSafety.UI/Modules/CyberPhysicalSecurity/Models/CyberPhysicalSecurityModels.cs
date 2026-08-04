// ============================================================
// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Client-side DTOs for the CyberPhysicalSecurity module, mirroring the
//   PlantSafety.Api read-model shapes for OtAsset, ControlChangeEvent, and SabotageIndicator.
//   Display only.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;

namespace PlantSafety.UI.Modules.CyberPhysicalSecurity.Models;

/// <summary>A single OT (operational technology) asset inventory record.</summary>
public sealed class OtAssetDto
{
    /// <summary>Gets the asset record identifier.</summary>
    public Guid OtAssetId { get; init; }

    /// <summary>Gets the site identifier.</summary>
    public Guid SiteId { get; init; }

    /// <summary>Gets the asset tag.</summary>
    public string AssetTag { get; init; } = string.Empty;

    /// <summary>Gets the asset name.</summary>
    public string AssetName { get; init; } = string.Empty;

    /// <summary>Gets the asset type classification.</summary>
    public string AssetType { get; init; } = string.Empty;

    /// <summary>Gets the vendor.</summary>
    public string Vendor { get; init; } = string.Empty;

    /// <summary>Gets the model.</summary>
    public string Model { get; init; } = string.Empty;

    /// <summary>Gets the firmware version.</summary>
    public string FirmwareVersion { get; init; } = string.Empty;

    /// <summary>Gets the network zone.</summary>
    public string NetworkZone { get; init; } = string.Empty;

    /// <summary>Gets the criticality classification.</summary>
    public string Criticality { get; init; } = string.Empty;

    /// <summary>Gets the overall lifecycle status.</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp of the most recent inventory reconciliation, if any.</summary>
    public DateTime? LastReconciledAtUtc { get; init; }
}

/// <summary>A single OT control-system change observation.</summary>
public sealed class ControlChangeEventDto
{
    /// <summary>Gets the control-change event record identifier.</summary>
    public Guid ControlChangeEventId { get; init; }

    /// <summary>Gets the identity of the OT asset the change was observed on.</summary>
    public Guid OtAssetId { get; init; }

    /// <summary>Gets the free-text description of the change.</summary>
    public string ChangeDescription { get; init; } = string.Empty;

    /// <summary>Gets the changed parameter.</summary>
    public string ChangedParameter { get; init; } = string.Empty;

    /// <summary>Gets the prior value.</summary>
    public string OldValue { get; init; } = string.Empty;

    /// <summary>Gets the new value.</summary>
    public string NewValue { get; init; } = string.Empty;

    /// <summary>Gets the authorization status.</summary>
    public string AuthorizationStatus { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp the change was detected.</summary>
    public DateTime DetectedAtUtc { get; init; }
}

/// <summary>A single correlated sabotage indicator.</summary>
public sealed class SabotageIndicatorDto
{
    /// <summary>Gets the sabotage indicator record identifier.</summary>
    public Guid SabotageIndicatorId { get; init; }

    /// <summary>Gets the originating source of the indicator.</summary>
    public string Source { get; init; } = string.Empty;

    /// <summary>Gets the free-text description.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>Gets the severity classification.</summary>
    public string Severity { get; init; } = string.Empty;

    /// <summary>Gets the triage status.</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the number of other indicators this one has been correlated with.</summary>
    public int CorrelatedIndicatorCount { get; init; }

    /// <summary>Gets the UTC timestamp this indicator was raised.</summary>
    public DateTime RaisedAtUtc { get; init; }

    /// <summary>Gets the RiskManagement escalation priority.</summary>
    public string RiskManagementPriority { get; init; } = string.Empty;
}

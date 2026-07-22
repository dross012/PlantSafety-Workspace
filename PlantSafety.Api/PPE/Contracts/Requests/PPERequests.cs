// ============================================================================
// File: PPERequests.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-21
// Description: Request DTOs bound from the HTTP request body/query string for the PPE V1
//              controller (PPEAssignment, PPECategory, PPEInventory, PPETrainingFit,
//              PPEZoneMapping).
// ============================================================================

namespace PlantSafety.Api.PPE.Contracts.Requests;

/// <summary>Request body for flagging a PPEAssignment or PPEInventory item for replacement.</summary>
public sealed record FlagForReplacementRequest
{
    /// <summary>Reason the item is being flagged for replacement.</summary>
    public required string Reason { get; init; }
}

/// <summary>Request body for recording a PPE fit test result.</summary>
public sealed record RecordPPEFitTestRequest
{
    /// <summary>Whether the fit test passed.</summary>
    public required bool Passed { get; init; }

    /// <summary>Optional notes from the fit test.</summary>
    public string? Notes { get; init; }

    /// <summary>Optional UTC expiration of the fit test certification.</summary>
    public DateTime? FitTestExpiresAtUtc { get; init; }
}

/// <summary>Request body for receiving new stock into a PPEInventory item.</summary>
public sealed record ReceivePPEStockRequest
{
    /// <summary>Quantity of units received.</summary>
    public required int Quantity { get; init; }
}

/// <summary>Request body for issuing units of PPE out of a PPEInventory item's on-hand stock.</summary>
public sealed record IssuePPEUnitsRequest
{
    /// <summary>Quantity of units to issue.</summary>
    public required int Quantity { get; init; }

    /// <summary>Available balance at or below which the item is flagged LowStock.</summary>
    public required int ReorderThreshold { get; init; }
}

/// <summary>Request body for a bulk PPE operation.</summary>
public sealed record BulkUpdatePPERequest
{
    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Identifiers of the PPE resources to operate on. Maximum 500 per request.</summary>
    public required IReadOnlyList<Guid> ResourceIds { get; init; }

    /// <summary>The operation to apply (e.g. "Close", "Approve", "Reassign").</summary>
    public required string Operation { get; init; }

    /// <summary>Optional operation-specific key-value data.</summary>
    public Dictionary<string, string>? Parameters { get; init; }
}

/// <summary>Query-string bound filters for the PPE read-model list endpoints.</summary>
public sealed record PPEListRequest
{
    /// <summary>Optional site filter. Null returns data for all accessible sites.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Inclusive start of the date range (UTC). Defaults to 90 days before now.</summary>
    public DateTime? From { get; init; }

    /// <summary>Inclusive end of the date range (UTC). Defaults to 90 days after now.</summary>
    public DateTime? To { get; init; }
}

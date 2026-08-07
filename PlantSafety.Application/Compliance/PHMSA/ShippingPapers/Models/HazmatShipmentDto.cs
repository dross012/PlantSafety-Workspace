// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.ShippingPapers
//  Layer:          Application
//  File:           HazmatShipmentDto.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Flat read-model item DTO for a single HazmatShipment record. Backs both
//                  GetHazmatShipmentQuery and ListHazmatShipmentsQuery. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Models.PermitsItemDto's shape. The
//                  generated ShippingPaper and EmergencyResponseInfo owned value objects are
//                  flattened onto this DTO rather than nested, matching the flattened table shape
//                  in [Compliance].[PHMSA_HazmatShipment]. Reuses
//                  PlantSafety.Application.Compliance.PHMSA.Segregation.Models.
//                  SegregationViolationDto for the recorded segregation violations rather than
//                  introducing a duplicate shape.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.PHMSA.Segregation.Models;

namespace PlantSafety.Application.Compliance.PHMSA.ShippingPapers.Models;

/// <summary>A single HazmatShipment read-model record.</summary>
public sealed record HazmatShipmentDto
{
    /// <summary>Record identifier.</summary>
    public required Guid HazmatShipmentId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Current lifecycle status (Draft/Validated/Ready/Dispatched/Cancelled).</summary>
    public required string Status { get; init; }

    /// <summary>Classified materials contained in this shipment (Classification module).</summary>
    public IReadOnlyList<Guid> MaterialIds { get; init; } = [];

    /// <summary>Authorized packaging specifications used for this shipment (Packaging module).</summary>
    public IReadOnlyList<Guid> PackagingIds { get; init; } = [];

    /// <summary>Mode of transport for this shipment.</summary>
    public required string Mode { get; init; }

    /// <summary>Generated "Basic Description" sequence, null until a shipping paper has been attached.</summary>
    public string? ShippingPaperBasicDescription { get; init; }

    /// <summary>Reference to the accompanying emergency response information, null until attached.</summary>
    public string? ShippingPaperEmergencyResponseRef { get; init; }

    /// <summary>Shipper's certification statement text, null until attached.</summary>
    public string? ShippingPaperCertification { get; init; }

    /// <summary>Optional hazardous-waste manifest reference, when applicable.</summary>
    public string? ShippingPaperWasteManifestReference { get; init; }

    /// <summary>Modal-specific additional requirement notes.</summary>
    public IReadOnlyList<string> ShippingPaperAdditionalModalRequirements { get; init; } = [];

    /// <summary>UTC instant the shipping paper was generated, null until attached.</summary>
    public DateTime? ShippingPaperGeneratedAtUtc { get; init; }

    /// <summary>Emergency Response Guidebook guide number, null until the shipment is marked ready.</summary>
    public string? EmergencyResponseErgGuideNumber { get; init; }

    /// <summary>Emergency contact name, null until the shipment is marked ready.</summary>
    public string? EmergencyResponseContactName { get; init; }

    /// <summary>Emergency contact phone, null until the shipment is marked ready.</summary>
    public string? EmergencyResponseContactPhone { get; init; }

    /// <summary>Emergency response guidance summary, null until the shipment is marked ready.</summary>
    public string? EmergencyResponseGuidanceSummary { get; init; }

    /// <summary>Computed placard names for this shipment.</summary>
    public IReadOnlyList<string> PlacardingRequirements { get; init; } = [];

    /// <summary>Recorded segregation violations for this shipment.</summary>
    public IReadOnlyList<SegregationViolationDto> SegregationViolations { get; init; } = [];

    /// <summary>UTC timestamp when the record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public required Guid CreatedBy { get; init; }

    /// <summary>
    /// Optimistic-concurrency token (native RowVersion projected as a hex string). Callers must
    /// echo this value back on a subsequent mutating command's ETag field; a stale/mismatched
    /// value results in a concurrency-conflict error.
    /// </summary>
    public string? ETag { get; init; }
}

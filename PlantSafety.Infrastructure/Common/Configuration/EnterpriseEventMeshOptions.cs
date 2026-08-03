// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Common
//  Layer:          Infrastructure
//  File:           EnterpriseEventMeshOptions.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 6 §3.2 Cross-System Event Propagation, Eventing & Messaging Agent)
//  SchemaVersion:  2026.1
//  Description:    Strongly typed configuration for the Enterprise Event Mesh's outbound ERP/MES
//                  publishers. No live ERP/MES credentials exist in this environment -- every
//                  endpoint defaults to an empty string, and HttpExternalSystemPublisher gracefully
//                  no-ops (logs, does not fail) when its target endpoint is unset, matching this
//                  codebase's existing "optional, gracefully degrading" integration convention (see
//                  BlobStorageOptions defaulting to the Azurite emulator). Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Common.Configuration;

/// <summary>Strongly typed Enterprise Event Mesh outbound-publisher options.</summary>
public sealed class EnterpriseEventMeshOptions
{
    /// <summary>Configuration section key.</summary>
    public const string SectionName = "EnterpriseEventMesh";

    /// <summary>
    /// Base endpoint URL for the external ERP system's inbound webhook, or empty if not configured
    /// (the default in every environment this sandbox can exercise -- no live ERP endpoint exists).
    /// </summary>
    public string ErpEndpointUrl { get; init; } = string.Empty;

    /// <summary>Base endpoint URL for the external MES system's inbound webhook, or empty if not configured.</summary>
    public string MesEndpointUrl { get; init; } = string.Empty;

    /// <summary>
    /// Base endpoint URL for the external CMMS (Computerized Maintenance Management System)
    /// system's inbound webhook, or empty if not configured. Added Phase 6 §1.3 (CMMS
    /// Integration).
    /// </summary>
    public string CmmsEndpointUrl { get; init; } = string.Empty;

    /// <summary>
    /// Base endpoint URL for the external HR/Identity (HRIS) system's inbound webhook, or empty
    /// if not configured. Added Phase 6 §1.4 (HR/Identity Integration).
    /// </summary>
    public string HrEndpointUrl { get; init; } = string.Empty;

    /// <summary>
    /// Base endpoint URL for external third-party vendor systems' (IoT gateways, predictive
    /// maintenance vendors, external risk scoring) inbound webhook, or empty if not configured.
    /// Added Phase 6 §1.5 (Vendor API Integration).
    /// </summary>
    public string VendorEndpointUrl { get; init; } = string.Empty;

    /// <summary>Request timeout, in seconds, applied to outbound publish attempts.</summary>
    public int TimeoutSeconds { get; init; } = 10;
}

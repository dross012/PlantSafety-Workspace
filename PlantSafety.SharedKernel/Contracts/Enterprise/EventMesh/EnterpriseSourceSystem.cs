// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SharedKernel.Contracts.Enterprise.EventMesh
//  Layer:          SharedKernel
//  File:           EnterpriseSourceSystem.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 6 §3 Enterprise Event Mesh, Eventing & Messaging Agent)
//  SchemaVersion:  2026.1
//  Description:    Identifies the originating or target system of an <see cref="EnterpriseEventEnvelope"/>
//                  in the cross-SYSTEM (not just cross-module) integration boundary -- this platform
//                  itself versus an external Enterprise Resource Planning (ERP), Manufacturing
//                  Execution System (MES), or Computerized Maintenance Management System (CMMS).
//                  Additive-only evolution: new members may be appended; existing members MUST NOT
//                  be renamed or renumbered.
// ============================================================================
namespace PlantSafety.SharedKernel.Contracts.Enterprise.EventMesh;

/// <summary>
/// Identifies which system originated or is the target of an <see cref="EnterpriseEventEnvelope"/>
/// on the cross-system Enterprise Event Mesh boundary.
/// </summary>
public enum EnterpriseSourceSystem
{
    /// <summary>This PlantSafety platform itself.</summary>
    Platform = 0,

    /// <summary>An external Enterprise Resource Planning system.</summary>
    Erp = 1,

    /// <summary>An external Manufacturing Execution System.</summary>
    Mes = 2,

    /// <summary>An external Computerized Maintenance Management System.</summary>
    Cmms = 3,

    /// <summary>Any other external system not covered by the specific values above.</summary>
    External = 4,

    /// <summary>
    /// An external Human Resources / Identity system (HRIS) -- user provisioning, role/claim
    /// mapping, training and certification sync. Added Phase 6 §1.4 (Enterprise System
    /// Integrations, HR/Identity Integration). Additive-only evolution.
    /// </summary>
    Hr = 5,

    /// <summary>
    /// An external third-party vendor system not otherwise classified above -- IoT gateways,
    /// sensor platforms, predictive-maintenance vendors, and external risk-scoring systems. Added
    /// Phase 6 §1.5 (Enterprise System Integrations, Vendor API Integration). Additive-only
    /// evolution.
    /// </summary>
    Vendor = 6,
}

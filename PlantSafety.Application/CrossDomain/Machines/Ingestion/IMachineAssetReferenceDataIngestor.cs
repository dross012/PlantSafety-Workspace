// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CrossDomain.Machines
//  Layer:          Application
//  File:           IMachineAssetReferenceDataIngestor.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 6 §1.1 Enterprise System Integrations, ERP)
//  SchemaVersion:  2026.1
//  Description:    Reference-data ingestion contract for the Machines projection cache. CrossDomain
//                  is a projection-only bounded context (Handbook Part XXII): it owns no aggregates,
//                  emits no domain events, and accepts no CQRS Commands with business invariants --
//                  but it explicitly DOES cache normalized, versioned reference data supplied by
//                  inbound ETL/event-driven sync flows (Part XXII §4.1 "Inbound (External -&gt;
//                  PlantSafety)"). This interface is that inbound ingestion seam: an external ERP's
//                  asset-master-update event is normalized and upserted into the real Machines cache
//                  through this contract -- never through a fabricated CrossDomain "command" and
//                  never by reaching into Machines' Infrastructure/persistence directly from another
//                  module (Handbook Part IV §4.2.2/Part IX). Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.CrossDomain.Machines.Ingestion;

/// <summary>One normalized external asset-master update to upsert into the Machines reference-data cache.</summary>
/// <param name="MachineId">Stable identifier of the machine/asset (mirrors the external system's own asset identifier when mapped).</param>
/// <param name="TenantId">Tenant that owns this asset record.</param>
/// <param name="SiteId">Site this asset is scoped to, or null when not site-scoped.</param>
/// <param name="MachineName">Machine display name.</param>
/// <param name="MachineType">Machine type/class.</param>
/// <param name="SerialNumber">Manufacturer serial number.</param>
/// <param name="Status">Normalized current operating status.</param>
/// <param name="LastServicedAtUtc">UTC timestamp of the machine's last service, as reported by the external system.</param>
/// <param name="LastServicedBy">Identity of the technician who last serviced the machine, as reported by the external system.</param>
public sealed record ExternalMachineAssetUpdate(
    Guid MachineId,
    Guid TenantId,
    Guid? SiteId,
    string MachineName,
    string MachineType,
    string SerialNumber,
    string Status,
    DateTime LastServicedAtUtc,
    Guid LastServicedBy);

/// <summary>Ingests normalized external asset-master updates into the Machines reference-data cache.</summary>
public interface IMachineAssetReferenceDataIngestor
{
    /// <summary>
    /// Upserts <paramref name="update"/> into the Machines reference-data cache, replacing any
    /// existing cached record for the same <see cref="ExternalMachineAssetUpdate.MachineId"/>/
    /// <see cref="ExternalMachineAssetUpdate.TenantId"/> pair (idempotent by design -- safe to
    /// re-apply the same update on redelivery).
    /// </summary>
    Task UpsertAsync(ExternalMachineAssetUpdate update, CancellationToken cancellationToken);
}

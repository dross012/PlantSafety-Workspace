// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CrossDomain.Machines
//  Layer:          Infrastructure
//  File:           MachineAssetReferenceDataDapperIngestor.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 6 §1.1 Enterprise System Integrations, ERP)
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IMachineAssetReferenceDataIngestor: idempotent
//                  upsert into the [CrossDomain].[Machines] reference-data cache via
//                  usp_Machines_UpsertFromExternalSource. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.CrossDomain.Machines.Ingestion;

namespace PlantSafety.Infrastructure.CrossDomain.Machines.Persistence;

/// <summary>Dapper-backed <see cref="IMachineAssetReferenceDataIngestor"/> for the Machines reference-data cache.</summary>
public sealed class MachineAssetReferenceDataDapperIngestor(
    ISqlConnectionFactory connectionFactory,
    DapperCommandExecutor commandExecutor,
    ILogger<MachineAssetReferenceDataDapperIngestor> logger)
    : IMachineAssetReferenceDataIngestor
{
    /// <inheritdoc/>
    public async Task UpsertAsync(ExternalMachineAssetUpdate update, CancellationToken cancellationToken)
    {
        logger.LogDebug(
            "Enterprise.Integrations.Erp: upserting external asset-master update for Machine {MachineId} Tenant {TenantId}",
            update.MachineId, update.TenantId);

        using var conn = connectionFactory.CreateConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("MachineId", update.MachineId)
            .AddGuid("TenantId", update.TenantId)
            .AddGuid("SiteId", update.SiteId)
            .AddString("MachineName", update.MachineName, 200)
            .AddString("MachineType", update.MachineType, 100)
            .AddString("SerialNumber", update.SerialNumber, 100)
            .AddString("Status", update.Status, 50)
            .AddDateTime("LastServicedAtUtc", update.LastServicedAtUtc)
            .AddGuid("LastServicedBy", update.LastServicedBy)
            .Build();

        await commandExecutor.ExecuteAsync(
            conn, "[CrossDomain].[usp_Machines_UpsertFromExternalSource]", parameters, cancellationToken: cancellationToken);
    }
}

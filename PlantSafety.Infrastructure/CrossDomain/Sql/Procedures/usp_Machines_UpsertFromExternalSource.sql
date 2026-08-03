-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         CrossDomain.Machines
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_Machines_UpsertFromExternalSource.sql
--  Created:        2026-08-03
--  Author:         PlantSafety Platform Team (Phase 6 §1.1 Enterprise System Integrations, ERP)
--  SchemaVersion:  2026.1
--  Description:    Idempotent upsert procedure backing
--                  IMachineAssetReferenceDataIngestor.UpsertAsync -- the inbound ERP asset-master
--                  reference-data cache ingestion path for the projection-only CrossDomain.Machines
--                  bounded context (Handbook Part XXII). Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [CrossDomain].[usp_Machines_UpsertFromExternalSource]
    @MachineId          UNIQUEIDENTIFIER,
    @TenantId           UNIQUEIDENTIFIER,
    @SiteId             UNIQUEIDENTIFIER = NULL,
    @MachineName        NVARCHAR(200),
    @MachineType        NVARCHAR(100),
    @SerialNumber       NVARCHAR(100),
    @Status             NVARCHAR(50),
    @LastServicedAtUtc  DATETIME2(3),
    @LastServicedBy     UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;

MERGE [CrossDomain].[Machines] AS target
USING (SELECT @MachineId AS MachineId, @TenantId AS TenantId) AS source
    ON target.MachineId = source.MachineId AND target.TenantId = source.TenantId
WHEN MATCHED THEN
    UPDATE SET
        SiteId              = @SiteId,
        MachineName         = @MachineName,
        MachineType         = @MachineType,
        SerialNumber        = @SerialNumber,
        Status              = @Status,
        LastServicedAtUtc   = @LastServicedAtUtc,
        LastServicedBy      = @LastServicedBy
WHEN NOT MATCHED THEN
    INSERT (MachineId, TenantId, SiteId, MachineName, MachineType, SerialNumber, Status, LastServicedAtUtc, LastServicedBy)
    VALUES (@MachineId, @TenantId, @SiteId, @MachineName, @MachineType, @SerialNumber, @Status, @LastServicedAtUtc, @LastServicedBy);
GO

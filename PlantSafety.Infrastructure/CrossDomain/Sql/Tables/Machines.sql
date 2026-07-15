-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         CrossDomain.Machines
--  Layer:          Infrastructure - SQL Scripts
--  File:           Machines.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the Machines read-model, backing
--                  IMachinesRepository.GetMachinesAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Machines' AND schema_id = SCHEMA_ID('CrossDomain'))
BEGIN
    CREATE TABLE [CrossDomain].[Machines] (
        MachineId          UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId            UNIQUEIDENTIFIER NOT NULL,
        SiteId               UNIQUEIDENTIFIER NULL,
        MachineName         NVARCHAR(200)    NOT NULL,
        MachineType          NVARCHAR(100)    NOT NULL,
        SerialNumber         NVARCHAR(100)    NOT NULL,
        Status                NVARCHAR(50)     NOT NULL,
        LastServicedAtUtc    DATETIME2(3)     NOT NULL,
        LastServicedBy       UNIQUEIDENTIFIER NOT NULL
    );

    CREATE NONCLUSTERED INDEX IX_Machines_TenantId_SiteId ON [CrossDomain].[Machines] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX IX_Machines_LastServicedAtUtc ON [CrossDomain].[Machines] (LastServicedAtUtc);
END
GO

-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.FatigueManagement
--  Layer:          Infrastructure - SQL Scripts
--  File:           FatigueManagementReadModel.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the FatigueManagementReadModel read-model aggregate backing
--                  IFatigueManagementRepository.GetFatigueManagementAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'FatigueManagementReadModel' AND schema_id = SCHEMA_ID('Operational'))
BEGIN
    CREATE TABLE [Operational].[FatigueManagementReadModel] (
        [FatigueRecordId] UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]              UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                UNIQUEIDENTIFIER    NULL,
        [EmployeeId]              UNIQUEIDENTIFIER    NOT NULL,
        [FatigueRiskLevel]        NVARCHAR(400)       NOT NULL,
        [Status]                NVARCHAR(50)        NOT NULL,
        [RecordedAtUtc]   DATETIME2(3)        NOT NULL,
        [RecordedBy]      UNIQUEIDENTIFIER    NULL
    );

    CREATE NONCLUSTERED INDEX [IX_FatigueManagementReadModel_TenantId_RecordedAtUtc]
        ON [Operational].[FatigueManagementReadModel] ([TenantId], [RecordedAtUtc])
        INCLUDE ([SiteId]);
END
GO

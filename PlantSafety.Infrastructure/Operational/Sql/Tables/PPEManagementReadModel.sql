-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.PPEManagement
--  Layer:          Infrastructure - SQL Scripts
--  File:           PPEManagementReadModel.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the PPEManagementReadModel read-model aggregate backing
--                  IPPEManagementRepository.GetPPEManagementAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PPEManagementReadModel' AND schema_id = SCHEMA_ID('Operational'))
BEGIN
    CREATE TABLE [Operational].[PPEManagementReadModel] (
        [PPERecordId]     UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]              UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                UNIQUEIDENTIFIER    NULL,
        [EmployeeId]              UNIQUEIDENTIFIER    NOT NULL,
        [PPEType]                 NVARCHAR(400)       NOT NULL,
        [Status]                NVARCHAR(50)        NOT NULL,
        [IssuedAtUtc]     DATETIME2(3)        NOT NULL,
        [IssuedBy]        UNIQUEIDENTIFIER    NULL,
        RowVersion       ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_PPEManagementReadModel_TenantId_IssuedAtUtc]
        ON [Operational].[PPEManagementReadModel] ([TenantId], [IssuedAtUtc])
        INCLUDE ([SiteId]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Operational].[PPEManagementReadModel]') AND name = 'RowVersion')
BEGIN
    ALTER TABLE [Operational].[PPEManagementReadModel] ADD RowVersion ROWVERSION NOT NULL;
END
GO

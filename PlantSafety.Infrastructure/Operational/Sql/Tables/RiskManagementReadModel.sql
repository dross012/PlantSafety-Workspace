-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.RiskManagement
--  Layer:          Infrastructure - SQL Scripts
--  File:           RiskManagementReadModel.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the RiskManagementReadModel read-model aggregate backing
--                  IRiskManagementRepository.GetRiskManagementAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'RiskManagementReadModel' AND schema_id = SCHEMA_ID('Operational'))
BEGIN
    CREATE TABLE [Operational].[RiskManagementReadModel] (
        [RiskRecordId]    UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]              UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                UNIQUEIDENTIFIER    NULL,
        [RiskDescription]         NVARCHAR(400)       NOT NULL,
        [RiskLevel]               NVARCHAR(400)       NOT NULL,
        [Status]                NVARCHAR(50)        NOT NULL,
        [IdentifiedAtUtc] DATETIME2(3)        NOT NULL,
        [IdentifiedBy]    UNIQUEIDENTIFIER    NULL,
        RowVersion       ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_RiskManagementReadModel_TenantId_IdentifiedAtUtc]
        ON [Operational].[RiskManagementReadModel] ([TenantId], [IdentifiedAtUtc])
        INCLUDE ([SiteId]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Operational].[RiskManagementReadModel]') AND name = 'RowVersion')
BEGIN
    ALTER TABLE [Operational].[RiskManagementReadModel] ADD RowVersion ROWVERSION NOT NULL;
END
GO

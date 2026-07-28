-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.Contractor
--  Layer:          Infrastructure - SQL Scripts
--  File:           ContractorReadModel.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the ContractorReadModel read-model aggregate backing
--                  IContractorRepository.GetContractorAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ContractorReadModel' AND schema_id = SCHEMA_ID('Operational'))
BEGIN
    CREATE TABLE [Operational].[ContractorReadModel] (
        [ContractorRecordId] UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]              UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                UNIQUEIDENTIFIER    NULL,
        [ContractorName]          NVARCHAR(400)       NOT NULL,
        [ServiceType]             NVARCHAR(400)       NOT NULL,
        [Status]                NVARCHAR(50)        NOT NULL,
        [OnboardedAtUtc]  DATETIME2(3)        NOT NULL,
        [ApprovedBy]      UNIQUEIDENTIFIER    NULL,
        RowVersion       ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_ContractorReadModel_TenantId_OnboardedAtUtc]
        ON [Operational].[ContractorReadModel] ([TenantId], [OnboardedAtUtc])
        INCLUDE ([SiteId]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Operational].[ContractorReadModel]') AND name = 'RowVersion')
BEGIN
    ALTER TABLE [Operational].[ContractorReadModel] ADD RowVersion ROWVERSION NOT NULL;
END
GO

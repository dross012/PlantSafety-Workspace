-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.RenewalsTraining
--  Layer:          Infrastructure - SQL Scripts
--  File:           RenewalsTraining.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the RenewalsTraining read-model aggregate backing
--                  ITrainingRepository.GetRenewalsAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'RenewalsTraining' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[RenewalsTraining] (
        [RenewalId]               UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]                UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                  UNIQUEIDENTIFIER    NULL,
        [TrainingProgram]         NVARCHAR(300)       NOT NULL,
        [ExpiresAtUtc]            DATETIME2(3)        NOT NULL,
        [Status]                  NVARCHAR(50)        NOT NULL,
        [ResponsibleParty]        UNIQUEIDENTIFIER    NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_RenewalsTraining_TenantId_ExpiresAtUtc]
        ON [Compliance].[RenewalsTraining] ([TenantId], [ExpiresAtUtc])
        INCLUDE ([SiteId]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Compliance].[RenewalsTraining]') AND name = 'RowVersion')
BEGIN
    ALTER TABLE [Compliance].[RenewalsTraining] ADD RowVersion ROWVERSION NOT NULL;
END
GO
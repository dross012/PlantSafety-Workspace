-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.PermitsToWork
--  Layer:          Infrastructure - SQL Scripts
--  File:           PermitsToWorkReadModel.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the PermitsToWorkReadModel read-model aggregate backing
--                  IPermitsToWorkRepository.GetPermitsToWorkAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PermitsToWorkReadModel' AND schema_id = SCHEMA_ID('Operational'))
BEGIN
    CREATE TABLE [Operational].[PermitsToWorkReadModel] (
        [PermitId]        UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]              UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                UNIQUEIDENTIFIER    NULL,
        [PermitType]              NVARCHAR(400)       NOT NULL,
        [WorkDescription]         NVARCHAR(400)       NOT NULL,
        [Status]                NVARCHAR(50)        NOT NULL,
        [IssuedAtUtc]     DATETIME2(3)        NOT NULL,
        [IssuedBy]        UNIQUEIDENTIFIER    NULL
    );

    CREATE NONCLUSTERED INDEX [IX_PermitsToWorkReadModel_TenantId_IssuedAtUtc]
        ON [Operational].[PermitsToWorkReadModel] ([TenantId], [IssuedAtUtc])
        INCLUDE ([SiteId]);
END
GO

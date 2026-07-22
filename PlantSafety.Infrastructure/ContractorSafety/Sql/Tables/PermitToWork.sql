-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         ContractorSafety.PermitToWork
--  Layer:          Infrastructure - SQL Scripts
--  File:           PermitToWork.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the PermitToWork aggregate root backing IPermitToWorkRepository.
--                  TenantId is NOT NULL so this table is automatically picked up by the
--                  live-discovery Row-Level-Security policy in 90-row-level-security.sql
--                  (Security.fn_TenantAccessPredicate).
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'ContractorSafety')
BEGIN
    EXEC('CREATE SCHEMA [ContractorSafety]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PermitToWork' AND schema_id = SCHEMA_ID('ContractorSafety'))
BEGIN
    CREATE TABLE [ContractorSafety].[PermitToWork] (
        [PermitToWorkId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [SiteId] UNIQUEIDENTIFIER NULL,
        [Status] NVARCHAR(100) NOT NULL,
        [PermitTitle] NVARCHAR(400) NOT NULL,
        [PermitType] NVARCHAR(100) NOT NULL,
        [WorkLocation] NVARCHAR(400) NOT NULL,
        [HazardCount] INT NOT NULL CONSTRAINT DF_PermitToWork_HazardCount DEFAULT (0),
        [RequiresAtmosphericTesting] BIT NOT NULL CONSTRAINT DF_PermitToWork_RequiresAtmosphericTesting DEFAULT (0),
        [AtmosphericTestScore] DECIMAL(9,2) NULL,
        [AuthorizedByUserId] UNIQUEIDENTIFIER NOT NULL,
        [IssuedAtUtc] DATETIME2(3) NULL,
        [ExpiresAtUtc] DATETIME2(3) NULL,
        [SuspensionReason] NVARCHAR(2000) NULL,
        [CreatedAtUtc] DATETIME2(3) NOT NULL,
        [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
        [IsDeleted] BIT NOT NULL CONSTRAINT DF_PermitToWork_IsDeleted DEFAULT (0),
        [ModifiedAtUtc] DATETIME2(3) NULL,
        [ModifiedBy] UNIQUEIDENTIFIER NULL,
        [CorrelationId] UNIQUEIDENTIFIER NULL,
        [ActorId] UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_PermitToWork_TenantId_SiteId]
        ON [ContractorSafety].[PermitToWork] ([TenantId], [SiteId])
        INCLUDE ([Status], [PermitTitle]);
END
GO

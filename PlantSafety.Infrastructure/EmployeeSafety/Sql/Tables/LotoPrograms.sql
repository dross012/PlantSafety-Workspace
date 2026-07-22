-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         EmployeeSafety.LotoPrograms
--  Layer:          Infrastructure - SQL Scripts
--  File:           LotoPrograms.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the LotoPrograms aggregate root backing ILotoProgramsRepository.
--                  TenantId is NOT NULL so this table is automatically picked up by the
--                  live-discovery Row-Level-Security policy in 90-row-level-security.sql
--                  (Security.fn_TenantAccessPredicate).
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'EmployeeSafety')
BEGIN
    EXEC('CREATE SCHEMA [EmployeeSafety]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'LotoPrograms' AND schema_id = SCHEMA_ID('EmployeeSafety'))
BEGIN
    CREATE TABLE [EmployeeSafety].[LotoPrograms] (
        [LotoProgramsId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [SiteId] UNIQUEIDENTIFIER NULL,
        [Status] NVARCHAR(100) NOT NULL,
        [ProgramTitle] NVARCHAR(400) NOT NULL,
        [EquipmentTag] NVARCHAR(400) NOT NULL,
        [EnergySourceTypes] NVARCHAR(100) NOT NULL,
        [IsolationPointDescription] NVARCHAR(2000) NOT NULL,
        [IsolationPointCount] INT NOT NULL CONSTRAINT DF_LotoPrograms_IsolationPointCount DEFAULT (0),
        [AuthorizedByUserId] UNIQUEIDENTIFIER NOT NULL,
        [LocksAppliedCount] INT NOT NULL CONSTRAINT DF_LotoPrograms_LocksAppliedCount DEFAULT (0),
        [VerifiedByUserId] UNIQUEIDENTIFIER NULL,
        [VerifiedZeroEnergyAtUtc] DATETIME2(3) NULL,
        [ReleasedByUserId] UNIQUEIDENTIFIER NULL,
        [ReleasedAtUtc] DATETIME2(3) NULL,
        [LastAuditedAtUtc] DATETIME2(3) NULL,
        [ProgramComplianceScore] DECIMAL(9,2) NULL,
        [NonComplianceNotes] NVARCHAR(2000) NULL,
        [CreatedAtUtc] DATETIME2(3) NOT NULL,
        [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
        [IsDeleted] BIT NOT NULL CONSTRAINT DF_LotoPrograms_IsDeleted DEFAULT (0),
        [ModifiedAtUtc] DATETIME2(3) NULL,
        [ModifiedBy] UNIQUEIDENTIFIER NULL,
        [CorrelationId] UNIQUEIDENTIFIER NULL,
        [ActorId] UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_LotoPrograms_TenantId_SiteId]
        ON [EmployeeSafety].[LotoPrograms] ([TenantId], [SiteId])
        INCLUDE ([Status], [ProgramTitle]);
END
GO

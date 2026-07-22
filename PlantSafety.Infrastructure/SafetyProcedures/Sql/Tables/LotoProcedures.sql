-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyProcedures.LotoProcedures
--  Layer:          Infrastructure - SQL Scripts
--  File:           LotoProcedures.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the LotoProcedures aggregate root backing ILotoProceduresRepository.
--                  TenantId is NOT NULL so this table is automatically picked up by the
--                  live-discovery Row-Level-Security policy in 90-row-level-security.sql
--                  (Security.fn_TenantAccessPredicate).
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'SafetyProcedures')
BEGIN
    EXEC('CREATE SCHEMA [SafetyProcedures]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'LotoProcedures' AND schema_id = SCHEMA_ID('SafetyProcedures'))
BEGIN
    CREATE TABLE [SafetyProcedures].[LotoProcedures] (
        [LotoProceduresId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [SiteId] UNIQUEIDENTIFIER NULL,
        [Status] NVARCHAR(100) NOT NULL,
        [ProcedureTitle] NVARCHAR(400) NOT NULL,
        [EnergySourceTypes] NVARCHAR(100) NOT NULL,
        [IsolationPointDescription] NVARCHAR(2000) NOT NULL,
        [ApprovedByUserId] UNIQUEIDENTIFIER NOT NULL,
        [PublishedAtUtc] DATETIME2(3) NULL,
        [NextAuditDueAtUtc] DATETIME2(3) NULL,
        [AuthorizedPersonnelCount] INT NOT NULL CONSTRAINT DF_LotoProcedures_AuthorizedPersonnelCount DEFAULT (0),
        [ComplianceScore] DECIMAL(9,2) NULL,
        [CreatedAtUtc] DATETIME2(3) NOT NULL,
        [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
        [IsDeleted] BIT NOT NULL CONSTRAINT DF_LotoProcedures_IsDeleted DEFAULT (0),
        [ModifiedAtUtc] DATETIME2(3) NULL,
        [ModifiedBy] UNIQUEIDENTIFIER NULL,
        [CorrelationId] UNIQUEIDENTIFIER NULL,
        [ActorId] UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_LotoProcedures_TenantId_SiteId]
        ON [SafetyProcedures].[LotoProcedures] ([TenantId], [SiteId])
        INCLUDE ([Status], [ProcedureTitle]);
END
GO

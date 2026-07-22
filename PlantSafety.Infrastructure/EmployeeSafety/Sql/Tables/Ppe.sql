-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         EmployeeSafety.Ppe
--  Layer:          Infrastructure - SQL Scripts
--  File:           Ppe.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the Ppe aggregate root backing IPpeRepository.
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

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Ppe' AND schema_id = SCHEMA_ID('EmployeeSafety'))
BEGIN
    CREATE TABLE [EmployeeSafety].[Ppe] (
        [PpeId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [SiteId] UNIQUEIDENTIFIER NULL,
        [Status] NVARCHAR(100) NOT NULL,
        [ItemDescription] NVARCHAR(2000) NOT NULL,
        [Category] NVARCHAR(100) NOT NULL,
        [AssignedZone] NVARCHAR(400) NOT NULL,
        [IssuedToUserId] UNIQUEIDENTIFIER NOT NULL,
        [IssuedAtUtc] DATETIME2(3) NOT NULL,
        [RequiresFitTest] BIT NOT NULL CONSTRAINT DF_Ppe_RequiresFitTest DEFAULT (0),
        [FitTestedAtUtc] DATETIME2(3) NULL,
        [ReplacementCycleDays] INT NOT NULL CONSTRAINT DF_Ppe_ReplacementCycleDays DEFAULT (0),
        [NextReplacementDueAtUtc] DATETIME2(3) NULL,
        [LastInspectedAtUtc] DATETIME2(3) NULL,
        [ConditionScore] DECIMAL(9,2) NULL,
        [ReplacementReason] NVARCHAR(2000) NULL,
        [CreatedAtUtc] DATETIME2(3) NOT NULL,
        [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
        [IsDeleted] BIT NOT NULL CONSTRAINT DF_Ppe_IsDeleted DEFAULT (0),
        [ModifiedAtUtc] DATETIME2(3) NULL,
        [ModifiedBy] UNIQUEIDENTIFIER NULL,
        [CorrelationId] UNIQUEIDENTIFIER NULL,
        [ActorId] UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_Ppe_TenantId_SiteId]
        ON [EmployeeSafety].[Ppe] ([TenantId], [SiteId])
        INCLUDE ([Status], [ItemDescription]);
END
GO

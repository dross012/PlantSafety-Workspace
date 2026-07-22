-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyProcedures.PpePolicies
--  Layer:          Infrastructure - SQL Scripts
--  File:           PpePolicies.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the PpePolicies aggregate root backing IPpePoliciesRepository.
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

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PpePolicies' AND schema_id = SCHEMA_ID('SafetyProcedures'))
BEGIN
    CREATE TABLE [SafetyProcedures].[PpePolicies] (
        [PpePoliciesId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [SiteId] UNIQUEIDENTIFIER NULL,
        [Status] NVARCHAR(100) NOT NULL,
        [PolicyTitle] NVARCHAR(400) NOT NULL,
        [PpeCategory] NVARCHAR(100) NOT NULL,
        [AssignedZone] NVARCHAR(400) NOT NULL,
        [ApprovedByUserId] UNIQUEIDENTIFIER NOT NULL,
        [PublishedAtUtc] DATETIME2(3) NULL,
        [NextReviewDueAtUtc] DATETIME2(3) NULL,
        [ReplacementScheduleCount] INT NOT NULL CONSTRAINT DF_PpePolicies_ReplacementScheduleCount DEFAULT (0),
        [ComplianceScore] DECIMAL(9,2) NULL,
        [CreatedAtUtc] DATETIME2(3) NOT NULL,
        [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
        [IsDeleted] BIT NOT NULL CONSTRAINT DF_PpePolicies_IsDeleted DEFAULT (0),
        [ModifiedAtUtc] DATETIME2(3) NULL,
        [ModifiedBy] UNIQUEIDENTIFIER NULL,
        [CorrelationId] UNIQUEIDENTIFIER NULL,
        [ActorId] UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_PpePolicies_TenantId_SiteId]
        ON [SafetyProcedures].[PpePolicies] ([TenantId], [SiteId])
        INCLUDE ([Status], [PolicyTitle]);
END
GO

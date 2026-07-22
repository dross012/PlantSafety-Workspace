-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         EmployeeSafety.ErgonomicsAndMaterialHandling
--  Layer:          Infrastructure - SQL Scripts
--  File:           ErgonomicsAndMaterialHandling.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the ErgonomicsAndMaterialHandling aggregate root backing IErgonomicsAndMaterialHandlingRepository.
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

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ErgonomicsAndMaterialHandling' AND schema_id = SCHEMA_ID('EmployeeSafety'))
BEGIN
    CREATE TABLE [EmployeeSafety].[ErgonomicsAndMaterialHandling] (
        [ErgonomicsAndMaterialHandlingId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [SiteId] UNIQUEIDENTIFIER NULL,
        [Status] NVARCHAR(100) NOT NULL,
        [AssessmentTitle] NVARCHAR(400) NOT NULL,
        [HazardCategory] NVARCHAR(100) NOT NULL,
        [WorkstationLocation] NVARCHAR(400) NOT NULL,
        [AssessedByUserId] UNIQUEIDENTIFIER NOT NULL,
        [AssessedAtUtc] DATETIME2(3) NULL,
        [RiskFactorCount] INT NOT NULL CONSTRAINT DF_ErgonomicsAndMaterialHandling_RiskFactorCount DEFAULT (0),
        [RiskScore] DECIMAL(9,2) NULL,
        [ControlsDescription] NVARCHAR(2000) NULL,
        [FollowUpDueAtUtc] DATETIME2(3) NULL,
        [CreatedAtUtc] DATETIME2(3) NOT NULL,
        [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
        [IsDeleted] BIT NOT NULL CONSTRAINT DF_ErgonomicsAndMaterialHandling_IsDeleted DEFAULT (0),
        [ModifiedAtUtc] DATETIME2(3) NULL,
        [ModifiedBy] UNIQUEIDENTIFIER NULL,
        [CorrelationId] UNIQUEIDENTIFIER NULL,
        [ActorId] UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_ErgonomicsAndMaterialHandling_TenantId_SiteId]
        ON [EmployeeSafety].[ErgonomicsAndMaterialHandling] ([TenantId], [SiteId])
        INCLUDE ([Status], [AssessmentTitle]);
END
GO

-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyProcedures.ErgonomicsWorkstationDesign
--  Layer:          Infrastructure - SQL Scripts
--  File:           ErgonomicsWorkstationDesign.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the ErgonomicsWorkstationDesign aggregate root backing IErgonomicsWorkstationDesignRepository.
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

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ErgonomicsWorkstationDesign' AND schema_id = SCHEMA_ID('SafetyProcedures'))
BEGIN
    CREATE TABLE [SafetyProcedures].[ErgonomicsWorkstationDesign] (
        [ErgonomicsWorkstationDesignId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [SiteId] UNIQUEIDENTIFIER NULL,
        [Status] NVARCHAR(100) NOT NULL,
        [ProcedureTitle] NVARCHAR(400) NOT NULL,
        [DesignCategory] NVARCHAR(100) NOT NULL,
        [WorkstationLocation] NVARCHAR(400) NOT NULL,
        [ApprovedByUserId] UNIQUEIDENTIFIER NOT NULL,
        [AssessedAtUtc] DATETIME2(3) NULL,
        [NextAssessmentDueAtUtc] DATETIME2(3) NULL,
        [RiskFactorCount] INT NOT NULL CONSTRAINT DF_ErgonomicsWorkstationDesign_RiskFactorCount DEFAULT (0),
        [ErgonomicsScore] DECIMAL(9,2) NULL,
        [CreatedAtUtc] DATETIME2(3) NOT NULL,
        [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
        [IsDeleted] BIT NOT NULL CONSTRAINT DF_ErgonomicsWorkstationDesign_IsDeleted DEFAULT (0),
        [ModifiedAtUtc] DATETIME2(3) NULL,
        [ModifiedBy] UNIQUEIDENTIFIER NULL,
        [CorrelationId] UNIQUEIDENTIFIER NULL,
        [ActorId] UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_ErgonomicsWorkstationDesign_TenantId_SiteId]
        ON [SafetyProcedures].[ErgonomicsWorkstationDesign] ([TenantId], [SiteId])
        INCLUDE ([Status], [ProcedureTitle]);
END
GO

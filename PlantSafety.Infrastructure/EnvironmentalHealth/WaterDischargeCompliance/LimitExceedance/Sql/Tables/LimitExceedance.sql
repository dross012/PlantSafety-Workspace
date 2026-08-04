-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance
--  Layer:          Infrastructure - SQL Scripts
--  File:           LimitExceedance.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the LimitExceedance aggregate root backing ILimitExceedanceRepository.
--                  TenantId is NOT NULL so this table is automatically picked up by the
--                  live-discovery Row-Level-Security policy in 90-row-level-security.sql
--                  (Security.fn_TenantAccessPredicate). Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'EnvironmentalHealth')
BEGIN
    EXEC('CREATE SCHEMA [EnvironmentalHealth]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'LimitExceedance' AND schema_id = SCHEMA_ID('EnvironmentalHealth'))
BEGIN
    CREATE TABLE [EnvironmentalHealth].[LimitExceedance] (
        [LimitExceedanceId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [SiteId] UNIQUEIDENTIFIER NULL,
        [Status] NVARCHAR(50) NOT NULL,
        [DischargePointId] UNIQUEIDENTIFIER NOT NULL,
        [EffluentSampleId] UNIQUEIDENTIFIER NOT NULL,
        [DischargeLimitId] UNIQUEIDENTIFIER NOT NULL,
        [ParameterName] NVARCHAR(100) NOT NULL,
        [MeasuredValue] DECIMAL(18,4) NOT NULL,
        [LimitValue] DECIMAL(18,4) NOT NULL,
        [ExceedancePercentage] DECIMAL(9,2) NOT NULL,
        [Severity] NVARCHAR(50) NOT NULL,
        [DetectedAtUtc] DATETIME2(3) NOT NULL,
        [RepeatExceedanceCount] INT NOT NULL CONSTRAINT DF_LimitExceedance_RepeatExceedanceCount DEFAULT (0),
        [ResponseInitiatedAtUtc] DATETIME2(3) NULL,
        [ResponseInitiatedByUserId] UNIQUEIDENTIFIER NULL,
        [ResponseNotes] NVARCHAR(1000) NULL,
        [ClosedAtUtc] DATETIME2(3) NULL,
        [ClosedByUserId] UNIQUEIDENTIFIER NULL,
        [ResolutionNotes] NVARCHAR(1000) NULL,
        [CreatedAtUtc] DATETIME2(3) NOT NULL,
        [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
        [IsDeleted] BIT NOT NULL CONSTRAINT DF_LimitExceedance_IsDeleted DEFAULT (0),
        [ModifiedAtUtc] DATETIME2(3) NULL,
        [ModifiedBy] UNIQUEIDENTIFIER NULL,
        [CorrelationId] UNIQUEIDENTIFIER NULL,
        [ActorId] UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_LimitExceedance_TenantId_SiteId_Status]
        ON [EnvironmentalHealth].[LimitExceedance] ([TenantId], [SiteId], [Status]);
END
GO
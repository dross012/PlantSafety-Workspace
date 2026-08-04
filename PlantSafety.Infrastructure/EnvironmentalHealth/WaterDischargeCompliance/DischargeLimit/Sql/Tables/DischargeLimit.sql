-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         EnvironmentalHealth.WaterDischargeCompliance.DischargeLimit
--  Layer:          Infrastructure - SQL Scripts
--  File:           DischargeLimit.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the DischargeLimit aggregate root backing IDischargeLimitRepository.
--                  TenantId is NOT NULL so this table is automatically picked up by the
--                  live-discovery Row-Level-Security policy in 90-row-level-security.sql
--                  (Security.fn_TenantAccessPredicate). Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'EnvironmentalHealth')
BEGIN
    EXEC('CREATE SCHEMA [EnvironmentalHealth]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'DischargeLimit' AND schema_id = SCHEMA_ID('EnvironmentalHealth'))
BEGIN
    CREATE TABLE [EnvironmentalHealth].[DischargeLimit] (
        [DischargeLimitId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [SiteId] UNIQUEIDENTIFIER NULL,
        [Status] NVARCHAR(50) NOT NULL,
        [DischargePointId] UNIQUEIDENTIFIER NOT NULL,
        [DischargePermitId] UNIQUEIDENTIFIER NOT NULL,
        [Parameter] NVARCHAR(50) NOT NULL,
        [LimitBasis] NVARCHAR(50) NOT NULL,
        [LimitValue] DECIMAL(18,4) NOT NULL,
        [UnitOfMeasure] NVARCHAR(50) NOT NULL,
        [EffectiveAtUtc] DATETIME2(3) NOT NULL,
        [PermitConditionReference] NVARCHAR(200) NOT NULL,
        [RevisionCount] INT NOT NULL CONSTRAINT DF_DischargeLimit_RevisionCount DEFAULT (0),
        [CreatedAtUtc] DATETIME2(3) NOT NULL,
        [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
        [IsDeleted] BIT NOT NULL CONSTRAINT DF_DischargeLimit_IsDeleted DEFAULT (0),
        [ModifiedAtUtc] DATETIME2(3) NULL,
        [ModifiedBy] UNIQUEIDENTIFIER NULL,
        [CorrelationId] UNIQUEIDENTIFIER NULL,
        [ActorId] UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_DischargeLimit_TenantId_DischargePointId]
        ON [EnvironmentalHealth].[DischargeLimit] ([TenantId], [DischargePointId])
        INCLUDE ([Status], [Parameter]);
END
GO
-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         EnvironmentalHealth.WaterDischargeCompliance.EffluentSample
--  Layer:          Infrastructure - SQL Scripts
--  File:           EffluentSample.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the EffluentSample aggregate root backing IEffluentSampleRepository.
--                  TenantId is NOT NULL so this table is automatically picked up by the
--                  live-discovery Row-Level-Security policy in 90-row-level-security.sql
--                  (Security.fn_TenantAccessPredicate). Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'EnvironmentalHealth')
BEGIN
    EXEC('CREATE SCHEMA [EnvironmentalHealth]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'EffluentSample' AND schema_id = SCHEMA_ID('EnvironmentalHealth'))
BEGIN
    CREATE TABLE [EnvironmentalHealth].[EffluentSample] (
        [EffluentSampleId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [SiteId] UNIQUEIDENTIFIER NULL,
        [Status] NVARCHAR(50) NOT NULL,
        [DischargePointId] UNIQUEIDENTIFIER NOT NULL,
        [Parameter] NVARCHAR(50) NOT NULL,
        [MeasuredValue] DECIMAL(18,4) NOT NULL,
        [UnitOfMeasure] NVARCHAR(50) NOT NULL,
        [SampledAtUtc] DATETIME2(3) NOT NULL,
        [CollectedByUserId] UNIQUEIDENTIFIER NOT NULL,
        [LabSourceSystem] NVARCHAR(100) NOT NULL,
        [ChainOfCustodyReference] NVARCHAR(200) NOT NULL,
        [CorrectionOfSampleId] UNIQUEIDENTIFIER NULL,
        [CorrectionReason] NVARCHAR(500) NULL,
        [CreatedAtUtc] DATETIME2(3) NOT NULL,
        [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
        [IsDeleted] BIT NOT NULL CONSTRAINT DF_EffluentSample_IsDeleted DEFAULT (0),
        [ModifiedAtUtc] DATETIME2(3) NULL,
        [ModifiedBy] UNIQUEIDENTIFIER NULL,
        [CorrelationId] UNIQUEIDENTIFIER NULL,
        [ActorId] UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_EffluentSample_TenantId_DischargePointId]
        ON [EnvironmentalHealth].[EffluentSample] ([TenantId], [DischargePointId], [SampledAtUtc]);
END
GO
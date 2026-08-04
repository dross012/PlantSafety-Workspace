-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         EnvironmentalHealth.WaterDischargeCompliance.DischargePermit
--  Layer:          Infrastructure - SQL Scripts
--  File:           DischargePermit.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the DischargePermit aggregate root backing IDischargePermitRepository.
--                  TenantId is NOT NULL so this table is automatically picked up by the
--                  live-discovery Row-Level-Security policy in 90-row-level-security.sql
--                  (Security.fn_TenantAccessPredicate). Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'EnvironmentalHealth')
BEGIN
    EXEC('CREATE SCHEMA [EnvironmentalHealth]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'DischargePermit' AND schema_id = SCHEMA_ID('EnvironmentalHealth'))
BEGIN
    CREATE TABLE [EnvironmentalHealth].[DischargePermit] (
        [DischargePermitId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [SiteId] UNIQUEIDENTIFIER NULL,
        [Status] NVARCHAR(50) NOT NULL,
        [PermitNumber] NVARCHAR(100) NOT NULL,
        [RegulatoryProgram] NVARCHAR(50) NOT NULL,
        [IssuingAuthority] NVARCHAR(200) NOT NULL,
        [EffectiveAtUtc] DATETIME2(3) NOT NULL,
        [ExpiresAtUtc] DATETIME2(3) NOT NULL,
        [RenewalCount] INT NOT NULL CONSTRAINT DF_DischargePermit_RenewalCount DEFAULT (0),
        [PermitConditionSummary] NVARCHAR(1000) NOT NULL CONSTRAINT DF_DischargePermit_PermitConditionSummary DEFAULT (N''),
        [LifecycleActionReason] NVARCHAR(500) NULL,
        [CreatedAtUtc] DATETIME2(3) NOT NULL,
        [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
        [IsDeleted] BIT NOT NULL CONSTRAINT DF_DischargePermit_IsDeleted DEFAULT (0),
        [ModifiedAtUtc] DATETIME2(3) NULL,
        [ModifiedBy] UNIQUEIDENTIFIER NULL,
        [CorrelationId] UNIQUEIDENTIFIER NULL,
        [ActorId] UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_DischargePermit_TenantId_SiteId]
        ON [EnvironmentalHealth].[DischargePermit] ([TenantId], [SiteId])
        INCLUDE ([Status], [PermitNumber], [ExpiresAtUtc]);
END
GO
-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport
--  Layer:          Infrastructure - SQL Scripts
--  File:           DischargeMonitoringReport.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the DischargeMonitoringReport aggregate root backing IDischargeMonitoringReportRepository.
--                  TenantId is NOT NULL so this table is automatically picked up by the
--                  live-discovery Row-Level-Security policy in 90-row-level-security.sql
--                  (Security.fn_TenantAccessPredicate). Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'EnvironmentalHealth')
BEGIN
    EXEC('CREATE SCHEMA [EnvironmentalHealth]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'DischargeMonitoringReport' AND schema_id = SCHEMA_ID('EnvironmentalHealth'))
BEGIN
    CREATE TABLE [EnvironmentalHealth].[DischargeMonitoringReport] (
        [DischargeMonitoringReportId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [SiteId] UNIQUEIDENTIFIER NULL,
        [Status] NVARCHAR(50) NOT NULL,
        [DischargePermitId] UNIQUEIDENTIFIER NOT NULL,
        [ReportingPeriodStartUtc] DATETIME2(3) NOT NULL,
        [ReportingPeriodEndUtc] DATETIME2(3) NOT NULL,
        [DueAtUtc] DATETIME2(3) NOT NULL,
        [SubmittedAtUtc] DATETIME2(3) NULL,
        [SubmittedByUserId] UNIQUEIDENTIFIER NULL,
        [SamplesWithinLimitCount] INT NOT NULL CONSTRAINT DF_DischargeMonitoringReport_SamplesWithinLimitCount DEFAULT (0),
        [TotalSamplesCount] INT NOT NULL CONSTRAINT DF_DischargeMonitoringReport_TotalSamplesCount DEFAULT (0),
        [RetentionExpiresAtUtc] DATETIME2(3) NULL,
        [RejectionReason] NVARCHAR(500) NULL,
        [CreatedAtUtc] DATETIME2(3) NOT NULL,
        [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
        [IsDeleted] BIT NOT NULL CONSTRAINT DF_DischargeMonitoringReport_IsDeleted DEFAULT (0),
        [ModifiedAtUtc] DATETIME2(3) NULL,
        [ModifiedBy] UNIQUEIDENTIFIER NULL,
        [CorrelationId] UNIQUEIDENTIFIER NULL,
        [ActorId] UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_DischargeMonitoringReport_TenantId_Period]
        ON [EnvironmentalHealth].[DischargeMonitoringReport] ([TenantId], [ReportingPeriodStartUtc], [ReportingPeriodEndUtc])
        INCLUDE ([Status], [SiteId]);
END
GO
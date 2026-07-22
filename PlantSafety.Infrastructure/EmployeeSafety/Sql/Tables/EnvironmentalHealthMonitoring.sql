-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         EmployeeSafety.EnvironmentalHealthMonitoring
--  Layer:          Infrastructure - SQL Scripts
--  File:           EnvironmentalHealthMonitoring.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the EnvironmentalHealthMonitoring aggregate root backing IEnvironmentalHealthMonitoringRepository.
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

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'EnvironmentalHealthMonitoring' AND schema_id = SCHEMA_ID('EmployeeSafety'))
BEGIN
    CREATE TABLE [EmployeeSafety].[EnvironmentalHealthMonitoring] (
        [EnvironmentalHealthMonitoringId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [SiteId] UNIQUEIDENTIFIER NULL,
        [Status] NVARCHAR(100) NOT NULL,
        [MonitoringTitle] NVARCHAR(400) NOT NULL,
        [Category] NVARCHAR(100) NOT NULL,
        [MonitoringLocation] NVARCHAR(400) NOT NULL,
        [MonitoredByUserId] UNIQUEIDENTIFIER NOT NULL,
        [MeasuredAtUtc] DATETIME2(3) NULL,
        [NextMeasurementDueAtUtc] DATETIME2(3) NULL,
        [ExposureLevelScore] DECIMAL(9,2) NULL,
        [ExceedanceThreshold] DECIMAL(9,2) NOT NULL,
        [ExceedanceCount] INT NOT NULL CONSTRAINT DF_EnvironmentalHealthMonitoring_ExceedanceCount DEFAULT (0),
        [CorrectiveActionDescription] NVARCHAR(2000) NULL,
        [CreatedAtUtc] DATETIME2(3) NOT NULL,
        [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
        [IsDeleted] BIT NOT NULL CONSTRAINT DF_EnvironmentalHealthMonitoring_IsDeleted DEFAULT (0),
        [ModifiedAtUtc] DATETIME2(3) NULL,
        [ModifiedBy] UNIQUEIDENTIFIER NULL,
        [CorrelationId] UNIQUEIDENTIFIER NULL,
        [ActorId] UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_EnvironmentalHealthMonitoring_TenantId_SiteId]
        ON [EmployeeSafety].[EnvironmentalHealthMonitoring] ([TenantId], [SiteId])
        INCLUDE ([Status], [MonitoringTitle]);
END
GO

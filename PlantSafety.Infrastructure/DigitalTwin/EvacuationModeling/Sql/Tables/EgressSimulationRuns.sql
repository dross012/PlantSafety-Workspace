-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         DigitalTwin.EvacuationModeling.EgressSimulations
--  Layer:          Infrastructure - SQL Scripts
--  File:           EgressSimulationRuns.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the EgressSimulationRun aggregate root. Simulation data is
--                  retained 7 years per module data-governance rules -- no hard delete; retention
--                  purge jobs (out of scope here) must use IsDeleted soft-delete semantics only.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'DigitalTwin')
BEGIN
    EXEC('CREATE SCHEMA [DigitalTwin]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'EgressSimulationRuns' AND schema_id = SCHEMA_ID('DigitalTwin'))
BEGIN
    CREATE TABLE [DigitalTwin].[EgressSimulationRuns] (
        [EgressSimulationRunId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [SiteId] UNIQUEIDENTIFIER NULL,
        [Status] NVARCHAR(100) NOT NULL,
        [ZoneId] UNIQUEIDENTIFIER NOT NULL,
        [TwinModelVersion] NVARCHAR(100) NOT NULL,
        [TriggerReason] NVARCHAR(100) NOT NULL,
        [SimulatedEvacuationTimeSeconds] INT NULL,
        [PredictedPeakOccupancy] INT NULL,
        [LinkedDrillId] UNIQUEIDENTIFIER NULL,
        [StartedAtUtc] DATETIME2(3) NOT NULL,
        [CompletedAtUtc] DATETIME2(3) NULL,
        [CreatedAtUtc] DATETIME2(3) NOT NULL,
        [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
        [IsDeleted] BIT NOT NULL CONSTRAINT DF_EgressSimulationRuns_IsDeleted DEFAULT (0),
        [ModifiedAtUtc] DATETIME2(3) NULL,
        [ModifiedBy] UNIQUEIDENTIFIER NULL,
        [CorrelationId] UNIQUEIDENTIFIER NULL,
        [ActorId] UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_EgressSimulationRuns_TenantId_ZoneId_StartedAtUtc]
        ON [DigitalTwin].[EgressSimulationRuns] ([TenantId], [ZoneId], [StartedAtUtc] DESC)
        INCLUDE ([Status], [SimulatedEvacuationTimeSeconds]);
END
GO

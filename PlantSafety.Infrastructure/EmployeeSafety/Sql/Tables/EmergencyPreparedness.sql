-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         EmployeeSafety.EmergencyPreparedness
--  Layer:          Infrastructure - SQL Scripts
--  File:           EmergencyPreparedness.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the EmergencyPreparedness aggregate root backing IEmergencyPreparednessRepository.
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

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'EmergencyPreparedness' AND schema_id = SCHEMA_ID('EmployeeSafety'))
BEGIN
    CREATE TABLE [EmployeeSafety].[EmergencyPreparedness] (
        [EmergencyPreparednessId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [SiteId] UNIQUEIDENTIFIER NULL,
        [Status] NVARCHAR(100) NOT NULL,
        [PlanTitle] NVARCHAR(400) NOT NULL,
        [EmergencyType] NVARCHAR(100) NOT NULL,
        [EvacuationRoute] NVARCHAR(400) NOT NULL,
        [CoordinatedByUserId] UNIQUEIDENTIFIER NOT NULL,
        [DrillConductedAtUtc] DATETIME2(3) NULL,
        [NextDrillDueAtUtc] DATETIME2(3) NULL,
        [ParticipantCount] INT NOT NULL CONSTRAINT DF_EmergencyPreparedness_ParticipantCount DEFAULT (0),
        [ReadinessScore] DECIMAL(9,2) NULL,
        [ReadinessThreshold] DECIMAL(9,2) NOT NULL,
        [ReadinessGapNotes] NVARCHAR(2000) NULL,
        [CreatedAtUtc] DATETIME2(3) NOT NULL,
        [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
        [IsDeleted] BIT NOT NULL CONSTRAINT DF_EmergencyPreparedness_IsDeleted DEFAULT (0),
        [ModifiedAtUtc] DATETIME2(3) NULL,
        [ModifiedBy] UNIQUEIDENTIFIER NULL,
        [CorrelationId] UNIQUEIDENTIFIER NULL,
        [ActorId] UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_EmergencyPreparedness_TenantId_SiteId]
        ON [EmployeeSafety].[EmergencyPreparedness] ([TenantId], [SiteId])
        INCLUDE ([Status], [PlanTitle]);
END
GO

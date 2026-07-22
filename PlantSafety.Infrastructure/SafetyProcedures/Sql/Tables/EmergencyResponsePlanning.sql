-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyProcedures.EmergencyResponsePlanning
--  Layer:          Infrastructure - SQL Scripts
--  File:           EmergencyResponsePlanning.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the EmergencyResponsePlanning aggregate root backing IEmergencyResponsePlanningRepository.
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

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'EmergencyResponsePlanning' AND schema_id = SCHEMA_ID('SafetyProcedures'))
BEGIN
    CREATE TABLE [SafetyProcedures].[EmergencyResponsePlanning] (
        [EmergencyResponsePlanningId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [SiteId] UNIQUEIDENTIFIER NULL,
        [Status] NVARCHAR(100) NOT NULL,
        [PlanTitle] NVARCHAR(400) NOT NULL,
        [EmergencyType] NVARCHAR(100) NOT NULL,
        [EvacuationRoute] NVARCHAR(400) NOT NULL,
        [ApprovedByUserId] UNIQUEIDENTIFIER NOT NULL,
        [DrillConductedAtUtc] DATETIME2(3) NULL,
        [NextDrillDueAtUtc] DATETIME2(3) NULL,
        [ParticipantCount] INT NOT NULL CONSTRAINT DF_EmergencyResponsePlanning_ParticipantCount DEFAULT (0),
        [ReadinessScore] DECIMAL(9,2) NULL,
        [CreatedAtUtc] DATETIME2(3) NOT NULL,
        [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
        [IsDeleted] BIT NOT NULL CONSTRAINT DF_EmergencyResponsePlanning_IsDeleted DEFAULT (0),
        [ModifiedAtUtc] DATETIME2(3) NULL,
        [ModifiedBy] UNIQUEIDENTIFIER NULL,
        [CorrelationId] UNIQUEIDENTIFIER NULL,
        [ActorId] UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_EmergencyResponsePlanning_TenantId_SiteId]
        ON [SafetyProcedures].[EmergencyResponsePlanning] ([TenantId], [SiteId])
        INCLUDE ([Status], [PlanTitle]);
END
GO

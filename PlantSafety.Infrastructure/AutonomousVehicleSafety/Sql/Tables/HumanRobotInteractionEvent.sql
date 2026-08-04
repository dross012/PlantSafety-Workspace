-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         AutonomousVehicleSafety.HumanRobotInteractionEvent
--  Layer:          Infrastructure - SQL Scripts
--  File:           HumanRobotInteractionEvent.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the HumanRobotInteractionEvent aggregate root backing
--                  IHumanRobotInteractionEventRepository. TenantId is NOT NULL so this table is
--                  automatically picked up by the live-discovery Row-Level-Security policy in
--                  90-row-level-security.sql (Security.fn_TenantAccessPredicate).
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'AutonomousVehicleSafety')
BEGIN
    EXEC('CREATE SCHEMA [AutonomousVehicleSafety]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'HumanRobotInteractionEvent' AND schema_id = SCHEMA_ID('AutonomousVehicleSafety'))
BEGIN
    CREATE TABLE [AutonomousVehicleSafety].[HumanRobotInteractionEvent] (
        [HumanRobotInteractionEventId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [SiteId] UNIQUEIDENTIFIER NULL,
        [Status] NVARCHAR(100) NOT NULL,
        [AutonomousVehicleAssetId] UNIQUEIDENTIFIER NOT NULL,
        [InteractionType] NVARCHAR(100) NOT NULL,
        [SeverityLevel] NVARCHAR(100) NOT NULL,
        [DistanceMeters] DECIMAL(9,3) NULL,
        [LocationDescription] NVARCHAR(400) NOT NULL,
        [DetectedAtUtc] DATETIME2(3) NOT NULL,
        [ReviewedByUserId] UNIQUEIDENTIFIER NULL,
        [ReviewedAtUtc] DATETIME2(3) NULL,
        [CreatedAtUtc] DATETIME2(3) NOT NULL,
        [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
        [IsDeleted] BIT NOT NULL CONSTRAINT DF_HumanRobotInteractionEvent_IsDeleted DEFAULT (0),
        [ModifiedAtUtc] DATETIME2(3) NULL,
        [ModifiedBy] UNIQUEIDENTIFIER NULL,
        [CorrelationId] UNIQUEIDENTIFIER NULL,
        [ActorId] UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_HumanRobotInteractionEvent_TenantId_SiteId]
        ON [AutonomousVehicleSafety].[HumanRobotInteractionEvent] ([TenantId], [SiteId])
        INCLUDE ([Status], [InteractionType], [SeverityLevel], [DetectedAtUtc]);
END
GO

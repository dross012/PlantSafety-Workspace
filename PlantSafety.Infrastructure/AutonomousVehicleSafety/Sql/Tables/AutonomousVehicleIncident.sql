-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         AutonomousVehicleSafety.AutonomousVehicleIncident
--  Layer:          Infrastructure - SQL Scripts
--  File:           AutonomousVehicleIncident.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the AutonomousVehicleIncident aggregate root backing
--                  IAutonomousVehicleIncidentRepository. TenantId is NOT NULL so this table is
--                  automatically picked up by the live-discovery Row-Level-Security policy in
--                  90-row-level-security.sql (Security.fn_TenantAccessPredicate).
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'AutonomousVehicleSafety')
BEGIN
    EXEC('CREATE SCHEMA [AutonomousVehicleSafety]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AutonomousVehicleIncident' AND schema_id = SCHEMA_ID('AutonomousVehicleSafety'))
BEGIN
    CREATE TABLE [AutonomousVehicleSafety].[AutonomousVehicleIncident] (
        [AutonomousVehicleIncidentId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [SiteId] UNIQUEIDENTIFIER NULL,
        [Status] NVARCHAR(100) NOT NULL,
        [AutonomousVehicleAssetId] UNIQUEIDENTIFIER NOT NULL,
        [IncidentType] NVARCHAR(100) NOT NULL,
        [SeverityLevel] NVARCHAR(100) NOT NULL,
        [OccurredAtUtc] DATETIME2(3) NOT NULL,
        [DescriptionSummary] NVARCHAR(2000) NOT NULL,
        [InvestigationLeadUserId] UNIQUEIDENTIFIER NULL,
        [SuspensionTriggered] BIT NOT NULL CONSTRAINT DF_AutonomousVehicleIncident_SuspensionTriggered DEFAULT (0),
        [ClosedAtUtc] DATETIME2(3) NULL,
        [CreatedAtUtc] DATETIME2(3) NOT NULL,
        [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
        [IsDeleted] BIT NOT NULL CONSTRAINT DF_AutonomousVehicleIncident_IsDeleted DEFAULT (0),
        [ModifiedAtUtc] DATETIME2(3) NULL,
        [ModifiedBy] UNIQUEIDENTIFIER NULL,
        [CorrelationId] UNIQUEIDENTIFIER NULL,
        [ActorId] UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_AutonomousVehicleIncident_TenantId_AssetId]
        ON [AutonomousVehicleSafety].[AutonomousVehicleIncident] ([TenantId], [AutonomousVehicleAssetId])
        INCLUDE ([Status], [IncidentType], [SeverityLevel], [OccurredAtUtc]);
END
GO

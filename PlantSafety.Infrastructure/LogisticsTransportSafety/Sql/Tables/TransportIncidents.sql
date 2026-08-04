-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         LogisticsTransportSafety.TransportIncidents
--  Layer:          Infrastructure - SQL Scripts
--  File:           TransportIncidents.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team (Claude Code sql-database-skill agent)
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the TransportIncident aggregate root backing
--                  ITransportIncidentRepository (Domain write repository) and the
--                  Application-layer read-model repository of the same simple name. [TripId]
--                  (nullable -- an incident may be reported outside a formal trip) and
--                  [VehicleId] are intra-module references to
--                  [LogisticsTransportSafety].[TransportTrips] and [TransportVehicles]
--                  respectively; both are indexed rather than FK-constrained, matching the
--                  established convention in this same build batch (see TransportTrips.sql
--                  header). [DriverUserId] references an Identity-module user and is out of
--                  module scope for a FK. Column list is column-for-column with
--                  TransportIncidentDapperRepository.TransportIncidentRow. Additive-only
--                  evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'LogisticsTransportSafety')
BEGIN
    EXEC('CREATE SCHEMA [LogisticsTransportSafety]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'TransportIncidents' AND schema_id = SCHEMA_ID('LogisticsTransportSafety'))
BEGIN
    CREATE TABLE [LogisticsTransportSafety].[TransportIncidents] (
        [TransportIncidentId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [SiteId] UNIQUEIDENTIFIER NULL,
        [Status] NVARCHAR(50) NOT NULL,
        [TripId] UNIQUEIDENTIFIER NULL,
        [VehicleId] UNIQUEIDENTIFIER NOT NULL,
        [DriverUserId] UNIQUEIDENTIFIER NOT NULL,
        [IncidentType] NVARCHAR(50) NOT NULL,
        [Severity] NVARCHAR(30) NOT NULL,
        [OccurredAtUtc] DATETIME2(3) NULL,
        [Location] NVARCHAR(500) NOT NULL,
        [CreatedAtUtc] DATETIME2(3) NOT NULL,
        [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
        [IsDeleted] BIT NOT NULL CONSTRAINT DF_TransportIncidents_IsDeleted DEFAULT (0),
        [ModifiedAtUtc] DATETIME2(3) NULL,
        [ModifiedBy] UNIQUEIDENTIFIER NULL,
        [CorrelationId] UNIQUEIDENTIFIER NULL,
        [ActorId] UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_TransportIncidents_TripId]
        ON [LogisticsTransportSafety].[TransportIncidents] ([TripId])
        INCLUDE ([Status], [OccurredAtUtc]);

    CREATE NONCLUSTERED INDEX [IX_TransportIncidents_VehicleId]
        ON [LogisticsTransportSafety].[TransportIncidents] ([VehicleId])
        INCLUDE ([Status], [OccurredAtUtc]);

    CREATE NONCLUSTERED INDEX [IX_TransportIncidents_TenantId_SiteId_OccurredAtUtc]
        ON [LogisticsTransportSafety].[TransportIncidents] ([TenantId], [SiteId], [OccurredAtUtc])
        INCLUDE ([Status], [VehicleId], [DriverUserId], [IncidentType], [Severity], [Location]);
END
GO

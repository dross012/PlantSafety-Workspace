-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         LogisticsTransportSafety.TransportTrips
--  Layer:          Infrastructure - SQL Scripts
--  File:           TransportTrips.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team (Claude Code sql-database-skill agent)
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the TransportTrip aggregate root backing ITransportTripRepository
--                  (Domain write repository) and the Application-layer read-model repository of
--                  the same simple name. [VehicleId] is an intra-module reference to
--                  [LogisticsTransportSafety].[TransportVehicles]; it is intentionally indexed
--                  rather than FK-constrained (matching the established convention in this same
--                  build batch -- see e.g. HazardousWasteManagement.WasteContainers ->
--                  AccumulationAreas/WasteStreams) so that the six sibling table scripts under this
--                  folder remain independently, order-agnostically deployable with no cross-file
--                  sequencing requirement; every relationship column is still covered by a
--                  supporting index per create-fk-indexes. [DriverUserId] references an
--                  Identity-module user and is out of module scope for a FK regardless. The
--                  TransportTrip aggregate carries only Vehicle/Driver references -- no
--                  CarrierId or DriverQualificationId column exists on this aggregate (see gap
--                  note in the SQL delivery summary). Column list is column-for-column with
--                  TransportTripDapperRepository.TransportTripRow. Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'LogisticsTransportSafety')
BEGIN
    EXEC('CREATE SCHEMA [LogisticsTransportSafety]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'TransportTrips' AND schema_id = SCHEMA_ID('LogisticsTransportSafety'))
BEGIN
    CREATE TABLE [LogisticsTransportSafety].[TransportTrips] (
        [TransportTripId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [SiteId] UNIQUEIDENTIFIER NULL,
        [Status] NVARCHAR(50) NOT NULL,
        [VehicleId] UNIQUEIDENTIFIER NOT NULL,
        [DriverUserId] UNIQUEIDENTIFIER NOT NULL,
        [TripType] NVARCHAR(50) NOT NULL,
        [RouteDescription] NVARCHAR(1000) NOT NULL,
        [PlannedDepartureAtUtc] DATETIME2(3) NULL,
        [PlannedArrivalAtUtc] DATETIME2(3) NULL,
        [ActualDepartureAtUtc] DATETIME2(3) NULL,
        [ActualArrivalAtUtc] DATETIME2(3) NULL,
        [DistanceMiles] DECIMAL(9,2) NULL,
        [CreatedAtUtc] DATETIME2(3) NOT NULL,
        [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
        [IsDeleted] BIT NOT NULL CONSTRAINT DF_TransportTrips_IsDeleted DEFAULT (0),
        [ModifiedAtUtc] DATETIME2(3) NULL,
        [ModifiedBy] UNIQUEIDENTIFIER NULL,
        [CorrelationId] UNIQUEIDENTIFIER NULL,
        [ActorId] UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_TransportTrips_VehicleId]
        ON [LogisticsTransportSafety].[TransportTrips] ([VehicleId])
        INCLUDE ([Status], [PlannedDepartureAtUtc]);

    CREATE NONCLUSTERED INDEX [IX_TransportTrips_TenantId_SiteId_Status]
        ON [LogisticsTransportSafety].[TransportTrips] ([TenantId], [SiteId], [Status])
        INCLUDE ([VehicleId], [DriverUserId], [TripType], [RouteDescription], [PlannedDepartureAtUtc]);

    CREATE NONCLUSTERED INDEX [IX_TransportTrips_DriverUserId]
        ON [LogisticsTransportSafety].[TransportTrips] ([DriverUserId])
        INCLUDE ([Status], [PlannedDepartureAtUtc]);
END
GO

-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         LogisticsTransportSafety.LoadSecurementChecks
--  Layer:          Infrastructure - SQL Scripts
--  File:           LoadSecurementChecks.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team (Claude Code sql-database-skill agent)
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the LoadSecurementCheck aggregate root backing
--                  ILoadSecurementCheckRepository (Domain write repository) and the
--                  Application-layer read-model repository of the same simple name. [TripId]
--                  (nullable -- a check may be performed outside a formal trip) and [VehicleId]
--                  are intra-module references to [LogisticsTransportSafety].[TransportTrips] and
--                  [TransportVehicles] respectively; both are indexed rather than FK-constrained,
--                  matching the established convention in this same build batch (see
--                  TransportTrips.sql header) so all six sibling table scripts remain
--                  independently deployable. [CheckedByUserId] references an Identity-module user
--                  and is out of module scope for a FK. Column list is column-for-column with
--                  LoadSecurementCheckDapperRepository.LoadSecurementCheckRow. Additive-only
--                  evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'LogisticsTransportSafety')
BEGIN
    EXEC('CREATE SCHEMA [LogisticsTransportSafety]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'LoadSecurementChecks' AND schema_id = SCHEMA_ID('LogisticsTransportSafety'))
BEGIN
    CREATE TABLE [LogisticsTransportSafety].[LoadSecurementChecks] (
        [LoadSecurementCheckId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [SiteId] UNIQUEIDENTIFIER NULL,
        [Status] NVARCHAR(50) NOT NULL,
        [TripId] UNIQUEIDENTIFIER NULL,
        [VehicleId] UNIQUEIDENTIFIER NOT NULL,
        [CheckedByUserId] UNIQUEIDENTIFIER NOT NULL,
        [CheckedAtUtc] DATETIME2(3) NULL,
        [DeficiencyCount] INT NOT NULL CONSTRAINT DF_LoadSecurementChecks_DeficiencyCount DEFAULT (0),
        [SecurementScore] DECIMAL(5,2) NOT NULL CONSTRAINT DF_LoadSecurementChecks_SecurementScore DEFAULT (0),
        [CreatedAtUtc] DATETIME2(3) NOT NULL,
        [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
        [IsDeleted] BIT NOT NULL CONSTRAINT DF_LoadSecurementChecks_IsDeleted DEFAULT (0),
        [ModifiedAtUtc] DATETIME2(3) NULL,
        [ModifiedBy] UNIQUEIDENTIFIER NULL,
        [CorrelationId] UNIQUEIDENTIFIER NULL,
        [ActorId] UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_LoadSecurementChecks_TripId]
        ON [LogisticsTransportSafety].[LoadSecurementChecks] ([TripId])
        INCLUDE ([Status], [CheckedAtUtc]);

    CREATE NONCLUSTERED INDEX [IX_LoadSecurementChecks_VehicleId]
        ON [LogisticsTransportSafety].[LoadSecurementChecks] ([VehicleId])
        INCLUDE ([Status], [CheckedAtUtc]);

    CREATE NONCLUSTERED INDEX [IX_LoadSecurementChecks_TenantId_SiteId_CheckedAtUtc]
        ON [LogisticsTransportSafety].[LoadSecurementChecks] ([TenantId], [SiteId], [CheckedAtUtc])
        INCLUDE ([Status], [TripId], [VehicleId], [DeficiencyCount], [SecurementScore]);
END
GO

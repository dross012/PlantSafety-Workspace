// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.Setup
//  Layer:          IntegrationTests
//  File:           TestDatabaseFactory.LogisticsTransportSafety.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Partial-class extension of TestDatabaseFactory adding the
//                  LogisticsTransportSafety module schema (TransportVehicles, Carriers,
//                  DriverQualifications, TransportTrips, LoadSecurementChecks,
//                  TransportIncidents). Kept in its own file (rather than appended inline to the
//                  very large, heavily-concurrently-edited TestDatabaseFactory.cs) to avoid
//                  edit collisions with other parallel testing agents; TestDatabaseFactory was
//                  made `partial` to support this. Column lists and stored procedure bodies are
//                  transcribed verbatim from
//                  PlantSafety.Infrastructure\LogisticsTransportSafety\Sql\Tables\*.sql and
//                  PlantSafety.Infrastructure\LogisticsTransportSafety\Sql\Procedures\*.sql.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
namespace PlantSafety.IntegrationTests.Setup;

public static partial class TestDatabaseFactory
{
    /// <summary>
    /// Creates the LogisticsTransportSafety schema, its six aggregate tables, and all backing
    /// stored procedures against the provided Testcontainers connection string.
    /// </summary>
    public static async Task ApplyLogisticsTransportSafetySchemaAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken);

        await conn.ExecuteAsync("""
            IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'LogisticsTransportSafety')
                EXEC('CREATE SCHEMA [LogisticsTransportSafety]');
            """);

        // ---- Tables --------------------------------------------------------

        await conn.ExecuteAsync("""
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Carriers' AND schema_id = SCHEMA_ID('LogisticsTransportSafety'))
            BEGIN
                CREATE TABLE [LogisticsTransportSafety].[Carriers] (
                    [CarrierId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                    [TenantId] UNIQUEIDENTIFIER NOT NULL,
                    [SiteId] UNIQUEIDENTIFIER NULL,
                    [Status] NVARCHAR(50) NOT NULL,
                    [CarrierName] NVARCHAR(200) NOT NULL,
                    [CarrierType] NVARCHAR(50) NOT NULL,
                    [InsuranceExpiryAtUtc] DATETIME2(3) NULL,
                    [SafetyRatingScore] DECIMAL(5,2) NULL,
                    [QualifiedAtUtc] DATETIME2(3) NULL,
                    [SuspensionReason] NVARCHAR(1000) NULL,
                    [CreatedAtUtc] DATETIME2(3) NOT NULL,
                    [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
                    [IsDeleted] BIT NOT NULL CONSTRAINT DF_Carriers_IsDeleted DEFAULT (0),
                    [ModifiedAtUtc] DATETIME2(3) NULL,
                    [ModifiedBy] UNIQUEIDENTIFIER NULL,
                    [CorrelationId] UNIQUEIDENTIFIER NULL,
                    [ActorId] UNIQUEIDENTIFIER NULL
                );

                CREATE NONCLUSTERED INDEX [IX_Carriers_TenantId_SiteId]
                    ON [LogisticsTransportSafety].[Carriers] ([TenantId], [SiteId])
                    INCLUDE ([Status], [CarrierName], [CarrierType], [SafetyRatingScore]);

                CREATE NONCLUSTERED INDEX [IX_Carriers_TenantId_CreatedAtUtc]
                    ON [LogisticsTransportSafety].[Carriers] ([TenantId], [CreatedAtUtc])
                    INCLUDE ([SiteId], [Status], [CarrierName], [CarrierType], [SafetyRatingScore]);
            END
            """);

        await conn.ExecuteAsync("""
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'DriverQualifications' AND schema_id = SCHEMA_ID('LogisticsTransportSafety'))
            BEGIN
                CREATE TABLE [LogisticsTransportSafety].[DriverQualifications] (
                    [DriverQualificationId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                    [TenantId] UNIQUEIDENTIFIER NOT NULL,
                    [SiteId] UNIQUEIDENTIFIER NULL,
                    [Status] NVARCHAR(50) NOT NULL,
                    [DriverUserId] UNIQUEIDENTIFIER NOT NULL,
                    [QualificationType] NVARCHAR(50) NOT NULL,
                    [LicenseNumber] NVARCHAR(100) NOT NULL,
                    [IssuedAtUtc] DATETIME2(3) NULL,
                    [ExpiresAtUtc] DATETIME2(3) NULL,
                    [SuspensionReason] NVARCHAR(1000) NULL,
                    [CreatedAtUtc] DATETIME2(3) NOT NULL,
                    [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
                    [IsDeleted] BIT NOT NULL CONSTRAINT DF_DriverQualifications_IsDeleted DEFAULT (0),
                    [ModifiedAtUtc] DATETIME2(3) NULL,
                    [ModifiedBy] UNIQUEIDENTIFIER NULL,
                    [CorrelationId] UNIQUEIDENTIFIER NULL,
                    [ActorId] UNIQUEIDENTIFIER NULL
                );

                CREATE NONCLUSTERED INDEX [IX_DriverQualifications_TenantId_DriverUserId]
                    ON [LogisticsTransportSafety].[DriverQualifications] ([TenantId], [DriverUserId])
                    INCLUDE ([SiteId], [Status], [QualificationType], [LicenseNumber], [ExpiresAtUtc]);

                CREATE NONCLUSTERED INDEX [IX_DriverQualifications_TenantId_SiteId]
                    ON [LogisticsTransportSafety].[DriverQualifications] ([TenantId], [SiteId])
                    INCLUDE ([Status], [QualificationType]);
            END
            """);

        await conn.ExecuteAsync("""
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'TransportVehicles' AND schema_id = SCHEMA_ID('LogisticsTransportSafety'))
            BEGIN
                CREATE TABLE [LogisticsTransportSafety].[TransportVehicles] (
                    [TransportVehicleId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                    [TenantId] UNIQUEIDENTIFIER NOT NULL,
                    [SiteId] UNIQUEIDENTIFIER NULL,
                    [Status] NVARCHAR(50) NOT NULL,
                    [VehicleTitle] NVARCHAR(200) NOT NULL,
                    [VehicleType] NVARCHAR(50) NOT NULL,
                    [VehicleIdentifier] NVARCHAR(100) NOT NULL,
                    [LastInspectionAtUtc] DATETIME2(3) NULL,
                    [NextInspectionDueAtUtc] DATETIME2(3) NULL,
                    [LastInspectionScore] DECIMAL(5,2) NULL,
                    [CreatedAtUtc] DATETIME2(3) NOT NULL,
                    [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
                    [IsDeleted] BIT NOT NULL CONSTRAINT DF_TransportVehicles_IsDeleted DEFAULT (0),
                    [ModifiedAtUtc] DATETIME2(3) NULL,
                    [ModifiedBy] UNIQUEIDENTIFIER NULL,
                    [CorrelationId] UNIQUEIDENTIFIER NULL,
                    [ActorId] UNIQUEIDENTIFIER NULL
                );

                CREATE NONCLUSTERED INDEX [IX_TransportVehicles_TenantId_SiteId]
                    ON [LogisticsTransportSafety].[TransportVehicles] ([TenantId], [SiteId])
                    INCLUDE ([Status], [VehicleType], [VehicleIdentifier]);

                CREATE NONCLUSTERED INDEX [IX_TransportVehicles_TenantId_NextInspectionDueAtUtc]
                    ON [LogisticsTransportSafety].[TransportVehicles] ([TenantId], [NextInspectionDueAtUtc])
                    INCLUDE ([Status], [SiteId]);
            END
            """);

        await conn.ExecuteAsync("""
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
            """);

        await conn.ExecuteAsync("""
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
            """);

        await conn.ExecuteAsync("""
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
            """);

        // ---- Stored procedures: Carriers ------------------------------------

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [LogisticsTransportSafety].[usp_Carrier_Create]
                @CarrierId UNIQUEIDENTIFIER,
                @TenantId UNIQUEIDENTIFIER,
                @SiteId UNIQUEIDENTIFIER = NULL,
                @Status NVARCHAR(50),
                @CarrierName NVARCHAR(200),
                @CarrierType NVARCHAR(50),
                @InsuranceExpiryAtUtc DATETIME2(3) = NULL,
                @SafetyRatingScore DECIMAL(5,2) = NULL,
                @QualifiedAtUtc DATETIME2(3) = NULL,
                @SuspensionReason NVARCHAR(1000) = NULL,
                @CreatedAtUtc DATETIME2(3),
                @CreatedBy UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN TRY
                BEGIN TRANSACTION;
                    INSERT INTO [LogisticsTransportSafety].[Carriers]
                        ([CarrierId], [TenantId], [SiteId], [Status], [CarrierName], [CarrierType], [InsuranceExpiryAtUtc],
                         [SafetyRatingScore], [QualifiedAtUtc], [SuspensionReason], [CreatedAtUtc], [CreatedBy])
                    VALUES
                        (@CarrierId, @TenantId, @SiteId, @Status, @CarrierName, @CarrierType, @InsuranceExpiryAtUtc,
                         @SafetyRatingScore, @QualifiedAtUtc, @SuspensionReason, @CreatedAtUtc, @CreatedBy);
                COMMIT TRANSACTION;
            END TRY
            BEGIN CATCH
                IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
                THROW;
            END CATCH;
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [LogisticsTransportSafety].[usp_Carrier_GetAll]
            AS
            BEGIN
                SET NOCOUNT ON;

                SELECT
                    [CarrierId], [TenantId], [SiteId], [Status], [CarrierName], [CarrierType], [InsuranceExpiryAtUtc],
                    [SafetyRatingScore], [QualifiedAtUtc], [SuspensionReason], [CreatedAtUtc], [CreatedBy]
                FROM [LogisticsTransportSafety].[Carriers]
                WHERE ISNULL([IsDeleted], 0) = 0;
            END;
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [LogisticsTransportSafety].[usp_Carrier_GetById]
                @CarrierId UNIQUEIDENTIFIER
            AS
            BEGIN
                SET NOCOUNT ON;

                SELECT
                    [CarrierId], [TenantId], [SiteId], [Status], [CarrierName], [CarrierType], [InsuranceExpiryAtUtc],
                    [SafetyRatingScore], [QualifiedAtUtc], [SuspensionReason], [CreatedAtUtc], [CreatedBy]
                FROM [LogisticsTransportSafety].[Carriers]
                WHERE [CarrierId] = @CarrierId
                  AND ISNULL([IsDeleted], 0) = 0;
            END;
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [LogisticsTransportSafety].[usp_Carrier_GetItems]
                @TenantId UNIQUEIDENTIFIER,
                @SiteId UNIQUEIDENTIFIER = NULL,
                @FromUtc DATETIME2(3),
                @ToUtc DATETIME2(3)
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT
                    [CarrierId], [TenantId], [SiteId], [CarrierName], [CarrierType],
                    [Status] AS [CarrierStatus], [SafetyRatingScore], [CreatedAtUtc], [CreatedBy]
                FROM [LogisticsTransportSafety].[Carriers]
                WHERE [TenantId] = @TenantId
                  AND (@SiteId IS NULL OR [SiteId] = @SiteId)
                  AND [CreatedAtUtc] >= @FromUtc
                  AND [CreatedAtUtc] <= @ToUtc
                  AND ISNULL([IsDeleted], 0) = 0
                ORDER BY [CreatedAtUtc] DESC;
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [LogisticsTransportSafety].[usp_Carrier_Update]
                @NowUtc DATETIME2(3),
                @CarrierId UNIQUEIDENTIFIER,
                @TenantId UNIQUEIDENTIFIER,
                @SiteId UNIQUEIDENTIFIER = NULL,
                @Status NVARCHAR(50),
                @CarrierName NVARCHAR(200),
                @CarrierType NVARCHAR(50),
                @InsuranceExpiryAtUtc DATETIME2(3) = NULL,
                @SafetyRatingScore DECIMAL(5,2) = NULL,
                @QualifiedAtUtc DATETIME2(3) = NULL,
                @SuspensionReason NVARCHAR(1000) = NULL
            AS
            SET NOCOUNT ON;
            BEGIN TRY
                BEGIN TRANSACTION;
                    UPDATE [LogisticsTransportSafety].[Carriers]
                    SET
                        [SiteId] = @SiteId,
                        [Status] = @Status,
                        [CarrierName] = @CarrierName,
                        [CarrierType] = @CarrierType,
                        [InsuranceExpiryAtUtc] = @InsuranceExpiryAtUtc,
                        [SafetyRatingScore] = @SafetyRatingScore,
                        [QualifiedAtUtc] = @QualifiedAtUtc,
                        [SuspensionReason] = @SuspensionReason,
                        [ModifiedAtUtc] = @NowUtc
                    WHERE [CarrierId] = @CarrierId
                      AND [TenantId] = @TenantId
                      AND ISNULL([IsDeleted], 0) = 0;
                COMMIT TRANSACTION;
            END TRY
            BEGIN CATCH
                IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
                THROW;
            END CATCH;
            """);

        // ---- Stored procedures: DriverQualifications ------------------------

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [LogisticsTransportSafety].[usp_DriverQualification_Create]
                @DriverQualificationId UNIQUEIDENTIFIER,
                @TenantId UNIQUEIDENTIFIER,
                @SiteId UNIQUEIDENTIFIER = NULL,
                @Status NVARCHAR(50),
                @DriverUserId UNIQUEIDENTIFIER,
                @QualificationType NVARCHAR(50),
                @LicenseNumber NVARCHAR(100),
                @IssuedAtUtc DATETIME2(3) = NULL,
                @ExpiresAtUtc DATETIME2(3) = NULL,
                @SuspensionReason NVARCHAR(1000) = NULL,
                @CreatedAtUtc DATETIME2(3),
                @CreatedBy UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN TRY
                BEGIN TRANSACTION;
                    INSERT INTO [LogisticsTransportSafety].[DriverQualifications]
                        ([DriverQualificationId], [TenantId], [SiteId], [Status], [DriverUserId], [QualificationType],
                         [LicenseNumber], [IssuedAtUtc], [ExpiresAtUtc], [SuspensionReason], [CreatedAtUtc], [CreatedBy])
                    VALUES
                        (@DriverQualificationId, @TenantId, @SiteId, @Status, @DriverUserId, @QualificationType,
                         @LicenseNumber, @IssuedAtUtc, @ExpiresAtUtc, @SuspensionReason, @CreatedAtUtc, @CreatedBy);
                COMMIT TRANSACTION;
            END TRY
            BEGIN CATCH
                IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
                THROW;
            END CATCH;
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [LogisticsTransportSafety].[usp_DriverQualification_GetAll]
            AS
            BEGIN
                SET NOCOUNT ON;

                SELECT
                    [DriverQualificationId], [TenantId], [SiteId], [Status], [DriverUserId], [QualificationType],
                    [LicenseNumber], [IssuedAtUtc], [ExpiresAtUtc], [SuspensionReason], [CreatedAtUtc], [CreatedBy]
                FROM [LogisticsTransportSafety].[DriverQualifications]
                WHERE ISNULL([IsDeleted], 0) = 0;
            END;
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [LogisticsTransportSafety].[usp_DriverQualification_GetById]
                @DriverQualificationId UNIQUEIDENTIFIER
            AS
            BEGIN
                SET NOCOUNT ON;

                SELECT
                    [DriverQualificationId], [TenantId], [SiteId], [Status], [DriverUserId], [QualificationType],
                    [LicenseNumber], [IssuedAtUtc], [ExpiresAtUtc], [SuspensionReason], [CreatedAtUtc], [CreatedBy]
                FROM [LogisticsTransportSafety].[DriverQualifications]
                WHERE [DriverQualificationId] = @DriverQualificationId
                  AND ISNULL([IsDeleted], 0) = 0;
            END;
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [LogisticsTransportSafety].[usp_DriverQualification_GetStatusByDriver]
                @TenantId UNIQUEIDENTIFIER,
                @DriverUserId UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT
                    [DriverQualificationId], [TenantId], [SiteId], [DriverUserId], [QualificationType],
                    [Status] AS [QualificationStatus], [LicenseNumber], [ExpiresAtUtc], [CreatedAtUtc], [CreatedBy]
                FROM [LogisticsTransportSafety].[DriverQualifications]
                WHERE [TenantId] = @TenantId
                  AND [DriverUserId] = @DriverUserId
                  AND ISNULL([IsDeleted], 0) = 0
                ORDER BY [ExpiresAtUtc] ASC;
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [LogisticsTransportSafety].[usp_DriverQualification_Update]
                @NowUtc DATETIME2(3),
                @DriverQualificationId UNIQUEIDENTIFIER,
                @TenantId UNIQUEIDENTIFIER,
                @SiteId UNIQUEIDENTIFIER = NULL,
                @Status NVARCHAR(50),
                @SuspensionReason NVARCHAR(1000) = NULL
            AS
            SET NOCOUNT ON;
            BEGIN TRY
                BEGIN TRANSACTION;
                    UPDATE [LogisticsTransportSafety].[DriverQualifications]
                    SET
                        [SiteId] = @SiteId,
                        [Status] = @Status,
                        [SuspensionReason] = @SuspensionReason,
                        [ModifiedAtUtc] = @NowUtc
                    WHERE [DriverQualificationId] = @DriverQualificationId
                      AND [TenantId] = @TenantId
                      AND ISNULL([IsDeleted], 0) = 0;
                COMMIT TRANSACTION;
            END TRY
            BEGIN CATCH
                IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
                THROW;
            END CATCH;
            """);

        // ---- Stored procedures: TransportVehicles ---------------------------

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [LogisticsTransportSafety].[usp_TransportVehicle_Create]
                @TransportVehicleId UNIQUEIDENTIFIER,
                @TenantId UNIQUEIDENTIFIER,
                @SiteId UNIQUEIDENTIFIER = NULL,
                @Status NVARCHAR(50),
                @VehicleTitle NVARCHAR(200),
                @VehicleType NVARCHAR(50),
                @VehicleIdentifier NVARCHAR(100),
                @LastInspectionAtUtc DATETIME2(3) = NULL,
                @NextInspectionDueAtUtc DATETIME2(3) = NULL,
                @LastInspectionScore DECIMAL(5,2) = NULL,
                @CreatedAtUtc DATETIME2(3),
                @CreatedBy UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN TRY
                BEGIN TRANSACTION;
                    INSERT INTO [LogisticsTransportSafety].[TransportVehicles]
                        ([TransportVehicleId], [TenantId], [SiteId], [Status], [VehicleTitle], [VehicleType], [VehicleIdentifier],
                         [LastInspectionAtUtc], [NextInspectionDueAtUtc], [LastInspectionScore], [CreatedAtUtc], [CreatedBy])
                    VALUES
                        (@TransportVehicleId, @TenantId, @SiteId, @Status, @VehicleTitle, @VehicleType, @VehicleIdentifier,
                         @LastInspectionAtUtc, @NextInspectionDueAtUtc, @LastInspectionScore, @CreatedAtUtc, @CreatedBy);
                COMMIT TRANSACTION;
            END TRY
            BEGIN CATCH
                IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
                THROW;
            END CATCH;
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [LogisticsTransportSafety].[usp_TransportVehicle_GetAll]
            AS
            BEGIN
                SET NOCOUNT ON;

                SELECT
                    [TransportVehicleId], [TenantId], [SiteId], [Status], [VehicleTitle], [VehicleType], [VehicleIdentifier],
                    [LastInspectionAtUtc], [NextInspectionDueAtUtc], [LastInspectionScore], [CreatedAtUtc], [CreatedBy]
                FROM [LogisticsTransportSafety].[TransportVehicles]
                WHERE ISNULL([IsDeleted], 0) = 0;
            END;
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [LogisticsTransportSafety].[usp_TransportVehicle_GetById]
                @TransportVehicleId UNIQUEIDENTIFIER
            AS
            BEGIN
                SET NOCOUNT ON;

                SELECT
                    [TransportVehicleId], [TenantId], [SiteId], [Status], [VehicleTitle], [VehicleType], [VehicleIdentifier],
                    [LastInspectionAtUtc], [NextInspectionDueAtUtc], [LastInspectionScore], [CreatedAtUtc], [CreatedBy]
                FROM [LogisticsTransportSafety].[TransportVehicles]
                WHERE [TransportVehicleId] = @TransportVehicleId
                  AND ISNULL([IsDeleted], 0) = 0;
            END;
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [LogisticsTransportSafety].[usp_TransportVehicle_GetItem]
                @TenantId UNIQUEIDENTIFIER,
                @TransportVehicleId UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT
                    [TransportVehicleId], [TenantId], [SiteId], [VehicleTitle], [VehicleType], [VehicleIdentifier],
                    [Status] AS [VehicleStatus], [CreatedAtUtc], [CreatedBy]
                FROM [LogisticsTransportSafety].[TransportVehicles]
                WHERE [TenantId] = @TenantId
                  AND [TransportVehicleId] = @TransportVehicleId
                  AND ISNULL([IsDeleted], 0) = 0;
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [LogisticsTransportSafety].[usp_TransportVehicle_Update]
                @NowUtc DATETIME2(3),
                @TransportVehicleId UNIQUEIDENTIFIER,
                @TenantId UNIQUEIDENTIFIER,
                @SiteId UNIQUEIDENTIFIER = NULL,
                @Status NVARCHAR(50),
                @VehicleTitle NVARCHAR(200),
                @VehicleType NVARCHAR(50),
                @VehicleIdentifier NVARCHAR(100),
                @LastInspectionAtUtc DATETIME2(3) = NULL,
                @NextInspectionDueAtUtc DATETIME2(3) = NULL,
                @LastInspectionScore DECIMAL(5,2) = NULL
            AS
            SET NOCOUNT ON;
            BEGIN TRY
                BEGIN TRANSACTION;
                    UPDATE [LogisticsTransportSafety].[TransportVehicles]
                    SET
                        [SiteId] = @SiteId,
                        [Status] = @Status,
                        [VehicleTitle] = @VehicleTitle,
                        [VehicleType] = @VehicleType,
                        [VehicleIdentifier] = @VehicleIdentifier,
                        [LastInspectionAtUtc] = @LastInspectionAtUtc,
                        [NextInspectionDueAtUtc] = @NextInspectionDueAtUtc,
                        [LastInspectionScore] = @LastInspectionScore,
                        [ModifiedAtUtc] = @NowUtc
                    WHERE [TransportVehicleId] = @TransportVehicleId
                      AND [TenantId] = @TenantId
                      AND ISNULL([IsDeleted], 0) = 0;
                COMMIT TRANSACTION;
            END TRY
            BEGIN CATCH
                IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
                THROW;
            END CATCH;
            """);

        // ---- Stored procedures: TransportTrips -------------------------------

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [LogisticsTransportSafety].[usp_TransportTrip_Create]
                @TransportTripId UNIQUEIDENTIFIER,
                @TenantId UNIQUEIDENTIFIER,
                @SiteId UNIQUEIDENTIFIER = NULL,
                @Status NVARCHAR(50),
                @VehicleId UNIQUEIDENTIFIER,
                @DriverUserId UNIQUEIDENTIFIER,
                @TripType NVARCHAR(50),
                @RouteDescription NVARCHAR(1000),
                @PlannedDepartureAtUtc DATETIME2(3) = NULL,
                @PlannedArrivalAtUtc DATETIME2(3) = NULL,
                @CreatedAtUtc DATETIME2(3),
                @CreatedBy UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN TRY
                BEGIN TRANSACTION;
                    INSERT INTO [LogisticsTransportSafety].[TransportTrips]
                        ([TransportTripId], [TenantId], [SiteId], [Status], [VehicleId], [DriverUserId], [TripType],
                         [RouteDescription], [PlannedDepartureAtUtc], [PlannedArrivalAtUtc], [CreatedAtUtc], [CreatedBy])
                    VALUES
                        (@TransportTripId, @TenantId, @SiteId, @Status, @VehicleId, @DriverUserId, @TripType,
                         @RouteDescription, @PlannedDepartureAtUtc, @PlannedArrivalAtUtc, @CreatedAtUtc, @CreatedBy);
                COMMIT TRANSACTION;
            END TRY
            BEGIN CATCH
                IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
                THROW;
            END CATCH;
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [LogisticsTransportSafety].[usp_TransportTrip_GetAll]
            AS
            BEGIN
                SET NOCOUNT ON;

                SELECT
                    [TransportTripId], [TenantId], [SiteId], [Status], [VehicleId], [DriverUserId], [TripType],
                    [RouteDescription], [PlannedDepartureAtUtc], [PlannedArrivalAtUtc], [ActualDepartureAtUtc],
                    [ActualArrivalAtUtc], [DistanceMiles], [CreatedAtUtc], [CreatedBy]
                FROM [LogisticsTransportSafety].[TransportTrips]
                WHERE ISNULL([IsDeleted], 0) = 0;
            END;
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [LogisticsTransportSafety].[usp_TransportTrip_GetById]
                @TransportTripId UNIQUEIDENTIFIER
            AS
            BEGIN
                SET NOCOUNT ON;

                SELECT
                    [TransportTripId], [TenantId], [SiteId], [Status], [VehicleId], [DriverUserId], [TripType],
                    [RouteDescription], [PlannedDepartureAtUtc], [PlannedArrivalAtUtc], [ActualDepartureAtUtc],
                    [ActualArrivalAtUtc], [DistanceMiles], [CreatedAtUtc], [CreatedBy]
                FROM [LogisticsTransportSafety].[TransportTrips]
                WHERE [TransportTripId] = @TransportTripId
                  AND ISNULL([IsDeleted], 0) = 0;
            END;
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [LogisticsTransportSafety].[usp_TransportTrip_ListActive]
                @TenantId UNIQUEIDENTIFIER,
                @SiteId UNIQUEIDENTIFIER = NULL
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT
                    [TransportTripId], [TenantId], [SiteId], [VehicleId], [DriverUserId], [TripType],
                    [Status] AS [TripStatus], [RouteDescription], [PlannedDepartureAtUtc], [CreatedAtUtc], [CreatedBy]
                FROM [LogisticsTransportSafety].[TransportTrips]
                WHERE [TenantId] = @TenantId
                  AND (@SiteId IS NULL OR [SiteId] = @SiteId)
                  AND [Status] NOT IN (N'Completed', N'Cancelled')
                  AND ISNULL([IsDeleted], 0) = 0
                ORDER BY [PlannedDepartureAtUtc] ASC;
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [LogisticsTransportSafety].[usp_TransportTrip_Update]
                @NowUtc DATETIME2(3),
                @TransportTripId UNIQUEIDENTIFIER,
                @TenantId UNIQUEIDENTIFIER,
                @SiteId UNIQUEIDENTIFIER = NULL,
                @Status NVARCHAR(50),
                @ActualDepartureAtUtc DATETIME2(3) = NULL,
                @ActualArrivalAtUtc DATETIME2(3) = NULL,
                @DistanceMiles DECIMAL(9,2) = NULL
            AS
            SET NOCOUNT ON;
            BEGIN TRY
                BEGIN TRANSACTION;
                    UPDATE [LogisticsTransportSafety].[TransportTrips]
                    SET
                        [SiteId] = @SiteId,
                        [Status] = @Status,
                        [ActualDepartureAtUtc] = @ActualDepartureAtUtc,
                        [ActualArrivalAtUtc] = @ActualArrivalAtUtc,
                        [DistanceMiles] = @DistanceMiles,
                        [ModifiedAtUtc] = @NowUtc
                    WHERE [TransportTripId] = @TransportTripId
                      AND [TenantId] = @TenantId
                      AND ISNULL([IsDeleted], 0) = 0;
                COMMIT TRANSACTION;
            END TRY
            BEGIN CATCH
                IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
                THROW;
            END CATCH;
            """);

        // ---- Stored procedures: LoadSecurementChecks -------------------------

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [LogisticsTransportSafety].[usp_LoadSecurementCheck_Create]
                @LoadSecurementCheckId UNIQUEIDENTIFIER,
                @TenantId UNIQUEIDENTIFIER,
                @SiteId UNIQUEIDENTIFIER = NULL,
                @Status NVARCHAR(50),
                @TripId UNIQUEIDENTIFIER = NULL,
                @VehicleId UNIQUEIDENTIFIER,
                @CheckedByUserId UNIQUEIDENTIFIER,
                @CheckedAtUtc DATETIME2(3) = NULL,
                @DeficiencyCount INT,
                @SecurementScore DECIMAL(5,2),
                @CreatedAtUtc DATETIME2(3),
                @CreatedBy UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN TRY
                BEGIN TRANSACTION;
                    INSERT INTO [LogisticsTransportSafety].[LoadSecurementChecks]
                        ([LoadSecurementCheckId], [TenantId], [SiteId], [Status], [TripId], [VehicleId], [CheckedByUserId],
                         [CheckedAtUtc], [DeficiencyCount], [SecurementScore], [CreatedAtUtc], [CreatedBy])
                    VALUES
                        (@LoadSecurementCheckId, @TenantId, @SiteId, @Status, @TripId, @VehicleId, @CheckedByUserId,
                         @CheckedAtUtc, @DeficiencyCount, @SecurementScore, @CreatedAtUtc, @CreatedBy);
                COMMIT TRANSACTION;
            END TRY
            BEGIN CATCH
                IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
                THROW;
            END CATCH;
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [LogisticsTransportSafety].[usp_LoadSecurementCheck_GetAll]
            AS
            BEGIN
                SET NOCOUNT ON;

                SELECT
                    [LoadSecurementCheckId], [TenantId], [SiteId], [Status], [TripId], [VehicleId], [CheckedByUserId],
                    [CheckedAtUtc], [DeficiencyCount], [SecurementScore], [CreatedAtUtc], [CreatedBy]
                FROM [LogisticsTransportSafety].[LoadSecurementChecks]
                WHERE ISNULL([IsDeleted], 0) = 0;
            END;
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [LogisticsTransportSafety].[usp_LoadSecurementCheck_GetById]
                @LoadSecurementCheckId UNIQUEIDENTIFIER
            AS
            BEGIN
                SET NOCOUNT ON;

                SELECT
                    [LoadSecurementCheckId], [TenantId], [SiteId], [Status], [TripId], [VehicleId], [CheckedByUserId],
                    [CheckedAtUtc], [DeficiencyCount], [SecurementScore], [CreatedAtUtc], [CreatedBy]
                FROM [LogisticsTransportSafety].[LoadSecurementChecks]
                WHERE [LoadSecurementCheckId] = @LoadSecurementCheckId
                  AND ISNULL([IsDeleted], 0) = 0;
            END;
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [LogisticsTransportSafety].[usp_LoadSecurementCheck_ListFailures]
                @TenantId UNIQUEIDENTIFIER,
                @SiteId UNIQUEIDENTIFIER = NULL,
                @FromUtc DATETIME2(3),
                @ToUtc DATETIME2(3)
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT
                    [LoadSecurementCheckId], [TenantId], [SiteId], [TripId], [VehicleId],
                    [Status] AS [CheckStatus], [DeficiencyCount], [SecurementScore], [CheckedAtUtc], [CreatedAtUtc], [CreatedBy]
                FROM [LogisticsTransportSafety].[LoadSecurementChecks]
                WHERE [TenantId] = @TenantId
                  AND (@SiteId IS NULL OR [SiteId] = @SiteId)
                  AND [Status] <> N'Passed'
                  AND [CheckedAtUtc] >= @FromUtc
                  AND [CheckedAtUtc] <= @ToUtc
                  AND ISNULL([IsDeleted], 0) = 0
                ORDER BY [CheckedAtUtc] DESC;
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [LogisticsTransportSafety].[usp_LoadSecurementCheck_Update]
                @NowUtc DATETIME2(3),
                @LoadSecurementCheckId UNIQUEIDENTIFIER,
                @TenantId UNIQUEIDENTIFIER,
                @SiteId UNIQUEIDENTIFIER = NULL,
                @Status NVARCHAR(50)
            AS
            SET NOCOUNT ON;
            BEGIN TRY
                BEGIN TRANSACTION;
                    UPDATE [LogisticsTransportSafety].[LoadSecurementChecks]
                    SET
                        [SiteId] = @SiteId,
                        [Status] = @Status,
                        [ModifiedAtUtc] = @NowUtc
                    WHERE [LoadSecurementCheckId] = @LoadSecurementCheckId
                      AND [TenantId] = @TenantId
                      AND ISNULL([IsDeleted], 0) = 0;
                COMMIT TRANSACTION;
            END TRY
            BEGIN CATCH
                IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
                THROW;
            END CATCH;
            """);

        // ---- Stored procedures: TransportIncidents ---------------------------

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [LogisticsTransportSafety].[usp_TransportIncident_Create]
                @TransportIncidentId UNIQUEIDENTIFIER,
                @TenantId UNIQUEIDENTIFIER,
                @SiteId UNIQUEIDENTIFIER = NULL,
                @Status NVARCHAR(50),
                @TripId UNIQUEIDENTIFIER = NULL,
                @VehicleId UNIQUEIDENTIFIER,
                @DriverUserId UNIQUEIDENTIFIER,
                @IncidentType NVARCHAR(50),
                @Severity NVARCHAR(30),
                @OccurredAtUtc DATETIME2(3) = NULL,
                @Location NVARCHAR(500),
                @CreatedAtUtc DATETIME2(3),
                @CreatedBy UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN TRY
                BEGIN TRANSACTION;
                    INSERT INTO [LogisticsTransportSafety].[TransportIncidents]
                        ([TransportIncidentId], [TenantId], [SiteId], [Status], [TripId], [VehicleId], [DriverUserId],
                         [IncidentType], [Severity], [OccurredAtUtc], [Location], [CreatedAtUtc], [CreatedBy])
                    VALUES
                        (@TransportIncidentId, @TenantId, @SiteId, @Status, @TripId, @VehicleId, @DriverUserId,
                         @IncidentType, @Severity, @OccurredAtUtc, @Location, @CreatedAtUtc, @CreatedBy);
                COMMIT TRANSACTION;
            END TRY
            BEGIN CATCH
                IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
                THROW;
            END CATCH;
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [LogisticsTransportSafety].[usp_TransportIncident_GetAll]
            AS
            BEGIN
                SET NOCOUNT ON;

                SELECT
                    [TransportIncidentId], [TenantId], [SiteId], [Status], [TripId], [VehicleId], [DriverUserId],
                    [IncidentType], [Severity], [OccurredAtUtc], [Location], [CreatedAtUtc], [CreatedBy]
                FROM [LogisticsTransportSafety].[TransportIncidents]
                WHERE ISNULL([IsDeleted], 0) = 0;
            END;
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [LogisticsTransportSafety].[usp_TransportIncident_GetById]
                @TransportIncidentId UNIQUEIDENTIFIER
            AS
            BEGIN
                SET NOCOUNT ON;

                SELECT
                    [TransportIncidentId], [TenantId], [SiteId], [Status], [TripId], [VehicleId], [DriverUserId],
                    [IncidentType], [Severity], [OccurredAtUtc], [Location], [CreatedAtUtc], [CreatedBy]
                FROM [LogisticsTransportSafety].[TransportIncidents]
                WHERE [TransportIncidentId] = @TransportIncidentId
                  AND ISNULL([IsDeleted], 0) = 0;
            END;
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [LogisticsTransportSafety].[usp_TransportIncident_GetHistory]
                @TenantId UNIQUEIDENTIFIER,
                @SiteId UNIQUEIDENTIFIER = NULL,
                @FromUtc DATETIME2(3),
                @ToUtc DATETIME2(3)
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT
                    [TransportIncidentId], [TenantId], [SiteId], [VehicleId], [DriverUserId], [IncidentType], [Severity],
                    [Status] AS [IncidentStatus], [Location], [OccurredAtUtc], [CreatedAtUtc], [CreatedBy]
                FROM [LogisticsTransportSafety].[TransportIncidents]
                WHERE [TenantId] = @TenantId
                  AND (@SiteId IS NULL OR [SiteId] = @SiteId)
                  AND [OccurredAtUtc] >= @FromUtc
                  AND [OccurredAtUtc] <= @ToUtc
                  AND ISNULL([IsDeleted], 0) = 0
                ORDER BY [OccurredAtUtc] DESC;
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [LogisticsTransportSafety].[usp_TransportIncident_Update]
                @NowUtc DATETIME2(3),
                @TransportIncidentId UNIQUEIDENTIFIER,
                @TenantId UNIQUEIDENTIFIER,
                @SiteId UNIQUEIDENTIFIER = NULL,
                @Status NVARCHAR(50)
            AS
            SET NOCOUNT ON;
            BEGIN TRY
                BEGIN TRANSACTION;
                    UPDATE [LogisticsTransportSafety].[TransportIncidents]
                    SET
                        [SiteId] = @SiteId,
                        [Status] = @Status,
                        [ModifiedAtUtc] = @NowUtc
                    WHERE [TransportIncidentId] = @TransportIncidentId
                      AND [TenantId] = @TenantId
                      AND ISNULL([IsDeleted], 0) = 0;
                COMMIT TRANSACTION;
            END TRY
            BEGIN CATCH
                IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
                THROW;
            END CATCH;
            """);
    }
}

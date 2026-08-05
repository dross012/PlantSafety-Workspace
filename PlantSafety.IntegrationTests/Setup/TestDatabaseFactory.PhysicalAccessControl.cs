// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.Setup
//  Layer:          IntegrationTests
//  File:           TestDatabaseFactory.PhysicalAccessControl.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Partial-class extension of TestDatabaseFactory adding the
//                  PhysicalAccessControl module schema (AccessPoints, Credentials, AccessEvents,
//                  SafetyZones, AccessAnomalies, RestrictedZonePolicies). Kept in its own file
//                  (rather than appended inline to the very large, heavily-concurrently-edited
//                  TestDatabaseFactory.cs) to avoid edit collisions with other parallel testing
//                  agents; TestDatabaseFactory was made `partial` to support this (see the
//                  LogisticsTransportSafety sibling file for the established precedent). Table
//                  DDL and stored procedure bodies are transcribed verbatim from
//                  PlantSafety.Infrastructure\PhysicalAccessControl\Sql\Tables\*.sql and
//                  PlantSafety.Infrastructure\PhysicalAccessControl\Sql\Procedures\*.sql.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
namespace PlantSafety.IntegrationTests.Setup;

public static partial class TestDatabaseFactory
{
    /// <summary>
    /// Creates the PhysicalAccessControl schema, its six aggregate tables, and all backing
    /// stored procedures against the provided Testcontainers connection string.
    /// </summary>
    public static async Task ApplyPhysicalAccessControlSchemaAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken);

        await conn.ExecuteAsync("""
            IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'PhysicalAccessControl')
                EXEC('CREATE SCHEMA [PhysicalAccessControl]');
            """);

        // ---- Tables ----------------------------------------------------------------

        await conn.ExecuteAsync("""
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AccessPoints' AND schema_id = SCHEMA_ID('PhysicalAccessControl'))
            BEGIN
                CREATE TABLE [PhysicalAccessControl].[AccessPoints] (
                    [AccessPointId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                    [TenantId] UNIQUEIDENTIFIER NOT NULL,
                    [SiteId] UNIQUEIDENTIFIER NULL,
                    [Status] NVARCHAR(100) NOT NULL,
                    [AccessPointCode] NVARCHAR(100) NOT NULL,
                    [Description] NVARCHAR(2000) NOT NULL,
                    [DeviceType] NVARCHAR(100) NOT NULL,
                    [SafetyZoneId] UNIQUEIDENTIFIER NULL,
                    [IsPerimeter] BIT NOT NULL CONSTRAINT DF_AccessPoints_IsPerimeter DEFAULT (0),
                    [CreatedAtUtc] DATETIME2(3) NOT NULL,
                    [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
                    [IsDeleted] BIT NOT NULL CONSTRAINT DF_AccessPoints_IsDeleted DEFAULT (0),
                    [ModifiedAtUtc] DATETIME2(3) NULL,
                    [ModifiedBy] UNIQUEIDENTIFIER NULL,
                    [CorrelationId] UNIQUEIDENTIFIER NULL,
                    [ActorId] UNIQUEIDENTIFIER NULL
                );

                CREATE NONCLUSTERED INDEX [IX_AccessPoints_TenantId_SiteId]
                    ON [PhysicalAccessControl].[AccessPoints] ([TenantId], [SiteId])
                    INCLUDE ([Status], [AccessPointCode], [SafetyZoneId]);
            END
            """);

        await conn.ExecuteAsync("""
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Credentials' AND schema_id = SCHEMA_ID('PhysicalAccessControl'))
            BEGIN
                CREATE TABLE [PhysicalAccessControl].[Credentials] (
                    [CredentialId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                    [TenantId] UNIQUEIDENTIFIER NOT NULL,
                    [SiteId] UNIQUEIDENTIFIER NULL,
                    [Status] NVARCHAR(100) NOT NULL,
                    [BadgeNumber] NVARCHAR(100) NOT NULL,
                    [HolderUserId] UNIQUEIDENTIFIER NOT NULL,
                    [HolderType] NVARCHAR(100) NOT NULL,
                    [AccessLevel] NVARCHAR(100) NOT NULL,
                    [IssuedAtUtc] DATETIME2(3) NOT NULL,
                    [ExpiresAtUtc] DATETIME2(3) NULL,
                    [RevocationReason] NVARCHAR(1000) NULL,
                    [CreatedAtUtc] DATETIME2(3) NOT NULL,
                    [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
                    [IsDeleted] BIT NOT NULL CONSTRAINT DF_Credentials_IsDeleted DEFAULT (0),
                    [ModifiedAtUtc] DATETIME2(3) NULL,
                    [ModifiedBy] UNIQUEIDENTIFIER NULL,
                    [CorrelationId] UNIQUEIDENTIFIER NULL,
                    [ActorId] UNIQUEIDENTIFIER NULL
                );

                CREATE NONCLUSTERED INDEX [IX_Credentials_TenantId_HolderUserId]
                    ON [PhysicalAccessControl].[Credentials] ([TenantId], [HolderUserId])
                    INCLUDE ([Status], [AccessLevel]);
            END
            """);

        await conn.ExecuteAsync("""
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AccessEvents' AND schema_id = SCHEMA_ID('PhysicalAccessControl'))
            BEGIN
                CREATE TABLE [PhysicalAccessControl].[AccessEvents] (
                    [AccessEventId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                    [TenantId] UNIQUEIDENTIFIER NOT NULL,
                    [SiteId] UNIQUEIDENTIFIER NULL,
                    [Status] NVARCHAR(100) NOT NULL,
                    [AccessPointId] UNIQUEIDENTIFIER NOT NULL,
                    [CredentialId] UNIQUEIDENTIFIER NULL,
                    [SafetyZoneId] UNIQUEIDENTIFIER NULL,
                    [EventType] NVARCHAR(100) NOT NULL,
                    [Direction] NVARCHAR(20) NOT NULL,
                    [OccurredAtUtc] DATETIME2(3) NOT NULL,
                    [FlaggedAnomalyType] NVARCHAR(100) NULL,
                    [CreatedAtUtc] DATETIME2(3) NOT NULL,
                    [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
                    [IsDeleted] BIT NOT NULL CONSTRAINT DF_AccessEvents_IsDeleted DEFAULT (0),
                    [ModifiedAtUtc] DATETIME2(3) NULL,
                    [ModifiedBy] UNIQUEIDENTIFIER NULL,
                    [CorrelationId] UNIQUEIDENTIFIER NULL,
                    [ActorId] UNIQUEIDENTIFIER NULL
                );

                CREATE NONCLUSTERED INDEX [IX_AccessEvents_TenantId_SafetyZoneId_OccurredAtUtc]
                    ON [PhysicalAccessControl].[AccessEvents] ([TenantId], [SafetyZoneId], [OccurredAtUtc])
                    INCLUDE ([Status], [EventType], [Direction], [AccessPointId], [CredentialId]);
            END
            """);

        await conn.ExecuteAsync("""
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'SafetyZones' AND schema_id = SCHEMA_ID('PhysicalAccessControl'))
            BEGIN
                CREATE TABLE [PhysicalAccessControl].[SafetyZones] (
                    [SafetyZoneId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                    [TenantId] UNIQUEIDENTIFIER NOT NULL,
                    [SiteId] UNIQUEIDENTIFIER NULL,
                    [Status] NVARCHAR(100) NOT NULL,
                    [ZoneCode] NVARCHAR(100) NOT NULL,
                    [ZoneName] NVARCHAR(200) NOT NULL,
                    [HazardLevel] NVARCHAR(50) NOT NULL,
                    [MaxOccupancy] INT NOT NULL CONSTRAINT DF_SafetyZones_MaxOccupancy DEFAULT (0),
                    [CurrentOccupancy] INT NOT NULL CONSTRAINT DF_SafetyZones_CurrentOccupancy DEFAULT (0),
                    [CreatedAtUtc] DATETIME2(3) NOT NULL,
                    [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
                    [IsDeleted] BIT NOT NULL CONSTRAINT DF_SafetyZones_IsDeleted DEFAULT (0),
                    [ModifiedAtUtc] DATETIME2(3) NULL,
                    [ModifiedBy] UNIQUEIDENTIFIER NULL,
                    [CorrelationId] UNIQUEIDENTIFIER NULL,
                    [ActorId] UNIQUEIDENTIFIER NULL
                );

                CREATE NONCLUSTERED INDEX [IX_SafetyZones_TenantId_SiteId]
                    ON [PhysicalAccessControl].[SafetyZones] ([TenantId], [SiteId])
                    INCLUDE ([Status], [ZoneCode]);
            END
            """);

        await conn.ExecuteAsync("""
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AccessAnomalies' AND schema_id = SCHEMA_ID('PhysicalAccessControl'))
            BEGIN
                CREATE TABLE [PhysicalAccessControl].[AccessAnomalies] (
                    [AccessAnomalyId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                    [TenantId] UNIQUEIDENTIFIER NOT NULL,
                    [SiteId] UNIQUEIDENTIFIER NULL,
                    [Status] NVARCHAR(100) NOT NULL,
                    [AccessEventId] UNIQUEIDENTIFIER NULL,
                    [AccessPointId] UNIQUEIDENTIFIER NOT NULL,
                    [CredentialId] UNIQUEIDENTIFIER NULL,
                    [SafetyZoneId] UNIQUEIDENTIFIER NULL,
                    [AnomalyType] NVARCHAR(100) NOT NULL,
                    [SeverityScore] DECIMAL(9,2) NOT NULL,
                    [AcknowledgedByUserId] UNIQUEIDENTIFIER NULL,
                    [Resolution] NVARCHAR(2000) NULL,
                    [CreatedAtUtc] DATETIME2(3) NOT NULL,
                    [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
                    [IsDeleted] BIT NOT NULL CONSTRAINT DF_AccessAnomalies_IsDeleted DEFAULT (0),
                    [ModifiedAtUtc] DATETIME2(3) NULL,
                    [ModifiedBy] UNIQUEIDENTIFIER NULL,
                    [CorrelationId] UNIQUEIDENTIFIER NULL,
                    [ActorId] UNIQUEIDENTIFIER NULL
                );

                CREATE NONCLUSTERED INDEX [IX_AccessAnomalies_TenantId_SiteId_Status]
                    ON [PhysicalAccessControl].[AccessAnomalies] ([TenantId], [SiteId], [Status])
                    INCLUDE ([AnomalyType], [SeverityScore], [CreatedAtUtc]);
            END
            """);

        await conn.ExecuteAsync("""
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'RestrictedZonePolicies' AND schema_id = SCHEMA_ID('PhysicalAccessControl'))
            BEGIN
                CREATE TABLE [PhysicalAccessControl].[RestrictedZonePolicies] (
                    [RestrictedZonePolicyId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                    [TenantId] UNIQUEIDENTIFIER NOT NULL,
                    [SiteId] UNIQUEIDENTIFIER NULL,
                    [Status] NVARCHAR(100) NOT NULL,
                    [SafetyZoneId] UNIQUEIDENTIFIER NOT NULL,
                    [PolicyName] NVARCHAR(200) NOT NULL,
                    [AllowedAccessLevel] NVARCHAR(100) NOT NULL,
                    [EnforceAfterHoursLockout] BIT NOT NULL CONSTRAINT DF_RestrictedZonePolicies_EnforceAfterHoursLockout DEFAULT (0),
                    [CreatedAtUtc] DATETIME2(3) NOT NULL,
                    [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
                    [IsDeleted] BIT NOT NULL CONSTRAINT DF_RestrictedZonePolicies_IsDeleted DEFAULT (0),
                    [ModifiedAtUtc] DATETIME2(3) NULL,
                    [ModifiedBy] UNIQUEIDENTIFIER NULL,
                    [CorrelationId] UNIQUEIDENTIFIER NULL,
                    [ActorId] UNIQUEIDENTIFIER NULL
                );

                CREATE NONCLUSTERED INDEX [IX_RestrictedZonePolicies_TenantId_SafetyZoneId]
                    ON [PhysicalAccessControl].[RestrictedZonePolicies] ([TenantId], [SafetyZoneId])
                    INCLUDE ([Status], [AllowedAccessLevel]);
            END
            """);

        // ---- Stored procedures: AccessPoints ----------------------------------------

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [PhysicalAccessControl].[usp_AccessPoints_Create]
                @AccessPointId    UNIQUEIDENTIFIER,
                @TenantId         UNIQUEIDENTIFIER,
                @SiteId           UNIQUEIDENTIFIER = NULL,
                @Status           NVARCHAR(100),
                @AccessPointCode  NVARCHAR(100),
                @Description      NVARCHAR(2000),
                @DeviceType       NVARCHAR(100),
                @SafetyZoneId     UNIQUEIDENTIFIER = NULL,
                @IsPerimeter      BIT,
                @CreatedAtUtc     DATETIME2(3),
                @CreatedBy        UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN
                BEGIN TRY
                    INSERT INTO [PhysicalAccessControl].[AccessPoints]
                        ([AccessPointId], [TenantId], [SiteId], [Status], [AccessPointCode], [Description],
                         [DeviceType], [SafetyZoneId], [IsPerimeter], [CreatedAtUtc], [CreatedBy])
                    VALUES
                        (@AccessPointId, @TenantId, @SiteId, @Status, @AccessPointCode, @Description,
                         @DeviceType, @SafetyZoneId, @IsPerimeter, @CreatedAtUtc, @CreatedBy);
                END TRY
                BEGIN CATCH
                    THROW;
                END CATCH
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [PhysicalAccessControl].[usp_AccessPoints_Update]
                @NowUtc           DATETIME2(3),
                @AccessPointId    UNIQUEIDENTIFIER,
                @TenantId         UNIQUEIDENTIFIER,
                @SiteId           UNIQUEIDENTIFIER = NULL,
                @Status           NVARCHAR(100),
                @AccessPointCode  NVARCHAR(100),
                @Description      NVARCHAR(2000),
                @DeviceType       NVARCHAR(100),
                @SafetyZoneId     UNIQUEIDENTIFIER = NULL,
                @IsPerimeter      BIT
            AS
            SET NOCOUNT ON;
            BEGIN
                BEGIN TRY
                    UPDATE [PhysicalAccessControl].[AccessPoints]
                    SET [Status]          = @Status,
                        [AccessPointCode] = @AccessPointCode,
                        [Description]     = @Description,
                        [DeviceType]      = @DeviceType,
                        [SafetyZoneId]    = @SafetyZoneId,
                        [IsPerimeter]     = @IsPerimeter,
                        [ModifiedAtUtc]   = @NowUtc
                    WHERE [AccessPointId] = @AccessPointId
                      AND [TenantId] = @TenantId;
                END TRY
                BEGIN CATCH
                    THROW;
                END CATCH
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [PhysicalAccessControl].[usp_AccessPoints_GetById]
                @AccessPointId UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT
                    [AccessPointId], [TenantId], [SiteId], [Status], [AccessPointCode], [Description],
                    [DeviceType], [SafetyZoneId], [IsPerimeter], [CreatedAtUtc], [CreatedBy]
                FROM [PhysicalAccessControl].[AccessPoints]
                WHERE [AccessPointId] = @AccessPointId
                  AND ISNULL([IsDeleted], 0) = 0;
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [PhysicalAccessControl].[usp_AccessPoints_GetByIdItem]
                @TenantId      UNIQUEIDENTIFIER,
                @AccessPointId UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT
                    [AccessPointId],
                    [TenantId],
                    [SiteId],
                    [AccessPointCode],
                    [Description],
                    [DeviceType],
                    [SafetyZoneId],
                    [IsPerimeter],
                    [Status],
                    [CreatedAtUtc],
                    [CreatedBy]
                FROM [PhysicalAccessControl].[AccessPoints]
                WHERE [TenantId] = @TenantId
                  AND [AccessPointId] = @AccessPointId
                  AND ISNULL([IsDeleted], 0) = 0;
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [PhysicalAccessControl].[usp_AccessPoints_GetAll]
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT
                    [AccessPointId], [TenantId], [SiteId], [Status], [AccessPointCode], [Description],
                    [DeviceType], [SafetyZoneId], [IsPerimeter], [CreatedAtUtc], [CreatedBy]
                FROM [PhysicalAccessControl].[AccessPoints]
                WHERE ISNULL([IsDeleted], 0) = 0;
            END
            """);

        // ---- Stored procedures: Credentials ------------------------------------------

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [PhysicalAccessControl].[usp_Credentials_Create]
                @CredentialId  UNIQUEIDENTIFIER,
                @TenantId      UNIQUEIDENTIFIER,
                @SiteId        UNIQUEIDENTIFIER = NULL,
                @Status        NVARCHAR(100),
                @BadgeNumber   NVARCHAR(100),
                @HolderUserId  UNIQUEIDENTIFIER,
                @HolderType    NVARCHAR(100),
                @AccessLevel   NVARCHAR(100),
                @IssuedAtUtc   DATETIME2(3),
                @ExpiresAtUtc  DATETIME2(3) = NULL,
                @CreatedAtUtc  DATETIME2(3),
                @CreatedBy     UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN
                BEGIN TRY
                    INSERT INTO [PhysicalAccessControl].[Credentials]
                        ([CredentialId], [TenantId], [SiteId], [Status], [BadgeNumber], [HolderUserId],
                         [HolderType], [AccessLevel], [IssuedAtUtc], [ExpiresAtUtc], [CreatedAtUtc], [CreatedBy])
                    VALUES
                        (@CredentialId, @TenantId, @SiteId, @Status, @BadgeNumber, @HolderUserId,
                         @HolderType, @AccessLevel, @IssuedAtUtc, @ExpiresAtUtc, @CreatedAtUtc, @CreatedBy);
                END TRY
                BEGIN CATCH
                    THROW;
                END CATCH
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [PhysicalAccessControl].[usp_Credentials_Update]
                @NowUtc            DATETIME2(3),
                @CredentialId      UNIQUEIDENTIFIER,
                @TenantId          UNIQUEIDENTIFIER,
                @SiteId            UNIQUEIDENTIFIER = NULL,
                @Status            NVARCHAR(100),
                @BadgeNumber       NVARCHAR(100),
                @HolderUserId      UNIQUEIDENTIFIER,
                @HolderType        NVARCHAR(100),
                @AccessLevel       NVARCHAR(100),
                @ExpiresAtUtc      DATETIME2(3) = NULL,
                @RevocationReason  NVARCHAR(1000) = NULL
            AS
            SET NOCOUNT ON;
            BEGIN
                BEGIN TRY
                    UPDATE [PhysicalAccessControl].[Credentials]
                    SET [Status]           = @Status,
                        [BadgeNumber]      = @BadgeNumber,
                        [HolderUserId]     = @HolderUserId,
                        [HolderType]       = @HolderType,
                        [AccessLevel]      = @AccessLevel,
                        [ExpiresAtUtc]     = @ExpiresAtUtc,
                        [RevocationReason] = @RevocationReason,
                        [ModifiedAtUtc]    = @NowUtc
                    WHERE [CredentialId] = @CredentialId
                      AND [TenantId] = @TenantId;
                END TRY
                BEGIN CATCH
                    THROW;
                END CATCH
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [PhysicalAccessControl].[usp_Credentials_GetById]
                @CredentialId UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT
                    [CredentialId], [TenantId], [SiteId], [Status], [BadgeNumber], [HolderUserId],
                    [HolderType], [AccessLevel], [IssuedAtUtc], [ExpiresAtUtc], [RevocationReason],
                    [CreatedAtUtc], [CreatedBy]
                FROM [PhysicalAccessControl].[Credentials]
                WHERE [CredentialId] = @CredentialId
                  AND ISNULL([IsDeleted], 0) = 0;
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [PhysicalAccessControl].[usp_Credentials_GetStatusItem]
                @TenantId     UNIQUEIDENTIFIER,
                @CredentialId UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT
                    [CredentialId],
                    [TenantId],
                    ('****' + RIGHT([BadgeNumber], 4)) AS [BadgeNumberMasked],
                    [HolderType],
                    [AccessLevel],
                    [Status],
                    [IssuedAtUtc],
                    [ExpiresAtUtc]
                FROM [PhysicalAccessControl].[Credentials]
                WHERE [TenantId] = @TenantId
                  AND [CredentialId] = @CredentialId
                  AND ISNULL([IsDeleted], 0) = 0;
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [PhysicalAccessControl].[usp_Credentials_GetAll]
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT
                    [CredentialId], [TenantId], [SiteId], [Status], [BadgeNumber], [HolderUserId],
                    [HolderType], [AccessLevel], [IssuedAtUtc], [ExpiresAtUtc], [RevocationReason],
                    [CreatedAtUtc], [CreatedBy]
                FROM [PhysicalAccessControl].[Credentials]
                WHERE ISNULL([IsDeleted], 0) = 0;
            END
            """);

        // ---- Stored procedures: AccessEvents -----------------------------------------

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [PhysicalAccessControl].[usp_AccessEvents_Create]
                @AccessEventId  UNIQUEIDENTIFIER,
                @TenantId       UNIQUEIDENTIFIER,
                @SiteId         UNIQUEIDENTIFIER = NULL,
                @Status         NVARCHAR(100),
                @AccessPointId  UNIQUEIDENTIFIER,
                @CredentialId   UNIQUEIDENTIFIER = NULL,
                @SafetyZoneId   UNIQUEIDENTIFIER = NULL,
                @EventType      NVARCHAR(100),
                @Direction      NVARCHAR(20),
                @OccurredAtUtc  DATETIME2(3),
                @CreatedAtUtc   DATETIME2(3),
                @CreatedBy      UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN
                BEGIN TRY
                    INSERT INTO [PhysicalAccessControl].[AccessEvents]
                        ([AccessEventId], [TenantId], [SiteId], [Status], [AccessPointId], [CredentialId],
                         [SafetyZoneId], [EventType], [Direction], [OccurredAtUtc], [CreatedAtUtc], [CreatedBy])
                    VALUES
                        (@AccessEventId, @TenantId, @SiteId, @Status, @AccessPointId, @CredentialId,
                         @SafetyZoneId, @EventType, @Direction, @OccurredAtUtc, @CreatedAtUtc, @CreatedBy);
                END TRY
                BEGIN CATCH
                    THROW;
                END CATCH
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [PhysicalAccessControl].[usp_AccessEvents_Update]
                @NowUtc              DATETIME2(3),
                @AccessEventId       UNIQUEIDENTIFIER,
                @TenantId            UNIQUEIDENTIFIER,
                @SiteId              UNIQUEIDENTIFIER = NULL,
                @Status              NVARCHAR(100),
                @AccessPointId       UNIQUEIDENTIFIER,
                @CredentialId        UNIQUEIDENTIFIER = NULL,
                @SafetyZoneId        UNIQUEIDENTIFIER = NULL,
                @EventType           NVARCHAR(100),
                @Direction           NVARCHAR(20),
                @FlaggedAnomalyType  NVARCHAR(100) = NULL
            AS
            SET NOCOUNT ON;
            BEGIN
                BEGIN TRY
                    UPDATE [PhysicalAccessControl].[AccessEvents]
                    SET [Status]             = @Status,
                        [AccessPointId]      = @AccessPointId,
                        [CredentialId]       = @CredentialId,
                        [SafetyZoneId]       = @SafetyZoneId,
                        [EventType]          = @EventType,
                        [Direction]          = @Direction,
                        [FlaggedAnomalyType] = @FlaggedAnomalyType,
                        [ModifiedAtUtc]      = @NowUtc
                    WHERE [AccessEventId] = @AccessEventId
                      AND [TenantId] = @TenantId;
                END TRY
                BEGIN CATCH
                    THROW;
                END CATCH
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [PhysicalAccessControl].[usp_AccessEvents_GetById]
                @AccessEventId UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT
                    [AccessEventId], [TenantId], [SiteId], [Status], [AccessPointId], [CredentialId],
                    [SafetyZoneId], [EventType], [Direction], [OccurredAtUtc], [FlaggedAnomalyType],
                    [CreatedAtUtc], [CreatedBy]
                FROM [PhysicalAccessControl].[AccessEvents]
                WHERE [AccessEventId] = @AccessEventId
                  AND ISNULL([IsDeleted], 0) = 0;
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [PhysicalAccessControl].[usp_AccessEvents_ListByZoneItems]
                @TenantId     UNIQUEIDENTIFIER,
                @SafetyZoneId UNIQUEIDENTIFIER,
                @FromUtc      DATETIME2(3),
                @ToUtc        DATETIME2(3)
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT
                    [AccessEventId],
                    [TenantId],
                    [AccessPointId],
                    [CredentialId],
                    [SafetyZoneId],
                    [EventType],
                    [Direction],
                    [Status],
                    [OccurredAtUtc]
                FROM [PhysicalAccessControl].[AccessEvents]
                WHERE [TenantId] = @TenantId
                  AND [SafetyZoneId] = @SafetyZoneId
                  AND ISNULL([IsDeleted], 0) = 0
                  AND [OccurredAtUtc] >= @FromUtc
                  AND [OccurredAtUtc] <= @ToUtc
                ORDER BY [OccurredAtUtc] DESC;
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [PhysicalAccessControl].[usp_AccessEvents_GetAll]
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT
                    [AccessEventId], [TenantId], [SiteId], [Status], [AccessPointId], [CredentialId],
                    [SafetyZoneId], [EventType], [Direction], [OccurredAtUtc], [FlaggedAnomalyType],
                    [CreatedAtUtc], [CreatedBy]
                FROM [PhysicalAccessControl].[AccessEvents]
                WHERE ISNULL([IsDeleted], 0) = 0;
            END
            """);

        // ---- Stored procedures: SafetyZones ------------------------------------------

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [PhysicalAccessControl].[usp_SafetyZones_Create]
                @SafetyZoneId      UNIQUEIDENTIFIER,
                @TenantId          UNIQUEIDENTIFIER,
                @SiteId            UNIQUEIDENTIFIER = NULL,
                @Status            NVARCHAR(100),
                @ZoneCode          NVARCHAR(100),
                @ZoneName          NVARCHAR(200),
                @HazardLevel       NVARCHAR(50),
                @MaxOccupancy      INT,
                @CurrentOccupancy  INT,
                @CreatedAtUtc      DATETIME2(3),
                @CreatedBy         UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN
                BEGIN TRY
                    INSERT INTO [PhysicalAccessControl].[SafetyZones]
                        ([SafetyZoneId], [TenantId], [SiteId], [Status], [ZoneCode], [ZoneName],
                         [HazardLevel], [MaxOccupancy], [CurrentOccupancy], [CreatedAtUtc], [CreatedBy])
                    VALUES
                        (@SafetyZoneId, @TenantId, @SiteId, @Status, @ZoneCode, @ZoneName,
                         @HazardLevel, @MaxOccupancy, @CurrentOccupancy, @CreatedAtUtc, @CreatedBy);
                END TRY
                BEGIN CATCH
                    THROW;
                END CATCH
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [PhysicalAccessControl].[usp_SafetyZones_Update]
                @NowUtc            DATETIME2(3),
                @SafetyZoneId      UNIQUEIDENTIFIER,
                @TenantId          UNIQUEIDENTIFIER,
                @SiteId            UNIQUEIDENTIFIER = NULL,
                @Status            NVARCHAR(100),
                @ZoneCode          NVARCHAR(100),
                @ZoneName          NVARCHAR(200),
                @HazardLevel       NVARCHAR(50),
                @MaxOccupancy      INT,
                @CurrentOccupancy  INT
            AS
            SET NOCOUNT ON;
            BEGIN
                BEGIN TRY
                    UPDATE [PhysicalAccessControl].[SafetyZones]
                    SET [Status]           = @Status,
                        [ZoneCode]         = @ZoneCode,
                        [ZoneName]         = @ZoneName,
                        [HazardLevel]      = @HazardLevel,
                        [MaxOccupancy]     = @MaxOccupancy,
                        [CurrentOccupancy] = @CurrentOccupancy,
                        [ModifiedAtUtc]    = @NowUtc
                    WHERE [SafetyZoneId] = @SafetyZoneId
                      AND [TenantId] = @TenantId;
                END TRY
                BEGIN CATCH
                    THROW;
                END CATCH
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [PhysicalAccessControl].[usp_SafetyZones_GetById]
                @SafetyZoneId UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT
                    [SafetyZoneId], [TenantId], [SiteId], [Status], [ZoneCode], [ZoneName],
                    [HazardLevel], [MaxOccupancy], [CurrentOccupancy], [CreatedAtUtc], [CreatedBy]
                FROM [PhysicalAccessControl].[SafetyZones]
                WHERE [SafetyZoneId] = @SafetyZoneId
                  AND ISNULL([IsDeleted], 0) = 0;
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [PhysicalAccessControl].[usp_SafetyZones_GetOccupancyItem]
                @TenantId     UNIQUEIDENTIFIER,
                @SafetyZoneId UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT
                    [SafetyZoneId],
                    [TenantId],
                    [ZoneCode],
                    [ZoneName],
                    [MaxOccupancy],
                    [CurrentOccupancy],
                    [Status],
                    ISNULL([ModifiedAtUtc], [CreatedAtUtc]) AS [AsOfUtc]
                FROM [PhysicalAccessControl].[SafetyZones]
                WHERE [TenantId] = @TenantId
                  AND [SafetyZoneId] = @SafetyZoneId
                  AND ISNULL([IsDeleted], 0) = 0;
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [PhysicalAccessControl].[usp_SafetyZones_GetAll]
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT
                    [SafetyZoneId], [TenantId], [SiteId], [Status], [ZoneCode], [ZoneName],
                    [HazardLevel], [MaxOccupancy], [CurrentOccupancy], [CreatedAtUtc], [CreatedBy]
                FROM [PhysicalAccessControl].[SafetyZones]
                WHERE ISNULL([IsDeleted], 0) = 0;
            END
            """);

        // ---- Stored procedures: AccessAnomalies --------------------------------------

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [PhysicalAccessControl].[usp_AccessAnomalies_Create]
                @AccessAnomalyId  UNIQUEIDENTIFIER,
                @TenantId         UNIQUEIDENTIFIER,
                @SiteId           UNIQUEIDENTIFIER = NULL,
                @Status           NVARCHAR(100),
                @AccessEventId    UNIQUEIDENTIFIER = NULL,
                @AccessPointId    UNIQUEIDENTIFIER,
                @CredentialId     UNIQUEIDENTIFIER = NULL,
                @SafetyZoneId     UNIQUEIDENTIFIER = NULL,
                @AnomalyType      NVARCHAR(100),
                @SeverityScore    DECIMAL(9,2),
                @CreatedAtUtc     DATETIME2(3),
                @CreatedBy        UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN
                BEGIN TRY
                    INSERT INTO [PhysicalAccessControl].[AccessAnomalies]
                        ([AccessAnomalyId], [TenantId], [SiteId], [Status], [AccessEventId], [AccessPointId],
                         [CredentialId], [SafetyZoneId], [AnomalyType], [SeverityScore], [CreatedAtUtc], [CreatedBy])
                    VALUES
                        (@AccessAnomalyId, @TenantId, @SiteId, @Status, @AccessEventId, @AccessPointId,
                         @CredentialId, @SafetyZoneId, @AnomalyType, @SeverityScore, @CreatedAtUtc, @CreatedBy);
                END TRY
                BEGIN CATCH
                    THROW;
                END CATCH
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [PhysicalAccessControl].[usp_AccessAnomalies_Update]
                @NowUtc                DATETIME2(3),
                @AccessAnomalyId       UNIQUEIDENTIFIER,
                @TenantId              UNIQUEIDENTIFIER,
                @SiteId                UNIQUEIDENTIFIER = NULL,
                @Status                NVARCHAR(100),
                @AnomalyType           NVARCHAR(100),
                @SeverityScore         DECIMAL(9,2),
                @AcknowledgedByUserId  UNIQUEIDENTIFIER = NULL,
                @Resolution            NVARCHAR(2000) = NULL
            AS
            SET NOCOUNT ON;
            BEGIN
                BEGIN TRY
                    UPDATE [PhysicalAccessControl].[AccessAnomalies]
                    SET [Status]               = @Status,
                        [AnomalyType]          = @AnomalyType,
                        [SeverityScore]        = @SeverityScore,
                        [AcknowledgedByUserId] = @AcknowledgedByUserId,
                        [Resolution]           = @Resolution,
                        [ModifiedAtUtc]        = @NowUtc
                    WHERE [AccessAnomalyId] = @AccessAnomalyId
                      AND [TenantId] = @TenantId;
                END TRY
                BEGIN CATCH
                    THROW;
                END CATCH
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [PhysicalAccessControl].[usp_AccessAnomalies_GetById]
                @AccessAnomalyId UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT
                    [AccessAnomalyId], [TenantId], [SiteId], [Status], [AccessEventId], [AccessPointId],
                    [CredentialId], [SafetyZoneId], [AnomalyType], [SeverityScore], [AcknowledgedByUserId],
                    [Resolution], [CreatedAtUtc], [CreatedBy]
                FROM [PhysicalAccessControl].[AccessAnomalies]
                WHERE [AccessAnomalyId] = @AccessAnomalyId
                  AND ISNULL([IsDeleted], 0) = 0;
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [PhysicalAccessControl].[usp_AccessAnomalies_ListActiveItems]
                @TenantId UNIQUEIDENTIFIER,
                @SiteId   UNIQUEIDENTIFIER = NULL
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT
                    [AccessAnomalyId],
                    [TenantId],
                    [AccessPointId],
                    [CredentialId],
                    [SafetyZoneId],
                    [AnomalyType],
                    [SeverityScore],
                    [Status],
                    [CreatedAtUtc]
                FROM [PhysicalAccessControl].[AccessAnomalies]
                WHERE [TenantId] = @TenantId
                  AND (@SiteId IS NULL OR [SiteId] = @SiteId)
                  AND [Status] <> 'Resolved'
                  AND ISNULL([IsDeleted], 0) = 0
                ORDER BY [SeverityScore] DESC, [CreatedAtUtc] DESC;
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [PhysicalAccessControl].[usp_AccessAnomalies_GetAll]
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT
                    [AccessAnomalyId], [TenantId], [SiteId], [Status], [AccessEventId], [AccessPointId],
                    [CredentialId], [SafetyZoneId], [AnomalyType], [SeverityScore], [AcknowledgedByUserId],
                    [Resolution], [CreatedAtUtc], [CreatedBy]
                FROM [PhysicalAccessControl].[AccessAnomalies]
                WHERE ISNULL([IsDeleted], 0) = 0;
            END
            """);

        // ---- Stored procedures: RestrictedZonePolicies -------------------------------

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [PhysicalAccessControl].[usp_RestrictedZonePolicies_Create]
                @RestrictedZonePolicyId    UNIQUEIDENTIFIER,
                @TenantId                  UNIQUEIDENTIFIER,
                @SiteId                    UNIQUEIDENTIFIER = NULL,
                @Status                    NVARCHAR(100),
                @SafetyZoneId              UNIQUEIDENTIFIER,
                @PolicyName                NVARCHAR(200),
                @AllowedAccessLevel        NVARCHAR(100),
                @EnforceAfterHoursLockout  BIT,
                @CreatedAtUtc              DATETIME2(3),
                @CreatedBy                 UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN
                BEGIN TRY
                    INSERT INTO [PhysicalAccessControl].[RestrictedZonePolicies]
                        ([RestrictedZonePolicyId], [TenantId], [SiteId], [Status], [SafetyZoneId],
                         [PolicyName], [AllowedAccessLevel], [EnforceAfterHoursLockout], [CreatedAtUtc], [CreatedBy])
                    VALUES
                        (@RestrictedZonePolicyId, @TenantId, @SiteId, @Status, @SafetyZoneId,
                         @PolicyName, @AllowedAccessLevel, @EnforceAfterHoursLockout, @CreatedAtUtc, @CreatedBy);
                END TRY
                BEGIN CATCH
                    THROW;
                END CATCH
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [PhysicalAccessControl].[usp_RestrictedZonePolicies_Update]
                @NowUtc                    DATETIME2(3),
                @RestrictedZonePolicyId    UNIQUEIDENTIFIER,
                @TenantId                  UNIQUEIDENTIFIER,
                @SiteId                    UNIQUEIDENTIFIER = NULL,
                @Status                    NVARCHAR(100),
                @SafetyZoneId              UNIQUEIDENTIFIER,
                @PolicyName                NVARCHAR(200),
                @AllowedAccessLevel        NVARCHAR(100),
                @EnforceAfterHoursLockout  BIT
            AS
            SET NOCOUNT ON;
            BEGIN
                BEGIN TRY
                    UPDATE [PhysicalAccessControl].[RestrictedZonePolicies]
                    SET [Status]                    = @Status,
                        [SafetyZoneId]              = @SafetyZoneId,
                        [PolicyName]                = @PolicyName,
                        [AllowedAccessLevel]        = @AllowedAccessLevel,
                        [EnforceAfterHoursLockout]  = @EnforceAfterHoursLockout,
                        [ModifiedAtUtc]             = @NowUtc
                    WHERE [RestrictedZonePolicyId] = @RestrictedZonePolicyId
                      AND [TenantId] = @TenantId;
                END TRY
                BEGIN CATCH
                    THROW;
                END CATCH
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [PhysicalAccessControl].[usp_RestrictedZonePolicies_GetById]
                @RestrictedZonePolicyId UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT
                    [RestrictedZonePolicyId], [TenantId], [SiteId], [Status], [SafetyZoneId],
                    [PolicyName], [AllowedAccessLevel], [EnforceAfterHoursLockout], [CreatedAtUtc], [CreatedBy]
                FROM [PhysicalAccessControl].[RestrictedZonePolicies]
                WHERE [RestrictedZonePolicyId] = @RestrictedZonePolicyId
                  AND ISNULL([IsDeleted], 0) = 0;
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [PhysicalAccessControl].[usp_RestrictedZonePolicies_GetAll]
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT
                    [RestrictedZonePolicyId], [TenantId], [SiteId], [Status], [SafetyZoneId],
                    [PolicyName], [AllowedAccessLevel], [EnforceAfterHoursLockout], [CreatedAtUtc], [CreatedBy]
                FROM [PhysicalAccessControl].[RestrictedZonePolicies]
                WHERE ISNULL([IsDeleted], 0) = 0;
            END
            """);
    }
}

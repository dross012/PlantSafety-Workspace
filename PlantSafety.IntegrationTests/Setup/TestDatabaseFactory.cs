// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.Setup
//  Layer:          IntegrationTests
//  File:           TestDatabaseFactory.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Creates and migrates the integration test SQL Server database schema.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
namespace PlantSafety.IntegrationTests.Setup;

/// <summary>
/// Applies all canonical System schema objects (OutboxMessages, ProcessedEvents)
/// against the provided Testcontainers connection string before tests execute.
/// </summary>
public static class TestDatabaseFactory
{
    /// <summary>Creates System schema and required tables.</summary>
    public static async Task ApplySchemaAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken);

        await conn.ExecuteAsync(@"
            IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'System')
                EXEC('CREATE SCHEMA [System]');

            IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES
                           WHERE TABLE_SCHEMA = 'System' AND TABLE_NAME = 'OutboxMessages')
            BEGIN
                CREATE TABLE [System].[OutboxMessages] (
                    MessageId       UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_OutboxMessages PRIMARY KEY,
                    EventType       NVARCHAR(200)    NOT NULL,
                    SchemaVersion   NVARCHAR(50)     NOT NULL,
                    TenantId        NVARCHAR(100)    NOT NULL,
                    ActorId         NVARCHAR(100)    NOT NULL,
                    CorrelationId   NVARCHAR(100)    NOT NULL,
                    OccurredAt      DATETIME2(3)     NOT NULL,
                    Payload         NVARCHAR(MAX)    NOT NULL,
                    Processed       BIT              NOT NULL CONSTRAINT DF_OutboxMessages_Processed DEFAULT (0),
                    ProcessedAt     DATETIME2(3)     NULL,
                    CreatedAt       DATETIME2(3)     NOT NULL CONSTRAINT DF_OutboxMessages_CreatedAt DEFAULT (SYSUTCDATETIME())
                );

                CREATE INDEX IX_OutboxMessages_Processed_OccurredAt
                    ON [System].[OutboxMessages] (Processed, OccurredAt)
                    INCLUDE (MessageId);

                CREATE INDEX IX_OutboxMessages_TenantId
                    ON [System].[OutboxMessages] (TenantId);
            END;

            IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES
                           WHERE TABLE_SCHEMA = 'System' AND TABLE_NAME = 'ProcessedEvents')
            BEGIN
                CREATE TABLE [System].[ProcessedEvents] (
                    EventId     UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                    ProcessedAt DATETIME2(3)     NOT NULL
                );
            END;");

        // Matches PlantSafety.Infrastructure/Persistence/Scripts/Procedures/usp_OutboxMessages_Insert.sql
        // verbatim -- previously missing entirely, so any test exercising a handler that writes a
        // domain event through OutboxWriter.FlushAsync failed with "Could not find stored procedure
        // 'System.usp_OutboxMessages_Insert'" the moment TransactionBehavior.Handle committed.
        await conn.ExecuteAsync(@"
            CREATE OR ALTER PROCEDURE [System].[usp_OutboxMessages_Insert]
                @MessageId          UNIQUEIDENTIFIER,
                @EventType          NVARCHAR(200),
                @SchemaVersion      NVARCHAR(50),
                @EnvelopeTenantId   NVARCHAR(100),
                @ActorId            NVARCHAR(100),
                @CorrelationId      NVARCHAR(100),
                @OccurredAt         DATETIME2(3),
                @Payload            NVARCHAR(MAX)
            AS
            SET NOCOUNT ON;
            BEGIN TRY

                INSERT INTO [System].[OutboxMessages]
                    (MessageId, EventType, SchemaVersion, TenantId, ActorId, CorrelationId, OccurredAt, Payload, Processed)
                VALUES
                    (@MessageId, @EventType, @SchemaVersion, @EnvelopeTenantId, @ActorId, @CorrelationId, @OccurredAt, @Payload, 0);

            END TRY
            BEGIN CATCH
                THROW;
            END CATCH;");
    }

    /// <summary>
    /// Creates the shared, cross-database <c>dbo.AuditLog</c> table and
    /// <c>dbo.usp_SearchAuditLog</c> stored procedure against the Testcontainers
    /// database so that <c>Shared.AuditLogging.Infrastructure.Persistence.AuditLogDapperRepository</c>
    /// can be exercised end-to-end by <see cref="IntegrationTestWebAppFactory"/>. Mirrors the
    /// canonical, version-controlled definitions in
    /// Shared\AuditLogging\sql\Phase0-Tables\00-create-auditlog-table.sql and
    /// Shared\AuditLogging\sql\Phase1-StoredProcedures\01-usp-search-auditlog.sql, minus the
    /// <c>USE [PlantSafety.AuditDB]</c> statements (the Testcontainers instance exposes a single
    /// database, not the real multi-database AuditDB topology).
    /// </summary>
    public static async Task ApplySharedAuditLoggingSchemaAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken);

        await conn.ExecuteAsync(@"
            IF OBJECT_ID(N'dbo.AuditLog', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.AuditLog
                (
                    TableName       nvarchar(128)   NOT NULL,
                    SourceDatabase  nvarchar(128)   NOT NULL,
                    Operation       nvarchar(50)    NOT NULL,
                    ChangedBy       nvarchar(128)   NOT NULL,
                    ChangeTime      datetime2       NOT NULL,
                    ViewerAccessed  bit             NOT NULL CONSTRAINT DF_AuditLog_ViewerAccessed DEFAULT (0),
                    ErrorContext    nvarchar(400)   NULL,
                    Severity        int             NOT NULL CONSTRAINT DF_AuditLog_Severity DEFAULT (0),
                    Module          nvarchar(128)   NOT NULL,
                    BeforeSnapshot  nvarchar(max)   NULL,
                    AfterSnapshot   nvarchar(max)   NULL
                );
            END;");

        await conn.ExecuteAsync(@"
            CREATE OR ALTER PROCEDURE dbo.usp_SearchAuditLog
                @TableName   nvarchar(128) = NULL,
                @Operation   nvarchar(50)  = NULL,
                @ChangedBy   nvarchar(128) = NULL,
                @Module      nvarchar(128) = NULL,
                @Severity    int           = NULL,
                @FromUtc     datetime2     = NULL,
                @ToUtc       datetime2     = NULL,
                @SearchText  nvarchar(400) = NULL,
                @PageNumber  int           = 1,
                @PageSize    int           = 50
            AS
            BEGIN
                SET NOCOUNT ON;

                IF @PageNumber IS NULL OR @PageNumber < 1
                    THROW 50001, N'PageNumber must be a positive integer.', 1;
                IF @PageSize IS NULL OR @PageSize < 1
                    THROW 50001, N'PageSize must be a positive integer.', 1;

                SELECT COUNT(*)
                FROM dbo.AuditLog
                WHERE (@TableName  IS NULL OR TableName  = @TableName)
                  AND (@Operation  IS NULL OR Operation  = @Operation)
                  AND (@ChangedBy  IS NULL OR ChangedBy  = @ChangedBy)
                  AND (@Module     IS NULL OR Module     = @Module)
                  AND (@Severity   IS NULL OR Severity   = @Severity)
                  AND (@FromUtc    IS NULL OR ChangeTime >= @FromUtc)
                  AND (@ToUtc      IS NULL OR ChangeTime <= @ToUtc)
                  AND (@SearchText IS NULL OR ErrorContext LIKE N'%' + @SearchText + N'%');

                SELECT
                    TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                    ViewerAccessed, ErrorContext, Severity, Module, BeforeSnapshot, AfterSnapshot
                FROM dbo.AuditLog
                WHERE (@TableName  IS NULL OR TableName  = @TableName)
                  AND (@Operation  IS NULL OR Operation  = @Operation)
                  AND (@ChangedBy  IS NULL OR ChangedBy  = @ChangedBy)
                  AND (@Module     IS NULL OR Module     = @Module)
                  AND (@Severity   IS NULL OR Severity   = @Severity)
                  AND (@FromUtc    IS NULL OR ChangeTime >= @FromUtc)
                  AND (@ToUtc      IS NULL OR ChangeTime <= @ToUtc)
                  AND (@SearchText IS NULL OR ErrorContext LIKE N'%' + @SearchText + N'%')
                ORDER BY ChangeTime DESC
                OFFSET (@PageNumber - 1) * @PageSize ROWS
                FETCH NEXT @PageSize ROWS ONLY;
            END;");
    }

    /// <summary>
    /// Creates the <c>[SafetyEvents].[Incident]</c> table and its write/read stored procedures
    /// (<c>usp_Incident_Create/Update/SoftDelete</c>, <c>usp_IncidentRead_GetById/List/Search/
    /// Count/SearchCount</c>) against the Testcontainers database, mirroring the canonical,
    /// version-controlled definitions in PlantSafety.Infrastructure\SafetyEvents\Sql\Tables\
    /// Incident.sql and \Procedures\usp_Incident_*.sql / usp_IncidentRead_*.sql verbatim. Unlike
    /// <see cref="ApplySharedAuditLoggingSchemaAsync"/>, the write procedures' AuditLog/ErrorLog
    /// writes are wrapped in their own defensive BEGIN TRY/END TRY (empty CATCH) blocks in the
    /// canonical scripts, so they silently no-op if those tables are unreachable. That TRY/CATCH
    /// only shields *runtime* errors (e.g. a missing table in an existing database, resolved via
    /// deferred name resolution) -- referencing a three-part name whose *database* doesn't exist
    /// at all is a bind-time error SQL Server raises before the TRY block ever executes, so it is
    /// NOT swallowed. The <c>[PlantSafety.AuditDB]</c> / <c>[PlantSafety.ErrorDB]</c> three-part
    /// prefixes are therefore stripped down to <c>dbo.AuditLog</c> / <c>dbo.ErrorLog</c> below,
    /// matching the single-database Testcontainers topology (dbo.AuditLog is created by
    /// <see cref="ApplySharedAuditLoggingSchemaAsync"/>; dbo.ErrorLog is intentionally left
    /// uncreated so its INSERT still no-ops via the now-catchable "missing table" error).
    /// </summary>
    public static async Task ApplyIncidentManagementSchemaAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken);

        await conn.ExecuteAsync(@"
            IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'SafetyEvents')
                EXEC('CREATE SCHEMA [SafetyEvents]');

            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Incident' AND schema_id = SCHEMA_ID('SafetyEvents'))
            BEGIN
                CREATE TABLE [SafetyEvents].[Incident]
                (
                    IncidentId             UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Incident PRIMARY KEY,
                    TenantId               UNIQUEIDENTIFIER NOT NULL,
                    SiteId                 UNIQUEIDENTIFIER NOT NULL,
                    Title                  NVARCHAR(200)    NOT NULL,
                    Description            NVARCHAR(4000)   NOT NULL,
                    IncidentOccurredAtUtc  DATETIME2(3)     NOT NULL,
                    ReportedAtUtc          DATETIME2(3)     NOT NULL,
                    Severity               NVARCHAR(30)     NOT NULL,
                    Status                 NVARCHAR(30)     NOT NULL CONSTRAINT DF_Incident_Status DEFAULT (N'Reported'),
                    Location               NVARCHAR(500)    NOT NULL,
                    ReportedByWorkerId     UNIQUEIDENTIFIER NOT NULL,
                    AssignedInvestigatorId UNIQUEIDENTIFIER NULL,
                    ClosedAtUtc            DATETIME2(3)     NULL,
                    DeletionReason         NVARCHAR(1000)   NULL,
                    DeletedAtUtc           DATETIME2(3)     NULL,
                    IsDeleted              BIT              NOT NULL CONSTRAINT DF_Incident_IsDeleted DEFAULT (0),
                    LastModifiedByActorId  UNIQUEIDENTIFIER NOT NULL,
                    LastModifiedAtUtc      DATETIME2(3)     NOT NULL,
                    CreatedAtUtc           DATETIME2(3)     NOT NULL,
                    CreatedBy              UNIQUEIDENTIFIER NOT NULL,
                    RowVersion             ROWVERSION       NOT NULL
                );

                CREATE NONCLUSTERED INDEX IX_Incident_TenantId_SiteId ON [SafetyEvents].[Incident] (TenantId, SiteId, IsDeleted);
                CREATE NONCLUSTERED INDEX IX_Incident_TenantId_Status ON [SafetyEvents].[Incident] (TenantId, Status, IsDeleted);
                CREATE NONCLUSTERED INDEX IX_Incident_TenantId_Severity ON [SafetyEvents].[Incident] (TenantId, Severity, IsDeleted);
                CREATE NONCLUSTERED INDEX IX_Incident_TenantId_OccurredAtUtc ON [SafetyEvents].[Incident] (TenantId, IncidentOccurredAtUtc, IsDeleted);
            END;");

        await conn.ExecuteAsync(@"
            CREATE OR ALTER PROCEDURE [SafetyEvents].[usp_Incident_Create]
                @TenantId               UNIQUEIDENTIFIER,
                @CorrelationId          UNIQUEIDENTIFIER,
                @ActorId                UNIQUEIDENTIFIER,
                @NowUtc                 DATETIME2(3),
                @IncidentId             UNIQUEIDENTIFIER,
                @SiteId                 UNIQUEIDENTIFIER,
                @Title                  NVARCHAR(200),
                @Description            NVARCHAR(4000),
                @IncidentOccurredAtUtc  DATETIME2(3),
                @Severity               NVARCHAR(30),
                @Location               NVARCHAR(500),
                @ReportedByWorkerId     UNIQUEIDENTIFIER,
                @AssignedInvestigatorId UNIQUEIDENTIFIER = NULL
            AS
            SET NOCOUNT ON;
            BEGIN
                BEGIN TRY
                    INSERT INTO [SafetyEvents].[Incident]
                    (
                        IncidentId, TenantId, SiteId, Title, Description, IncidentOccurredAtUtc, ReportedAtUtc,
                        Severity, Status, Location, ReportedByWorkerId, AssignedInvestigatorId, ClosedAtUtc,
                        DeletionReason, DeletedAtUtc, IsDeleted, LastModifiedByActorId, LastModifiedAtUtc,
                        CreatedAtUtc, CreatedBy
                    )
                    VALUES
                    (
                        @IncidentId, @TenantId, @SiteId, @Title, @Description, @IncidentOccurredAtUtc, @NowUtc,
                        @Severity, N'Reported', @Location, @ReportedByWorkerId, @AssignedInvestigatorId, NULL,
                        NULL, NULL, 0, @ActorId, @NowUtc,
                        @NowUtc, @ActorId
                    );

                    BEGIN TRY
                        INSERT INTO dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
                        VALUES (N'Incident', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'IncidentId=', @IncidentId, N'; CorrelationId=', @CorrelationId), 0, OBJECT_NAME(@@PROCID));
                    END TRY
                    BEGIN CATCH
                    END CATCH;
                END TRY
                BEGIN CATCH
                    DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'IncidentId=', @IncidentId, N'; CorrelationId=', @CorrelationId, N'; User=', SYSTEM_USER);
                    DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

                    BEGIN TRY
                        INSERT INTO dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
                        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));
                    END TRY
                    BEGIN CATCH
                    END CATCH;

                    THROW;
                END CATCH
            END;");

        await conn.ExecuteAsync(@"
            CREATE OR ALTER PROCEDURE [SafetyEvents].[usp_Incident_Update]
                @TenantId               UNIQUEIDENTIFIER,
                @CorrelationId          UNIQUEIDENTIFIER,
                @ActorId                UNIQUEIDENTIFIER,
                @NowUtc                 DATETIME2(3),
                @IncidentId             UNIQUEIDENTIFIER,
                @Title                  NVARCHAR(200)  = NULL,
                @Description            NVARCHAR(4000) = NULL,
                @Severity               NVARCHAR(30)   = NULL,
                @Location               NVARCHAR(500)  = NULL,
                @AssignedInvestigatorId UNIQUEIDENTIFIER = NULL,
                @ETag                   NVARCHAR(64)
            AS
            SET NOCOUNT ON;
            BEGIN
                BEGIN TRY
                    UPDATE [SafetyEvents].[Incident]
                    SET
                        Title = ISNULL(@Title, Title),
                        Description = ISNULL(@Description, Description),
                        Severity = ISNULL(@Severity, Severity),
                        Location = ISNULL(@Location, Location),
                        AssignedInvestigatorId = COALESCE(@AssignedInvestigatorId, AssignedInvestigatorId),
                        LastModifiedByActorId = @ActorId,
                        LastModifiedAtUtc = @NowUtc
                    WHERE IncidentId = @IncidentId
                      AND TenantId = @TenantId
                      AND IsDeleted = 0
                      AND CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) = @ETag;

                    DECLARE @RowsAffected INT = @@ROWCOUNT;

                    BEGIN TRY
                        INSERT INTO dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
                        VALUES (N'Incident', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'IncidentId=', @IncidentId, N'; CorrelationId=', @CorrelationId), 0, OBJECT_NAME(@@PROCID));
                    END TRY
                    BEGIN CATCH
                    END CATCH;

                    SELECT @RowsAffected;
                END TRY
                BEGIN CATCH
                    DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'IncidentId=', @IncidentId, N'; CorrelationId=', @CorrelationId, N'; User=', SYSTEM_USER);
                    DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

                    BEGIN TRY
                        INSERT INTO dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
                        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));
                    END TRY
                    BEGIN CATCH
                    END CATCH;

                    THROW;
                END CATCH
            END;");

        await conn.ExecuteAsync(@"
            CREATE OR ALTER PROCEDURE [SafetyEvents].[usp_Incident_SoftDelete]
                @TenantId       UNIQUEIDENTIFIER,
                @CorrelationId  UNIQUEIDENTIFIER,
                @ActorId        UNIQUEIDENTIFIER,
                @NowUtc         DATETIME2(3),
                @IncidentId     UNIQUEIDENTIFIER,
                @DeletionReason NVARCHAR(1000),
                @ETag           NVARCHAR(64)
            AS
            SET NOCOUNT ON;
            BEGIN
                BEGIN TRY
                    UPDATE [SafetyEvents].[Incident]
                    SET
                        IsDeleted = 1,
                        Status = N'Archived',
                        DeletionReason = @DeletionReason,
                        DeletedAtUtc = @NowUtc,
                        LastModifiedByActorId = @ActorId,
                        LastModifiedAtUtc = @NowUtc
                    WHERE IncidentId = @IncidentId
                      AND TenantId = @TenantId
                      AND IsDeleted = 0
                      AND CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) = @ETag;

                    DECLARE @RowsAffected INT = @@ROWCOUNT;

                    BEGIN TRY
                        INSERT INTO dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
                        VALUES (N'Incident', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'IncidentId=', @IncidentId, N'; CorrelationId=', @CorrelationId), 0, OBJECT_NAME(@@PROCID));
                    END TRY
                    BEGIN CATCH
                    END CATCH;

                    SELECT @RowsAffected;
                END TRY
                BEGIN CATCH
                    DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'IncidentId=', @IncidentId, N'; CorrelationId=', @CorrelationId, N'; User=', SYSTEM_USER);
                    DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

                    BEGIN TRY
                        INSERT INTO dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
                        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));
                    END TRY
                    BEGIN CATCH
                    END CATCH;

                    THROW;
                END CATCH
            END;");

        await conn.ExecuteAsync(@"
            CREATE OR ALTER PROCEDURE [SafetyEvents].[usp_IncidentRead_GetById]
            (
                @TenantId   UNIQUEIDENTIFIER,
                @IncidentId UNIQUEIDENTIFIER
            )
            AS
            BEGIN
                SET NOCOUNT ON;
                SELECT
                    IncidentId, TenantId, SiteId, Title, Description, IncidentOccurredAtUtc, ReportedAtUtc,
                    Severity, Status, Location, ReportedByWorkerId, AssignedInvestigatorId, ClosedAtUtc,
                    LastModifiedByActorId, LastModifiedAtUtc, CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag,
                    CreatedAtUtc, CreatedBy
                FROM [SafetyEvents].[Incident]
                WHERE IncidentId = @IncidentId AND TenantId = @TenantId AND IsDeleted = 0;
            END;");

        await conn.ExecuteAsync(@"
            CREATE OR ALTER PROCEDURE [SafetyEvents].[usp_IncidentRead_List]
            (
                @TenantId        UNIQUEIDENTIFIER,
                @SiteId          UNIQUEIDENTIFIER = NULL,
                @Status          NVARCHAR(64)     = NULL,
                @Severity        NVARCHAR(64)     = NULL,
                @OccurredFromUtc DATETIME2(3)     = NULL,
                @OccurredToUtc   DATETIME2(3)     = NULL,
                @OrderBy         NVARCHAR(64)     = 'IncidentOccurredAtUtc',
                @SortDirection   NVARCHAR(4)      = 'DESC',
                @PageNumber      INT              = 1,
                @PageSize        INT              = 25
            )
            AS
            BEGIN
                SET NOCOUNT ON;
                SELECT
                    IncidentId, TenantId, SiteId, Title, Description, IncidentOccurredAtUtc, ReportedAtUtc,
                    Severity, Status, Location, ReportedByWorkerId, AssignedInvestigatorId, ClosedAtUtc,
                    LastModifiedByActorId, LastModifiedAtUtc, CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag,
                    CreatedAtUtc, CreatedBy
                FROM [SafetyEvents].[Incident]
                WHERE TenantId = @TenantId
                  AND IsDeleted = 0
                  AND (@SiteId IS NULL OR SiteId = @SiteId)
                  AND (@Status IS NULL OR Status = @Status)
                  AND (@Severity IS NULL OR Severity = @Severity)
                  AND (@OccurredFromUtc IS NULL OR IncidentOccurredAtUtc >= @OccurredFromUtc)
                  AND (@OccurredToUtc IS NULL OR IncidentOccurredAtUtc <= @OccurredToUtc)
                ORDER BY
                    CASE WHEN @OrderBy = 'IncidentOccurredAtUtc' AND @SortDirection = 'ASC' THEN IncidentOccurredAtUtc END ASC,
                    CASE WHEN @OrderBy = 'IncidentOccurredAtUtc' AND @SortDirection = 'DESC' THEN IncidentOccurredAtUtc END DESC,
                    CASE WHEN @OrderBy = 'Status' AND @SortDirection = 'ASC' THEN Status END ASC,
                    CASE WHEN @OrderBy = 'Status' AND @SortDirection = 'DESC' THEN Status END DESC,
                    CASE WHEN @OrderBy = 'Severity' AND @SortDirection = 'ASC' THEN Severity END ASC,
                    CASE WHEN @OrderBy = 'Severity' AND @SortDirection = 'DESC' THEN Severity END DESC,
                    CASE WHEN @OrderBy = 'Title' AND @SortDirection = 'ASC' THEN Title END ASC,
                    CASE WHEN @OrderBy = 'Title' AND @SortDirection = 'DESC' THEN Title END DESC,
                    IncidentOccurredAtUtc DESC
                OFFSET (@PageNumber - 1) * @PageSize ROWS
                FETCH NEXT @PageSize ROWS ONLY;
            END;");

        await conn.ExecuteAsync(@"
            CREATE OR ALTER PROCEDURE [SafetyEvents].[usp_IncidentRead_Search]
            (
                @TenantId   UNIQUEIDENTIFIER,
                @SearchTerm NVARCHAR(200),
                @PageNumber INT = 1,
                @PageSize   INT = 25
            )
            AS
            BEGIN
                SET NOCOUNT ON;
                DECLARE @LikeTerm NVARCHAR(202) = N'%' + @SearchTerm + N'%';
                SELECT
                    IncidentId, TenantId, SiteId, Title, Description, IncidentOccurredAtUtc, ReportedAtUtc,
                    Severity, Status, Location, ReportedByWorkerId, AssignedInvestigatorId, ClosedAtUtc,
                    LastModifiedByActorId, LastModifiedAtUtc, CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag,
                    CreatedAtUtc, CreatedBy
                FROM [SafetyEvents].[Incident]
                WHERE TenantId = @TenantId
                  AND IsDeleted = 0
                  AND (Title LIKE @LikeTerm OR Description LIKE @LikeTerm OR Location LIKE @LikeTerm)
                ORDER BY IncidentOccurredAtUtc DESC, IncidentId ASC
                OFFSET (@PageNumber - 1) * @PageSize ROWS
                FETCH NEXT @PageSize ROWS ONLY;
            END;");

        await conn.ExecuteAsync(@"
            CREATE OR ALTER PROCEDURE [SafetyEvents].[usp_IncidentRead_Count]
            (
                @TenantId        UNIQUEIDENTIFIER,
                @SiteId          UNIQUEIDENTIFIER = NULL,
                @Status          NVARCHAR(64)     = NULL,
                @Severity        NVARCHAR(64)     = NULL,
                @OccurredFromUtc DATETIME2(3)     = NULL,
                @OccurredToUtc   DATETIME2(3)     = NULL
            )
            AS
            BEGIN
                SET NOCOUNT ON;
                SELECT COUNT(1)
                FROM [SafetyEvents].[Incident]
                WHERE TenantId = @TenantId
                  AND IsDeleted = 0
                  AND (@SiteId IS NULL OR SiteId = @SiteId)
                  AND (@Status IS NULL OR Status = @Status)
                  AND (@Severity IS NULL OR Severity = @Severity)
                  AND (@OccurredFromUtc IS NULL OR IncidentOccurredAtUtc >= @OccurredFromUtc)
                  AND (@OccurredToUtc IS NULL OR IncidentOccurredAtUtc <= @OccurredToUtc);
            END;");

        await conn.ExecuteAsync(@"
            CREATE OR ALTER PROCEDURE [SafetyEvents].[usp_IncidentRead_SearchCount]
            (
                @TenantId   UNIQUEIDENTIFIER,
                @SearchTerm NVARCHAR(200)
            )
            AS
            BEGIN
                SET NOCOUNT ON;
                DECLARE @LikeTerm NVARCHAR(202) = N'%' + @SearchTerm + N'%';
                SELECT COUNT(1)
                FROM [SafetyEvents].[Incident]
                WHERE TenantId = @TenantId
                  AND IsDeleted = 0
                  AND (Title LIKE @LikeTerm OR Description LIKE @LikeTerm OR Location LIKE @LikeTerm);
            END;");
    }

    /// <summary>
    /// Creates the <c>[RiskMgmt].[Risk]</c>/<c>[RiskMgmt].[ControlMeasure]</c> tables and their
    /// stored procedures (<c>usp_Risk_Insert/Update/GetById</c>,
    /// <c>usp_ControlMeasure_Upsert/GetByRiskId</c>) against the Testcontainers database,
    /// mirroring PlantSafety.Infrastructure\RiskManagement\Sql\Tables\*.sql and \Procedures\*.sql
    /// verbatim, minus the leading <c>USE [PlantSafety.CoreDB];</c> statements (the Testcontainers
    /// instance exposes a single database, not the real multi-database topology) -- same rationale
    /// as <see cref="ApplySharedAuditLoggingSchemaAsync"/>.
    /// </summary>
    public static async Task ApplyRiskManagementSchemaAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken);

        await conn.ExecuteAsync(@"
            IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'RiskMgmt')
                EXEC('CREATE SCHEMA RiskMgmt AUTHORIZATION dbo;');");

        await conn.ExecuteAsync(@"
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Risk' AND schema_id = SCHEMA_ID('RiskMgmt'))
            BEGIN
                CREATE TABLE [RiskMgmt].[Risk]
                (
                    RiskId              UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Risk PRIMARY KEY,
                    TenantId            UNIQUEIDENTIFIER NOT NULL,
                    SiteId              UNIQUEIDENTIFIER NULL,
                    Status              NVARCHAR(30)     NOT NULL,
                    RiskTitle           NVARCHAR(400)    NOT NULL,
                    Category            NVARCHAR(30)     NOT NULL,
                    HazardDomainCategory NVARCHAR(100)   NOT NULL CONSTRAINT DF_Risk_HazardDomainCategory DEFAULT (N'HazardIdentificationMeasurement'),
                    Location            NVARCHAR(400)    NOT NULL,
                    HazardSource        NVARCHAR(400)    NOT NULL,
                    OwnerId             UNIQUEIDENTIFIER NOT NULL,
                    CurrentSeverity     INT              NULL,
                    CurrentLikelihood   INT              NULL,
                    Priority            NVARCHAR(10)     NULL,
                    DocumentedAtUtc     DATETIME2(3)     NULL,
                    ResidualSeverity    INT              NULL,
                    ResidualLikelihood  INT              NULL,
                    LastValidatedAtUtc  DATETIME2(3)     NULL,
                    ReviewCadence       NVARCHAR(30)     NOT NULL,
                    LastReviewedAtUtc   DATETIME2(3)     NULL,
                    NextReviewDueUtc    DATETIME2(3)     NULL,
                    ReviewRequired      BIT              NOT NULL CONSTRAINT DF_Risk_ReviewRequired DEFAULT (1),
                    ReviewCount         INT              NOT NULL CONSTRAINT DF_Risk_ReviewCount DEFAULT (0),
                    RiskVersion         INT              NOT NULL CONSTRAINT DF_Risk_Version DEFAULT (1),
                    CreatedAtUtc        DATETIME2(3)     NOT NULL,
                    CreatedBy           UNIQUEIDENTIFIER NOT NULL,
                    UpdatedAtUtc        DATETIME2(3)     NULL,
                    UpdatedBy           UNIQUEIDENTIFIER NULL,
                    IsDeleted           BIT              NOT NULL CONSTRAINT DF_Risk_IsDeleted DEFAULT (0),
                    ConcurrencyToken    UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Risk_Concurrency DEFAULT (NEWID())
                );

                CREATE NONCLUSTERED INDEX IX_Risk_TenantId_Category ON [RiskMgmt].[Risk] (TenantId, Category, IsDeleted);
                CREATE NONCLUSTERED INDEX IX_Risk_TenantId_Site ON [RiskMgmt].[Risk] (TenantId, SiteId, IsDeleted);
            END;");

        await conn.ExecuteAsync(@"
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ControlMeasure' AND schema_id = SCHEMA_ID('RiskMgmt'))
            BEGIN
                CREATE TABLE [RiskMgmt].[ControlMeasure]
                (
                    ControlMeasureId    UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_ControlMeasure PRIMARY KEY,
                    RiskId              UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_ControlMeasure_Risk REFERENCES [RiskMgmt].[Risk] (RiskId),
                    TenantId            UNIQUEIDENTIFIER NOT NULL,
                    HierarchyLevel      NVARCHAR(30)     NOT NULL,
                    Description         NVARCHAR(2000)   NOT NULL,
                    Justification       NVARCHAR(2000)   NULL,
                    ResponsiblePersonId UNIQUEIDENTIFIER NOT NULL,
                    ImplementedAtUtc    DATETIME2(3)     NOT NULL,
                    IsDeleted           BIT              NOT NULL CONSTRAINT DF_ControlMeasure_IsDeleted DEFAULT (0)
                );

                CREATE NONCLUSTERED INDEX IX_ControlMeasure_RiskId ON [RiskMgmt].[ControlMeasure] (RiskId, IsDeleted);
            END;");

        await conn.ExecuteAsync(@"
            CREATE OR ALTER PROCEDURE [RiskMgmt].[usp_Risk_Insert]
                @RiskId               UNIQUEIDENTIFIER,
                @TenantId             UNIQUEIDENTIFIER,
                @SiteId               UNIQUEIDENTIFIER = NULL,
                @Status               NVARCHAR(30),
                @RiskTitle            NVARCHAR(400),
                @Category             NVARCHAR(30),
                @HazardDomainCategory NVARCHAR(100),
                @Location             NVARCHAR(400),
                @HazardSource         NVARCHAR(400),
                @OwnerId              UNIQUEIDENTIFIER,
                @CurrentSeverity      INT = NULL,
                @CurrentLikelihood    INT = NULL,
                @Priority             NVARCHAR(10) = NULL,
                @DocumentedAtUtc      DATETIME2(3) = NULL,
                @ResidualSeverity     INT = NULL,
                @ResidualLikelihood   INT = NULL,
                @LastValidatedAtUtc   DATETIME2(3) = NULL,
                @ReviewCadence        NVARCHAR(30),
                @LastReviewedAtUtc    DATETIME2(3) = NULL,
                @NextReviewDueUtc     DATETIME2(3) = NULL,
                @ReviewRequired       BIT,
                @ReviewCount          INT,
                @RiskVersion          INT,
                @CreatedAtUtc         DATETIME2(3),
                @CreatedBy            UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN
                BEGIN TRY
                    INSERT INTO [RiskMgmt].[Risk]
                    (
                        RiskId, TenantId, SiteId, Status, RiskTitle, Category, HazardDomainCategory, Location, HazardSource,
                        OwnerId, CurrentSeverity, CurrentLikelihood, Priority, DocumentedAtUtc, ResidualSeverity,
                        ResidualLikelihood, LastValidatedAtUtc, ReviewCadence, LastReviewedAtUtc, NextReviewDueUtc,
                        ReviewRequired, ReviewCount, RiskVersion, CreatedAtUtc, CreatedBy, IsDeleted
                    )
                    VALUES
                    (
                        @RiskId, @TenantId, @SiteId, @Status, @RiskTitle, @Category, @HazardDomainCategory, @Location, @HazardSource,
                        @OwnerId, @CurrentSeverity, @CurrentLikelihood, @Priority, @DocumentedAtUtc, @ResidualSeverity,
                        @ResidualLikelihood, @LastValidatedAtUtc, @ReviewCadence, @LastReviewedAtUtc, @NextReviewDueUtc,
                        @ReviewRequired, @ReviewCount, @RiskVersion, @CreatedAtUtc, @CreatedBy, 0
                    );

                    BEGIN TRY
                        INSERT INTO dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
                        VALUES (N'Risk', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'RiskId=', @RiskId), 0, OBJECT_NAME(@@PROCID));
                    END TRY
                    BEGIN CATCH
                    END CATCH;
                END TRY
                BEGIN CATCH
                    DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'RiskId=', @RiskId, N'; User=', SYSTEM_USER);
                    DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());
                    THROW;
                END CATCH
            END;");

        await conn.ExecuteAsync(@"
            CREATE OR ALTER PROCEDURE [RiskMgmt].[usp_Risk_Update]
                @RiskId              UNIQUEIDENTIFIER,
                @Status              NVARCHAR(30),
                @Category            NVARCHAR(30),
                @CurrentSeverity     INT = NULL,
                @CurrentLikelihood   INT = NULL,
                @Priority            NVARCHAR(10) = NULL,
                @DocumentedAtUtc     DATETIME2(3) = NULL,
                @ResidualSeverity    INT = NULL,
                @ResidualLikelihood  INT = NULL,
                @LastValidatedAtUtc  DATETIME2(3) = NULL,
                @ReviewCadence       NVARCHAR(30),
                @LastReviewedAtUtc   DATETIME2(3) = NULL,
                @NextReviewDueUtc    DATETIME2(3) = NULL,
                @ReviewCount         INT,
                @RiskVersion         INT,
                @UpdatedBy           UNIQUEIDENTIFIER = NULL
            AS
            SET NOCOUNT ON;
            BEGIN
                BEGIN TRY
                    IF NOT EXISTS (SELECT 1 FROM [RiskMgmt].[Risk] WHERE RiskId = @RiskId AND IsDeleted = 0)
                        THROW 50004, N'Risk not found or has been deleted.', 1;

                    UPDATE [RiskMgmt].[Risk]
                    SET
                        Status = @Status,
                        Category = @Category,
                        CurrentSeverity = @CurrentSeverity,
                        CurrentLikelihood = @CurrentLikelihood,
                        Priority = @Priority,
                        DocumentedAtUtc = @DocumentedAtUtc,
                        ResidualSeverity = @ResidualSeverity,
                        ResidualLikelihood = @ResidualLikelihood,
                        LastValidatedAtUtc = @LastValidatedAtUtc,
                        ReviewCadence = @ReviewCadence,
                        LastReviewedAtUtc = @LastReviewedAtUtc,
                        NextReviewDueUtc = @NextReviewDueUtc,
                        ReviewCount = @ReviewCount,
                        RiskVersion = @RiskVersion,
                        UpdatedBy = @UpdatedBy,
                        UpdatedAtUtc = SYSUTCDATETIME(),
                        ConcurrencyToken = NEWID()
                    WHERE RiskId = @RiskId AND IsDeleted = 0;

                    BEGIN TRY
                        INSERT INTO dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
                        VALUES (N'Risk', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'RiskId=', @RiskId), 0, OBJECT_NAME(@@PROCID));
                    END TRY
                    BEGIN CATCH
                    END CATCH;
                END TRY
                BEGIN CATCH
                    THROW;
                END CATCH
            END;");

        await conn.ExecuteAsync(@"
            CREATE OR ALTER PROCEDURE [RiskMgmt].[usp_Risk_GetById]
                @RiskId UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT
                    RiskId, TenantId, SiteId, Status, RiskTitle, Category, Location, HazardSource, OwnerId,
                    CurrentSeverity, CurrentLikelihood, Priority, DocumentedAtUtc, ResidualSeverity, ResidualLikelihood,
                    LastValidatedAtUtc, ReviewCadence, LastReviewedAtUtc, NextReviewDueUtc, ReviewRequired, ReviewCount,
                    RiskVersion, CreatedAtUtc, CreatedBy
                FROM [RiskMgmt].[Risk]
                WHERE RiskId = @RiskId AND IsDeleted = 0;
            END;");

        await conn.ExecuteAsync(@"
            CREATE OR ALTER PROCEDURE [RiskMgmt].[usp_Risk_GetAllActive]
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT
                    RiskId, TenantId, SiteId, Status, RiskTitle, Category, Location, HazardSource, OwnerId,
                    CurrentSeverity, CurrentLikelihood, Priority, DocumentedAtUtc, ResidualSeverity, ResidualLikelihood,
                    LastValidatedAtUtc, ReviewCadence, LastReviewedAtUtc, NextReviewDueUtc, ReviewRequired, ReviewCount,
                    RiskVersion, CreatedAtUtc, CreatedBy
                FROM [RiskMgmt].[Risk]
                WHERE IsDeleted = 0;
            END;");

        await conn.ExecuteAsync(@"
            CREATE OR ALTER PROCEDURE [RiskMgmt].[usp_ControlMeasure_Upsert]
                @ControlMeasureId    UNIQUEIDENTIFIER,
                @RiskId              UNIQUEIDENTIFIER,
                @TenantId            UNIQUEIDENTIFIER,
                @HierarchyLevel      NVARCHAR(30),
                @Description         NVARCHAR(2000),
                @Justification       NVARCHAR(2000) = NULL,
                @ResponsiblePersonId UNIQUEIDENTIFIER,
                @ImplementedAtUtc    DATETIME2(3)
            AS
            SET NOCOUNT ON;
            BEGIN
                BEGIN TRY
                    DECLARE @__IsInsert BIT = CASE WHEN EXISTS (SELECT 1 FROM [RiskMgmt].[ControlMeasure] WHERE ControlMeasureId = @ControlMeasureId) THEN 0 ELSE 1 END;

                    IF @__IsInsert = 0
                    BEGIN
                        UPDATE [RiskMgmt].[ControlMeasure]
                        SET HierarchyLevel = @HierarchyLevel, Description = @Description, Justification = @Justification
                        WHERE ControlMeasureId = @ControlMeasureId;
                    END
                    ELSE
                    BEGIN
                        INSERT INTO [RiskMgmt].[ControlMeasure]
                            (ControlMeasureId, RiskId, TenantId, HierarchyLevel, Description, Justification, ResponsiblePersonId, ImplementedAtUtc, IsDeleted)
                        VALUES
                            (@ControlMeasureId, @RiskId, @TenantId, @HierarchyLevel, @Description, @Justification, @ResponsiblePersonId, @ImplementedAtUtc, 0);
                    END

                    BEGIN TRY
                        INSERT INTO dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
                        VALUES (N'ControlMeasure', DB_NAME(), CASE WHEN @__IsInsert = 1 THEN N'INSERT' ELSE N'UPDATE' END, SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'ControlMeasureId=', @ControlMeasureId, N'; RiskId=', @RiskId), 0, OBJECT_NAME(@@PROCID));
                    END TRY
                    BEGIN CATCH
                    END CATCH;
                END TRY
                BEGIN CATCH
                    THROW;
                END CATCH
            END;");

        await conn.ExecuteAsync(@"
            CREATE OR ALTER PROCEDURE [RiskMgmt].[usp_ControlMeasure_GetByRiskId]
                @RiskId UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT ControlMeasureId, RiskId, HierarchyLevel, Description, Justification, ResponsiblePersonId, ImplementedAtUtc
                FROM [RiskMgmt].[ControlMeasure]
                WHERE RiskId = @RiskId AND IsDeleted = 0
                ORDER BY ImplementedAtUtc ASC;
            END;");
    }

    /// <summary>
    /// Creates the <c>[AccidentManagement].[Accident]</c>/<c>[AccidentManagement].
    /// [CorrectiveAction]</c> tables and their stored procedures (<c>usp_Accident_Insert/Update/
    /// GetById/GetAllActive</c>, <c>usp_CorrectiveAction_Upsert/GetByAccidentId</c>) against the
    /// Testcontainers database, mirroring PlantSafety.Infrastructure\AccidentManagement\Sql\
    /// Tables\*.sql and \Procedures\*.sql verbatim, minus the leading
    /// <c>USE [PlantSafety.CoreDB];</c> statements -- same rationale as
    /// <see cref="ApplyRiskManagementSchemaAsync"/>. Backs the CorrectiveAction (CAPA) lifecycle
    /// integration coverage, since CAPA items are a child entity of the Accident aggregate
    /// (IAccidentRepository), not a standalone aggregate with their own stored procedures.
    /// </summary>
    public static async Task ApplyAccidentManagementSchemaAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken);

        await conn.ExecuteAsync(@"
            IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'AccidentManagement')
                EXEC('CREATE SCHEMA AccidentManagement AUTHORIZATION dbo;');");

        await conn.ExecuteAsync(@"
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Accident' AND schema_id = SCHEMA_ID('AccidentManagement'))
            BEGIN
                CREATE TABLE [AccidentManagement].[Accident]
                (
                    AccidentId                UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Accident PRIMARY KEY,
                    TenantId                  UNIQUEIDENTIFIER NOT NULL,
                    SiteId                    UNIQUEIDENTIFIER NULL,
                    Status                    NVARCHAR(30)     NOT NULL,
                    AccidentTitle             NVARCHAR(400)    NOT NULL,
                    Location                  NVARCHAR(400)    NOT NULL,
                    Severity                  NVARCHAR(30)     NOT NULL,
                    InjuredPersonCount        INT              NOT NULL CONSTRAINT DF_Accident_InjuredCount DEFAULT (0),
                    OccurredAtUtc             DATETIME2(3)     NOT NULL,
                    Category                  NVARCHAR(100)    NOT NULL,
                    SiteSecuredAndTriaged     BIT              NOT NULL CONSTRAINT DF_Accident_SiteSecured DEFAULT (0),
                    InvestigatorId            UNIQUEIDENTIFIER NULL,
                    InvestigatorAssignedAtUtc DATETIME2(3)     NULL,
                    RootCause                 NVARCHAR(30)     NULL,
                    RootCauseNotes            NVARCHAR(MAX)    NULL,
                    RootCauseRecordedAtUtc    DATETIME2(3)     NULL,
                    RegulatoryFilingRequired  BIT              NOT NULL CONSTRAINT DF_Accident_RegFilingRequired DEFAULT (0),
                    RegulatoryAgency          NVARCHAR(200)    NULL,
                    RegulatoryReferenceNumber NVARCHAR(100)    NULL,
                    RegulatoryFiledAtUtc      DATETIME2(3)     NULL,
                    ClosedAtUtc               DATETIME2(3)     NULL,
                    OwnerId                   UNIQUEIDENTIFIER NOT NULL,
                    ReviewRequired            BIT              NOT NULL CONSTRAINT DF_Accident_ReviewRequired DEFAULT (1),
                    CreatedAtUtc              DATETIME2(3)     NOT NULL,
                    CreatedBy                 UNIQUEIDENTIFIER NOT NULL,
                    UpdatedAtUtc              DATETIME2(3)     NULL,
                    UpdatedBy                 UNIQUEIDENTIFIER NULL,
                    IsDeleted                 BIT              NOT NULL CONSTRAINT DF_Accident_IsDeleted DEFAULT (0),
                    ConcurrencyToken          UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Accident_Concurrency DEFAULT (NEWID())
                );

                CREATE NONCLUSTERED INDEX IX_Accident_TenantId_Category ON [AccidentManagement].[Accident] (TenantId, Category, IsDeleted);
                CREATE NONCLUSTERED INDEX IX_Accident_TenantId_Site ON [AccidentManagement].[Accident] (TenantId, SiteId, IsDeleted);
            END;");

        await conn.ExecuteAsync(@"
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CorrectiveAction' AND schema_id = SCHEMA_ID('AccidentManagement'))
            BEGIN
                CREATE TABLE [AccidentManagement].[CorrectiveAction]
                (
                    CorrectiveActionId  UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_CorrectiveAction PRIMARY KEY,
                    AccidentId          UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_CorrectiveAction_Accident REFERENCES [AccidentManagement].[Accident] (AccidentId),
                    TenantId            UNIQUEIDENTIFIER NOT NULL,
                    Category            NVARCHAR(30)     NOT NULL,
                    Description         NVARCHAR(2000)   NOT NULL,
                    ResponsiblePersonId UNIQUEIDENTIFIER NOT NULL,
                    DueAtUtc            DATETIME2(3)     NOT NULL,
                    Status              NVARCHAR(30)     NOT NULL,
                    VerificationSteps   NVARCHAR(2000)   NULL,
                    CompletionEvidence  NVARCHAR(2000)   NULL,
                    CompletedAtUtc      DATETIME2(3)     NULL,
                    VerifiedAtUtc       DATETIME2(3)     NULL,
                    VerifiedBy          UNIQUEIDENTIFIER NULL,
                    CreatedAtUtc        DATETIME2(3)     NOT NULL,
                    IsDeleted           BIT              NOT NULL CONSTRAINT DF_CorrectiveAction_IsDeleted DEFAULT (0)
                );

                CREATE NONCLUSTERED INDEX IX_CorrectiveAction_AccidentId ON [AccidentManagement].[CorrectiveAction] (AccidentId, IsDeleted);
            END;");

        await conn.ExecuteAsync(@"
            CREATE OR ALTER PROCEDURE [AccidentManagement].[usp_Accident_Insert]
                @AccidentId               UNIQUEIDENTIFIER,
                @TenantId                 UNIQUEIDENTIFIER,
                @SiteId                   UNIQUEIDENTIFIER = NULL,
                @Status                   NVARCHAR(30),
                @AccidentTitle            NVARCHAR(400),
                @Location                 NVARCHAR(400),
                @Severity                 NVARCHAR(30),
                @InjuredPersonCount       INT,
                @OccurredAtUtc            DATETIME2(3),
                @Category                 NVARCHAR(100),
                @SiteSecuredAndTriaged    BIT,
                @InvestigatorId           UNIQUEIDENTIFIER = NULL,
                @InvestigatorAssignedAtUtc DATETIME2(3) = NULL,
                @RootCause                NVARCHAR(30) = NULL,
                @RootCauseNotes           NVARCHAR(MAX) = NULL,
                @RootCauseRecordedAtUtc   DATETIME2(3) = NULL,
                @RegulatoryFilingRequired BIT,
                @RegulatoryAgency         NVARCHAR(200) = NULL,
                @RegulatoryReferenceNumber NVARCHAR(100) = NULL,
                @RegulatoryFiledAtUtc     DATETIME2(3) = NULL,
                @ClosedAtUtc              DATETIME2(3) = NULL,
                @OwnerId                  UNIQUEIDENTIFIER,
                @ReviewRequired           BIT,
                @CreatedAtUtc             DATETIME2(3),
                @CreatedBy                UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN
                BEGIN TRY
                    INSERT INTO [AccidentManagement].[Accident]
                    (
                        AccidentId, TenantId, SiteId, Status, AccidentTitle, Location, Severity, InjuredPersonCount,
                        OccurredAtUtc, Category, SiteSecuredAndTriaged, InvestigatorId, InvestigatorAssignedAtUtc,
                        RootCause, RootCauseNotes, RootCauseRecordedAtUtc, RegulatoryFilingRequired, RegulatoryAgency,
                        RegulatoryReferenceNumber, RegulatoryFiledAtUtc, ClosedAtUtc, OwnerId, ReviewRequired,
                        CreatedAtUtc, CreatedBy, IsDeleted
                    )
                    VALUES
                    (
                        @AccidentId, @TenantId, @SiteId, @Status, @AccidentTitle, @Location, @Severity, @InjuredPersonCount,
                        @OccurredAtUtc, @Category, @SiteSecuredAndTriaged, @InvestigatorId, @InvestigatorAssignedAtUtc,
                        @RootCause, @RootCauseNotes, @RootCauseRecordedAtUtc, @RegulatoryFilingRequired, @RegulatoryAgency,
                        @RegulatoryReferenceNumber, @RegulatoryFiledAtUtc, @ClosedAtUtc, @OwnerId, @ReviewRequired,
                        @CreatedAtUtc, @CreatedBy, 0
                    );

                    BEGIN TRY
                        INSERT INTO dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
                        VALUES (N'Accident', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'AccidentId=', @AccidentId), 0, OBJECT_NAME(@@PROCID));
                    END TRY
                    BEGIN CATCH
                    END CATCH;
                END TRY
                BEGIN CATCH
                    THROW;
                END CATCH
            END;");

        await conn.ExecuteAsync(@"
            CREATE OR ALTER PROCEDURE [AccidentManagement].[usp_Accident_Update]
                @AccidentId                UNIQUEIDENTIFIER,
                @TenantId                  UNIQUEIDENTIFIER,
                @Status                    NVARCHAR(30),
                @Category                  NVARCHAR(100),
                @SiteSecuredAndTriaged     BIT,
                @InvestigatorId            UNIQUEIDENTIFIER = NULL,
                @InvestigatorAssignedAtUtc DATETIME2(3) = NULL,
                @RootCause                 NVARCHAR(30) = NULL,
                @RootCauseNotes            NVARCHAR(MAX) = NULL,
                @RootCauseRecordedAtUtc    DATETIME2(3) = NULL,
                @RegulatoryAgency          NVARCHAR(200) = NULL,
                @RegulatoryReferenceNumber NVARCHAR(100) = NULL,
                @RegulatoryFiledAtUtc      DATETIME2(3) = NULL,
                @ClosedAtUtc               DATETIME2(3) = NULL,
                @UpdatedBy                 UNIQUEIDENTIFIER = NULL
            AS
            SET NOCOUNT ON;
            BEGIN
                BEGIN TRY
                    IF NOT EXISTS (SELECT 1 FROM [AccidentManagement].[Accident] WHERE AccidentId = @AccidentId AND TenantId = @TenantId AND IsDeleted = 0)
                        THROW 50004, N'Accident not found or has been deleted.', 1;

                    UPDATE [AccidentManagement].[Accident]
                    SET
                        Status = @Status,
                        Category = @Category,
                        SiteSecuredAndTriaged = @SiteSecuredAndTriaged,
                        InvestigatorId = @InvestigatorId,
                        InvestigatorAssignedAtUtc = @InvestigatorAssignedAtUtc,
                        RootCause = @RootCause,
                        RootCauseNotes = @RootCauseNotes,
                        RootCauseRecordedAtUtc = @RootCauseRecordedAtUtc,
                        RegulatoryAgency = @RegulatoryAgency,
                        RegulatoryReferenceNumber = @RegulatoryReferenceNumber,
                        RegulatoryFiledAtUtc = @RegulatoryFiledAtUtc,
                        ClosedAtUtc = @ClosedAtUtc,
                        UpdatedBy = @UpdatedBy,
                        UpdatedAtUtc = SYSUTCDATETIME(),
                        ConcurrencyToken = NEWID()
                    WHERE AccidentId = @AccidentId AND TenantId = @TenantId AND IsDeleted = 0;

                    BEGIN TRY
                        INSERT INTO dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
                        VALUES (N'Accident', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'AccidentId=', @AccidentId), 0, OBJECT_NAME(@@PROCID));
                    END TRY
                    BEGIN CATCH
                    END CATCH;
                END TRY
                BEGIN CATCH
                    THROW;
                END CATCH
            END;");

        await conn.ExecuteAsync(@"
            CREATE OR ALTER PROCEDURE [AccidentManagement].[usp_Accident_GetById]
                @AccidentId UNIQUEIDENTIFIER,
                @TenantId UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT
                    AccidentId, TenantId, SiteId, Status, AccidentTitle, Location, Severity, InjuredPersonCount,
                    OccurredAtUtc, Category, SiteSecuredAndTriaged, InvestigatorId, InvestigatorAssignedAtUtc,
                    RootCause, RootCauseNotes, RootCauseRecordedAtUtc, RegulatoryFilingRequired, RegulatoryAgency,
                    RegulatoryReferenceNumber, RegulatoryFiledAtUtc, ClosedAtUtc, OwnerId, ReviewRequired,
                    CreatedAtUtc, CreatedBy
                FROM [AccidentManagement].[Accident]
                WHERE AccidentId = @AccidentId AND TenantId = @TenantId AND IsDeleted = 0;
            END;");

        await conn.ExecuteAsync(@"
            CREATE OR ALTER PROCEDURE [AccidentManagement].[usp_Accident_GetAllActive]
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT
                    AccidentId, TenantId, SiteId, Status, AccidentTitle, Location, Severity, InjuredPersonCount,
                    OccurredAtUtc, Category, SiteSecuredAndTriaged, InvestigatorId, InvestigatorAssignedAtUtc,
                    RootCause, RootCauseNotes, RootCauseRecordedAtUtc, RegulatoryFilingRequired, RegulatoryAgency,
                    RegulatoryReferenceNumber, RegulatoryFiledAtUtc, ClosedAtUtc, OwnerId, ReviewRequired,
                    CreatedAtUtc, CreatedBy
                FROM [AccidentManagement].[Accident]
                WHERE IsDeleted = 0;
            END;");

        await conn.ExecuteAsync(@"
            CREATE OR ALTER PROCEDURE [AccidentManagement].[usp_CorrectiveAction_Upsert]
                @CorrectiveActionId  UNIQUEIDENTIFIER,
                @AccidentId          UNIQUEIDENTIFIER,
                @TenantId            UNIQUEIDENTIFIER,
                @Category            NVARCHAR(30),
                @Description         NVARCHAR(2000),
                @ResponsiblePersonId UNIQUEIDENTIFIER,
                @DueAtUtc            DATETIME2(3),
                @Status              NVARCHAR(30),
                @VerificationSteps   NVARCHAR(2000) = NULL,
                @CompletionEvidence  NVARCHAR(2000) = NULL,
                @CompletedAtUtc      DATETIME2(3) = NULL,
                @VerifiedAtUtc       DATETIME2(3) = NULL,
                @VerifiedBy          UNIQUEIDENTIFIER = NULL,
                @CreatedAtUtc        DATETIME2(3)
            AS
            SET NOCOUNT ON;
            BEGIN
                BEGIN TRY
                    DECLARE @__IsInsert BIT = CASE WHEN EXISTS (SELECT 1 FROM [AccidentManagement].[CorrectiveAction] WHERE CorrectiveActionId = @CorrectiveActionId) THEN 0 ELSE 1 END;

                    IF @__IsInsert = 0
                    BEGIN
                        UPDATE [AccidentManagement].[CorrectiveAction]
                        SET
                            Status = @Status,
                            VerificationSteps = @VerificationSteps,
                            CompletionEvidence = @CompletionEvidence,
                            CompletedAtUtc = @CompletedAtUtc,
                            VerifiedAtUtc = @VerifiedAtUtc,
                            VerifiedBy = @VerifiedBy
                        WHERE CorrectiveActionId = @CorrectiveActionId;
                    END
                    ELSE
                    BEGIN
                        INSERT INTO [AccidentManagement].[CorrectiveAction]
                        (
                            CorrectiveActionId, AccidentId, TenantId, Category, Description, ResponsiblePersonId, DueAtUtc,
                            Status, VerificationSteps, CompletionEvidence, CompletedAtUtc, VerifiedAtUtc, VerifiedBy,
                            CreatedAtUtc, IsDeleted
                        )
                        VALUES
                        (
                            @CorrectiveActionId, @AccidentId, @TenantId, @Category, @Description, @ResponsiblePersonId, @DueAtUtc,
                            @Status, @VerificationSteps, @CompletionEvidence, @CompletedAtUtc, @VerifiedAtUtc, @VerifiedBy,
                            @CreatedAtUtc, 0
                        );
                    END

                    BEGIN TRY
                        INSERT INTO dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
                        VALUES (N'CorrectiveAction', DB_NAME(), CASE WHEN @__IsInsert = 1 THEN N'INSERT' ELSE N'UPDATE' END, SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'CorrectiveActionId=', @CorrectiveActionId, N'; AccidentId=', @AccidentId), 0, OBJECT_NAME(@@PROCID));
                    END TRY
                    BEGIN CATCH
                    END CATCH;
                END TRY
                BEGIN CATCH
                    THROW;
                END CATCH
            END;");

        await conn.ExecuteAsync(@"
            CREATE OR ALTER PROCEDURE [AccidentManagement].[usp_CorrectiveAction_GetByAccidentId]
                @AccidentId UNIQUEIDENTIFIER,
                @TenantId UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT
                    CorrectiveActionId, AccidentId, Category, Description, ResponsiblePersonId, DueAtUtc, Status,
                    VerificationSteps, CompletionEvidence, CompletedAtUtc, VerifiedAtUtc, VerifiedBy, CreatedAtUtc
                FROM [AccidentManagement].[CorrectiveAction]
                WHERE AccidentId = @AccidentId AND TenantId = @TenantId AND IsDeleted = 0
                ORDER BY CreatedAtUtc ASC;
            END;");
    }

    public static async Task ApplyComplianceSchemaAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken);

        await conn.ExecuteAsync(@"
            IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Compliance')
                EXEC('CREATE SCHEMA Compliance AUTHORIZATION dbo;');");

        // Table: AuditFindings.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AuditFindings' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[AuditFindings] (
        FindingId       UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        AuditingId      UNIQUEIDENTIFIER NOT NULL,
        TenantId        UNIQUEIDENTIFIER NOT NULL,
        SiteId          UNIQUEIDENTIFIER NULL,
        Title           NVARCHAR(200)    NOT NULL,
        Severity        NVARCHAR(50)     NOT NULL,
        Status          NVARCHAR(50)     NOT NULL,
        IdentifiedAtUtc DATETIME2(3)     NOT NULL,
        IdentifiedBy    UNIQUEIDENTIFIER NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_AuditFindings_TenantId_SiteId] ON [Compliance].[AuditFindings] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_AuditFindings_IdentifiedAtUtc] ON [Compliance].[AuditFindings] (IdentifiedAtUtc);
    CREATE NONCLUSTERED INDEX [IX_AuditFindings_AuditingId] ON [Compliance].[AuditFindings] (AuditingId);
END
");

        // Table: CAPATriggers.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CAPATriggers' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[CAPATriggers] (
        TriggerId       UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        AuditingId      UNIQUEIDENTIFIER NOT NULL,
        SourceFindingId UNIQUEIDENTIFIER NULL,
        TenantId        UNIQUEIDENTIFIER NOT NULL,
        SiteId          UNIQUEIDENTIFIER NULL,
        Reason          NVARCHAR(500)    NOT NULL,
        Status          NVARCHAR(50)     NOT NULL,
        TriggeredAtUtc  DATETIME2(3)     NOT NULL,
        TriggeredBy     UNIQUEIDENTIFIER NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_CAPATriggers_TenantId_SiteId] ON [Compliance].[CAPATriggers] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_CAPATriggers_TriggeredAtUtc] ON [Compliance].[CAPATriggers] (TriggeredAtUtc);
    CREATE NONCLUSTERED INDEX [IX_CAPATriggers_AuditingId] ON [Compliance].[CAPATriggers] (AuditingId);
END
");

        // Table: CertificationContractor.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CertificationContractor' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[CertificationContractor] (
        CertificationId   UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId          UNIQUEIDENTIFIER NOT NULL,
        SiteId            UNIQUEIDENTIFIER NULL,
        ContractorName    NVARCHAR(200)    NOT NULL,
        CertificationType NVARCHAR(100)    NOT NULL,
        Status            NVARCHAR(50)     NOT NULL,
        IssuedAtUtc       DATETIME2(3)     NOT NULL,
        ExpiresAtUtc      DATETIME2(3)     NOT NULL,
        VerifiedBy        UNIQUEIDENTIFIER NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_CertificationContractor_TenantId_SiteId] ON [Compliance].[CertificationContractor] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_CertificationContractor_IssuedAtUtc] ON [Compliance].[CertificationContractor] (IssuedAtUtc);
END
");

        // Table: CertificationDOT.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CertificationDOT' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[CertificationDOT] (
        CertificationId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId        UNIQUEIDENTIFIER NOT NULL,
        SiteId          UNIQUEIDENTIFIER NULL,
        DriverName      NVARCHAR(200)    NOT NULL,
        LicenseClass    NVARCHAR(50)     NOT NULL,
        Status          NVARCHAR(50)     NOT NULL,
        IssuedAtUtc     DATETIME2(3)     NOT NULL,
        ExpiresAtUtc    DATETIME2(3)     NOT NULL,
        VerifiedBy      UNIQUEIDENTIFIER NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_CertificationDOT_TenantId_SiteId] ON [Compliance].[CertificationDOT] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_CertificationDOT_IssuedAtUtc] ON [Compliance].[CertificationDOT] (IssuedAtUtc);
END
");

        // Table: CertificationEPA.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CertificationEPA' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[CertificationEPA] (
        CertificationId  UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId         UNIQUEIDENTIFIER NOT NULL,
        SiteId           UNIQUEIDENTIFIER NULL,
        PermitHolderName NVARCHAR(200)    NOT NULL,
        EpaProgram       NVARCHAR(100)    NOT NULL,
        Status           NVARCHAR(50)     NOT NULL,
        IssuedAtUtc      DATETIME2(3)     NOT NULL,
        ExpiresAtUtc     DATETIME2(3)     NOT NULL,
        VerifiedBy       UNIQUEIDENTIFIER NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_CertificationEPA_TenantId_SiteId] ON [Compliance].[CertificationEPA] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_CertificationEPA_IssuedAtUtc] ON [Compliance].[CertificationEPA] (IssuedAtUtc);
END
");

        // Table: CertificationIndustrySpecific.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CertificationIndustrySpecific' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[CertificationIndustrySpecific] (
        CertificationId  UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId         UNIQUEIDENTIFIER NOT NULL,
        SiteId           UNIQUEIDENTIFIER NULL,
        HolderName       NVARCHAR(200)    NOT NULL,
        IndustryStandard NVARCHAR(150)    NOT NULL,
        Status           NVARCHAR(50)     NOT NULL,
        IssuedAtUtc      DATETIME2(3)     NOT NULL,
        ExpiresAtUtc     DATETIME2(3)     NOT NULL,
        VerifiedBy       UNIQUEIDENTIFIER NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_CertificationIndustrySpecific_TenantId_SiteId] ON [Compliance].[CertificationIndustrySpecific] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_CertificationIndustrySpecific_IssuedAtUtc] ON [Compliance].[CertificationIndustrySpecific] (IssuedAtUtc);
END
");

        // Table: CertificationISO14001.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CertificationISO14001' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[CertificationISO14001] (
        CertificationId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId        UNIQUEIDENTIFIER NOT NULL,
        SiteId          UNIQUEIDENTIFIER NULL,
        CertifyingBody  NVARCHAR(200)    NOT NULL,
        Scope           NVARCHAR(200)    NOT NULL,
        Status          NVARCHAR(50)     NOT NULL,
        IssuedAtUtc     DATETIME2(3)     NOT NULL,
        ExpiresAtUtc    DATETIME2(3)     NOT NULL,
        VerifiedBy      UNIQUEIDENTIFIER NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_CertificationISO14001_TenantId_SiteId] ON [Compliance].[CertificationISO14001] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_CertificationISO14001_IssuedAtUtc] ON [Compliance].[CertificationISO14001] (IssuedAtUtc);
END
");

        // Table: CertificationISO45001.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CertificationISO45001' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[CertificationISO45001] (
        CertificationId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId        UNIQUEIDENTIFIER NOT NULL,
        SiteId          UNIQUEIDENTIFIER NULL,
        CertifyingBody  NVARCHAR(200)    NOT NULL,
        Scope           NVARCHAR(200)    NOT NULL,
        Status          NVARCHAR(50)     NOT NULL,
        IssuedAtUtc     DATETIME2(3)     NOT NULL,
        ExpiresAtUtc    DATETIME2(3)     NOT NULL,
        VerifiedBy      UNIQUEIDENTIFIER NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_CertificationISO45001_TenantId_SiteId] ON [Compliance].[CertificationISO45001] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_CertificationISO45001_IssuedAtUtc] ON [Compliance].[CertificationISO45001] (IssuedAtUtc);
END
");

        // Table: CertificationOSHACertifications.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CertificationOSHACertifications' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[CertificationOSHACertifications] (
        CertificationId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId        UNIQUEIDENTIFIER NOT NULL,
        SiteId          UNIQUEIDENTIFIER NULL,
        HolderName      NVARCHAR(200)    NOT NULL,
        OshaStandard    NVARCHAR(100)    NOT NULL,
        Status          NVARCHAR(50)     NOT NULL,
        IssuedAtUtc     DATETIME2(3)     NOT NULL,
        ExpiresAtUtc    DATETIME2(3)     NOT NULL,
        VerifiedBy      UNIQUEIDENTIFIER NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_CertificationOSHACertifications_TenantId_SiteId] ON [Compliance].[CertificationOSHACertifications] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_CertificationOSHACertifications_IssuedAtUtc] ON [Compliance].[CertificationOSHACertifications] (IssuedAtUtc);
END
");

        // Table: CertificationTraining.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CertificationTraining' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[CertificationTraining] (
        TrainingRecordId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId         UNIQUEIDENTIFIER NOT NULL,
        SiteId           UNIQUEIDENTIFIER NULL,
        EmployeeName     NVARCHAR(200)    NOT NULL,
        CourseName       NVARCHAR(200)    NOT NULL,
        Status           NVARCHAR(50)     NOT NULL,
        CompletedAtUtc   DATETIME2(3)     NOT NULL,
        ExpiresAtUtc     DATETIME2(3)     NOT NULL,
        InstructorId     UNIQUEIDENTIFIER NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_CertificationTraining_TenantId_SiteId] ON [Compliance].[CertificationTraining] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_CertificationTraining_CompletedAtUtc] ON [Compliance].[CertificationTraining] (CompletedAtUtc);
END
");

        // Table: ComplianceAudit.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ComplianceAudit' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[ComplianceAudit] (
        AuditingId      UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId        UNIQUEIDENTIFIER NOT NULL,
        SiteId          UNIQUEIDENTIFIER NULL,
        AuditType       NVARCHAR(100)    NOT NULL,
        Status          NVARCHAR(50)     NOT NULL,
        ConductedAtUtc  DATETIME2(3)     NOT NULL,
        ConductedBy     UNIQUEIDENTIFIER NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_ComplianceAudit_TenantId_SiteId] ON [Compliance].[ComplianceAudit] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_ComplianceAudit_ConductedAtUtc] ON [Compliance].[ComplianceAudit] (ConductedAtUtc);
END
");

        // Table: ComplianceExportItems.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ComplianceExportItems' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[ComplianceExportItems] (
        [Id]                UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]          UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]            UNIQUEIDENTIFIER    NULL,
        [ResourceType]      NVARCHAR(100)       NOT NULL,
        [ResourceId]        UNIQUEIDENTIFIER    NOT NULL,
        [Title]             NVARCHAR(300)       NOT NULL,
        [Status]            NVARCHAR(50)        NOT NULL,
        [EffectiveAtUtc]    DATETIME2(3)        NOT NULL,
        [OwnerId]           UNIQUEIDENTIFIER    NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_ComplianceExportItems_TenantId_EffectiveAtUtc]
        ON [Compliance].[ComplianceExportItems] ([TenantId], [EffectiveAtUtc])
        INCLUDE ([SiteId], [Status], [ResourceType]);
END
");

        // Table: ComplianceResources.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ComplianceResources' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[ComplianceResources] (
        Id               UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId         UNIQUEIDENTIFIER NOT NULL,
        SiteId           UNIQUEIDENTIFIER NULL,
        ResourceType     NVARCHAR(100)    NOT NULL,
        Status           NVARCHAR(50)     NOT NULL,
        AssignedToUserId UNIQUEIDENTIFIER NULL,
        Notes            NVARCHAR(MAX)    NULL,
        UpdatedAtUtc     DATETIME2(3)     NOT NULL,
        UpdatedBy        UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_ComplianceResources_TenantId_SiteId] ON [Compliance].[ComplianceResources] (TenantId, SiteId);
END
");

        // Table: DeadlinesAnnualAudit.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'DeadlinesAnnualAudit' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[DeadlinesAnnualAudit] (
        [DeadlineId]              UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]                UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                  UNIQUEIDENTIFIER    NULL,
        [AuditScope]              NVARCHAR(300)       NOT NULL,
        [DueAtUtc]                DATETIME2(3)        NOT NULL,
        [Status]                  NVARCHAR(50)        NOT NULL,
        [ResponsibleParty]        UNIQUEIDENTIFIER    NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_DeadlinesAnnualAudit_TenantId_DueAtUtc]
        ON [Compliance].[DeadlinesAnnualAudit] ([TenantId], [DueAtUtc])
        INCLUDE ([SiteId]);
END
");

        // Table: DeadlinesEPA.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'DeadlinesEPA' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[DeadlinesEPA] (
        [DeadlineId]              UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]                UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                  UNIQUEIDENTIFIER    NULL,
        [EPARegulation]           NVARCHAR(300)       NOT NULL,
        [DueAtUtc]                DATETIME2(3)        NOT NULL,
        [Status]                  NVARCHAR(50)        NOT NULL,
        [ResponsibleParty]        UNIQUEIDENTIFIER    NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_DeadlinesEPA_TenantId_DueAtUtc]
        ON [Compliance].[DeadlinesEPA] ([TenantId], [DueAtUtc])
        INCLUDE ([SiteId]);
END
");

        // Table: InspectionsDOT.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'InspectionsDOT' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[InspectionsDOT] (
        [InspectionId]            UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]                UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                  UNIQUEIDENTIFIER    NULL,
        [InspectionScope]         NVARCHAR(300)       NOT NULL,
        [ScheduledAtUtc]          DATETIME2(3)        NOT NULL,
        [Status]                  NVARCHAR(50)        NOT NULL,
        [Inspector]               UNIQUEIDENTIFIER    NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_InspectionsDOT_TenantId_ScheduledAtUtc]
        ON [Compliance].[InspectionsDOT] ([TenantId], [ScheduledAtUtc])
        INCLUDE ([SiteId]);
END
");

        // Table: InspectionsEnvironmentalSampling.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'InspectionsEnvironmentalSampling' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[InspectionsEnvironmentalSampling] (
        [InspectionId]            UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]                UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                  UNIQUEIDENTIFIER    NULL,
        [SampleType]              NVARCHAR(300)       NOT NULL,
        [ScheduledAtUtc]          DATETIME2(3)        NOT NULL,
        [Status]                  NVARCHAR(50)        NOT NULL,
        [Inspector]               UNIQUEIDENTIFIER    NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_InspectionsEnvironmentalSampling_TenantId_ScheduledAtUtc]
        ON [Compliance].[InspectionsEnvironmentalSampling] ([TenantId], [ScheduledAtUtc])
        INCLUDE ([SiteId]);
END
");

        // Table: InspectionsEquipment.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'InspectionsEquipment' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[InspectionsEquipment] (
        [InspectionId]            UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]                UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                  UNIQUEIDENTIFIER    NULL,
        [EquipmentCategory]       NVARCHAR(300)       NOT NULL,
        [ScheduledAtUtc]          DATETIME2(3)        NOT NULL,
        [Status]                  NVARCHAR(50)        NOT NULL,
        [Inspector]               UNIQUEIDENTIFIER    NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_InspectionsEquipment_TenantId_ScheduledAtUtc]
        ON [Compliance].[InspectionsEquipment] ([TenantId], [ScheduledAtUtc])
        INCLUDE ([SiteId]);
END
");

        // Table: InspectionsOSHA.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'InspectionsOSHA' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[InspectionsOSHA] (
        [InspectionId]            UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]                UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                  UNIQUEIDENTIFIER    NULL,
        [InspectionScope]         NVARCHAR(300)       NOT NULL,
        [ScheduledAtUtc]          DATETIME2(3)        NOT NULL,
        [Status]                  NVARCHAR(50)        NOT NULL,
        [Inspector]               UNIQUEIDENTIFIER    NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_InspectionsOSHA_TenantId_ScheduledAtUtc]
        ON [Compliance].[InspectionsOSHA] ([TenantId], [ScheduledAtUtc])
        INCLUDE ([SiteId]);
END
");

        // Table: InternalSafetyAudit.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'InternalSafetyAudit' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[InternalSafetyAudit] (
        AuditingId      UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId        UNIQUEIDENTIFIER NOT NULL,
        SiteId          UNIQUEIDENTIFIER NULL,
        Scope           NVARCHAR(200)    NOT NULL,
        Status          NVARCHAR(50)     NOT NULL,
        ConductedAtUtc  DATETIME2(3)     NOT NULL,
        ConductedBy     UNIQUEIDENTIFIER NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_InternalSafetyAudit_TenantId_SiteId] ON [Compliance].[InternalSafetyAudit] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_InternalSafetyAudit_ConductedAtUtc] ON [Compliance].[InternalSafetyAudit] (ConductedAtUtc);
END
");

        // Table: Moc.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Moc' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[Moc] (
        MocId              UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Moc PRIMARY KEY,
        TenantId            UNIQUEIDENTIFIER NOT NULL,
        SiteId              UNIQUEIDENTIFIER NOT NULL,
        Title               NVARCHAR(200)    NOT NULL,
        Description         NVARCHAR(4000)   NOT NULL,
        ChangeType          NVARCHAR(50)     NOT NULL,
        RiskLevel           NVARCHAR(50)     NOT NULL,
        InitiatorId         UNIQUEIDENTIFIER NOT NULL,
        ProposedStartUtc    DATETIME2(3)     NOT NULL,
        ProposedEndUtc      DATETIME2(3)     NULL,
        Status              NVARCHAR(30)     NOT NULL CONSTRAINT DF_Moc_Status DEFAULT (N'Draft'),
        ApprovalNotes       NVARCHAR(2000)   NULL,
        ApprovedByActorId   UNIQUEIDENTIFIER NULL,
        ApprovedAtUtc       DATETIME2(3)     NULL,
        LastModifiedAtUtc   DATETIME2(3)     NOT NULL,
        CreatedAtUtc        DATETIME2(3)     NOT NULL,
        CreatedBy           UNIQUEIDENTIFIER NOT NULL,
        IsDeleted           BIT              NOT NULL CONSTRAINT DF_Moc_IsDeleted DEFAULT (0),
        RowVersion          ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_Moc_TenantId_SiteId] ON [Compliance].[Moc] (TenantId, SiteId, IsDeleted);
    CREATE NONCLUSTERED INDEX [IX_Moc_TenantId_Status] ON [Compliance].[Moc] (TenantId, Status, IsDeleted);
END
");

        // Table: MOCApprovals.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'MOCApprovals' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[MOCApprovals] (
        ApprovalId      UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        ChangeRequestId UNIQUEIDENTIFIER NOT NULL,
        TenantId        UNIQUEIDENTIFIER NOT NULL,
        SiteId          UNIQUEIDENTIFIER NULL,
        ApproverName    NVARCHAR(200)    NOT NULL,
        Status          NVARCHAR(50)     NOT NULL,
        DecidedAtUtc    DATETIME2(3)     NOT NULL,
        ApprovedBy      UNIQUEIDENTIFIER NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_MOCApprovals_TenantId_SiteId] ON [Compliance].[MOCApprovals] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_MOCApprovals_DecidedAtUtc] ON [Compliance].[MOCApprovals] (DecidedAtUtc);
    CREATE NONCLUSTERED INDEX [IX_MOCApprovals_ChangeRequestId] ON [Compliance].[MOCApprovals] (ChangeRequestId);
END
");

        // Table: MOCRiskAssessments.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'MOCRiskAssessments' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[MOCRiskAssessments] (
        RiskAssessmentId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        ChangeRequestId  UNIQUEIDENTIFIER NOT NULL,
        TenantId         UNIQUEIDENTIFIER NOT NULL,
        SiteId           UNIQUEIDENTIFIER NULL,
        RiskLevel        NVARCHAR(50)     NOT NULL,
        Status           NVARCHAR(50)     NOT NULL,
        AssessedAtUtc    DATETIME2(3)     NOT NULL,
        AssessedBy       UNIQUEIDENTIFIER NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_MOCRiskAssessments_TenantId_SiteId] ON [Compliance].[MOCRiskAssessments] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_MOCRiskAssessments_AssessedAtUtc] ON [Compliance].[MOCRiskAssessments] (AssessedAtUtc);
    CREATE NONCLUSTERED INDEX [IX_MOCRiskAssessments_ChangeRequestId] ON [Compliance].[MOCRiskAssessments] (ChangeRequestId);
END
");

        // Table: MOCWorkflows.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'MOCWorkflows' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[MOCWorkflows] (
        WorkflowId    UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId      UNIQUEIDENTIFIER NOT NULL,
        SiteId        UNIQUEIDENTIFIER NULL,
        ChangeTitle   NVARCHAR(300)    NOT NULL,
        CurrentStage  NVARCHAR(100)    NOT NULL,
        Status        NVARCHAR(50)     NOT NULL,
        InitiatedAtUtc DATETIME2(3)    NOT NULL,
        InitiatedBy   UNIQUEIDENTIFIER NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_MOCWorkflows_TenantId_SiteId] ON [Compliance].[MOCWorkflows] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_MOCWorkflows_InitiatedAtUtc] ON [Compliance].[MOCWorkflows] (InitiatedAtUtc);
END
");

        // Table: Obligations.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Obligations' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[Obligations] (
        ObligationId    UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId        UNIQUEIDENTIFIER NOT NULL,
        SiteId          UNIQUEIDENTIFIER NULL,
        ObligationTitle NVARCHAR(300)    NOT NULL,
        RegulatoryBody  NVARCHAR(200)    NOT NULL,
        Status          NVARCHAR(50)     NOT NULL,
        DueAtUtc        DATETIME2(3)     NOT NULL,
        OwnerId         UNIQUEIDENTIFIER NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_Obligations_TenantId_SiteId] ON [Compliance].[Obligations] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_Obligations_DueAtUtc] ON [Compliance].[Obligations] (DueAtUtc);
END
");

        // Table: Permit.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Permit' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[Permit] (
        PermitId            UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Permit PRIMARY KEY,
        TenantId             UNIQUEIDENTIFIER NOT NULL,
        SiteId               UNIQUEIDENTIFIER NOT NULL,
        PermitType           NVARCHAR(100)    NOT NULL,
        PermitNumber         NVARCHAR(50)     NOT NULL,
        WorkDescription      NVARCHAR(2000)   NOT NULL,
        WorkLocation         NVARCHAR(500)    NOT NULL,
        WorkLeadId           UNIQUEIDENTIFIER NOT NULL,
        ValidFromUtc         DATETIME2(3)     NOT NULL,
        ValidToUtc           DATETIME2(3)     NOT NULL,
        Status               NVARCHAR(30)     NOT NULL CONSTRAINT DF_Permit_Status DEFAULT (N'Draft'),
        ApprovalNotes        NVARCHAR(2000)   NULL,
        ApprovedByActorId    UNIQUEIDENTIFIER NULL,
        ApprovedAtUtc        DATETIME2(3)     NULL,
        LastModifiedAtUtc    DATETIME2(3)     NOT NULL,
        CreatedAtUtc         DATETIME2(3)     NOT NULL,
        CreatedBy            UNIQUEIDENTIFIER NOT NULL,
        IsDeleted            BIT              NOT NULL CONSTRAINT DF_Permit_IsDeleted DEFAULT (0),
        RowVersion           ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_Permit_TenantId_SiteId] ON [Compliance].[Permit] (TenantId, SiteId, IsDeleted);
    CREATE NONCLUSTERED INDEX [IX_Permit_TenantId_Status] ON [Compliance].[Permit] (TenantId, Status, IsDeleted);
    CREATE UNIQUE NONCLUSTERED INDEX [IX_Permit_PermitNumber] ON [Compliance].[Permit] (PermitNumber);
END
");

        // Table: PermitChemicalHandling.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PermitChemicalHandling' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[PermitChemicalHandling] (
        PermitId      UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId      UNIQUEIDENTIFIER NOT NULL,
        SiteId        UNIQUEIDENTIFIER NULL,
        ChemicalName  NVARCHAR(200)    NOT NULL,
        PermitHolder  NVARCHAR(200)    NOT NULL,
        Status        NVARCHAR(50)     NOT NULL,
        IssuedAtUtc   DATETIME2(3)     NOT NULL,
        ExpiresAtUtc  DATETIME2(3)     NOT NULL,
        ApprovedBy    UNIQUEIDENTIFIER NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_PermitChemicalHandling_TenantId_SiteId] ON [Compliance].[PermitChemicalHandling] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_PermitChemicalHandling_IssuedAtUtc] ON [Compliance].[PermitChemicalHandling] (IssuedAtUtc);
END
");

        // Table: PermitColdWork.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PermitColdWork' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[PermitColdWork] (
        PermitId        UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId        UNIQUEIDENTIFIER NOT NULL,
        SiteId          UNIQUEIDENTIFIER NULL,
        WorkDescription NVARCHAR(500)    NOT NULL,
        Location        NVARCHAR(200)    NOT NULL,
        Status          NVARCHAR(50)     NOT NULL,
        IssuedAtUtc     DATETIME2(3)     NOT NULL,
        ExpiresAtUtc    DATETIME2(3)     NOT NULL,
        ApprovedBy      UNIQUEIDENTIFIER NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_PermitColdWork_TenantId_SiteId] ON [Compliance].[PermitColdWork] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_PermitColdWork_IssuedAtUtc] ON [Compliance].[PermitColdWork] (IssuedAtUtc);
END
");

        // Table: PermitConfinedSpace.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PermitConfinedSpace' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[PermitConfinedSpace] (
        PermitId        UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId        UNIQUEIDENTIFIER NOT NULL,
        SiteId          UNIQUEIDENTIFIER NULL,
        SpaceIdentifier NVARCHAR(200)    NOT NULL,
        AttendantName   NVARCHAR(200)    NOT NULL,
        Status          NVARCHAR(50)     NOT NULL,
        IssuedAtUtc     DATETIME2(3)     NOT NULL,
        ExpiresAtUtc    DATETIME2(3)     NOT NULL,
        ApprovedBy      UNIQUEIDENTIFIER NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_PermitConfinedSpace_TenantId_SiteId] ON [Compliance].[PermitConfinedSpace] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_PermitConfinedSpace_IssuedAtUtc] ON [Compliance].[PermitConfinedSpace] (IssuedAtUtc);
END
");

        // Table: PermitElectrical.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PermitElectrical' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[PermitElectrical] (
        PermitId             UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId             UNIQUEIDENTIFIER NOT NULL,
        SiteId               UNIQUEIDENTIFIER NULL,
        EquipmentIdentifier  NVARCHAR(200)    NOT NULL,
        VoltageClass         NVARCHAR(50)     NOT NULL,
        Status               NVARCHAR(50)     NOT NULL,
        IssuedAtUtc          DATETIME2(3)     NOT NULL,
        ExpiresAtUtc         DATETIME2(3)     NOT NULL,
        ApprovedBy           UNIQUEIDENTIFIER NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_PermitElectrical_TenantId_SiteId] ON [Compliance].[PermitElectrical] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_PermitElectrical_IssuedAtUtc] ON [Compliance].[PermitElectrical] (IssuedAtUtc);
END
");

        // Table: PermitExcavation.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PermitExcavation' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[PermitExcavation] (
        PermitId          UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId          UNIQUEIDENTIFIER NOT NULL,
        SiteId            UNIQUEIDENTIFIER NULL,
        ExcavationLocation NVARCHAR(200)   NOT NULL,
        DepthMeters       DECIMAL(10,2)    NOT NULL,
        Status            NVARCHAR(50)     NOT NULL,
        IssuedAtUtc       DATETIME2(3)     NOT NULL,
        ExpiresAtUtc      DATETIME2(3)     NOT NULL,
        ApprovedBy        UNIQUEIDENTIFIER NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_PermitExcavation_TenantId_SiteId] ON [Compliance].[PermitExcavation] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_PermitExcavation_IssuedAtUtc] ON [Compliance].[PermitExcavation] (IssuedAtUtc);
END
");

        // Table: PermitHotTap.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PermitHotTap' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[PermitHotTap] (
        PermitId          UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId          UNIQUEIDENTIFIER NOT NULL,
        SiteId            UNIQUEIDENTIFIER NULL,
        LineIdentifier    NVARCHAR(200)    NOT NULL,
        PipelineContents  NVARCHAR(200)    NOT NULL,
        Status            NVARCHAR(50)     NOT NULL,
        IssuedAtUtc       DATETIME2(3)     NOT NULL,
        ExpiresAtUtc      DATETIME2(3)     NOT NULL,
        ApprovedBy        UNIQUEIDENTIFIER NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_PermitHotTap_TenantId_SiteId] ON [Compliance].[PermitHotTap] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_PermitHotTap_IssuedAtUtc] ON [Compliance].[PermitHotTap] (IssuedAtUtc);
END
");

        // Table: PermitHotWork.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PermitHotWork' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[PermitHotWork] (
        PermitId        UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId        UNIQUEIDENTIFIER NOT NULL,
        SiteId          UNIQUEIDENTIFIER NULL,
        WorkDescription NVARCHAR(500)    NOT NULL,
        FireWatchName   NVARCHAR(200)    NOT NULL,
        Status          NVARCHAR(50)     NOT NULL,
        IssuedAtUtc     DATETIME2(3)     NOT NULL,
        ExpiresAtUtc    DATETIME2(3)     NOT NULL,
        ApprovedBy      UNIQUEIDENTIFIER NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_PermitHotWork_TenantId_SiteId] ON [Compliance].[PermitHotWork] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_PermitHotWork_IssuedAtUtc] ON [Compliance].[PermitHotWork] (IssuedAtUtc);
END
");

        // Table: PermitLifting.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PermitLifting' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[PermitLifting] (
        PermitId        UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId        UNIQUEIDENTIFIER NOT NULL,
        SiteId          UNIQUEIDENTIFIER NULL,
        LoadDescription NVARCHAR(300)    NOT NULL,
        LoadWeightKg    DECIMAL(12,2)    NOT NULL,
        Status          NVARCHAR(50)     NOT NULL,
        IssuedAtUtc     DATETIME2(3)     NOT NULL,
        ExpiresAtUtc    DATETIME2(3)     NOT NULL,
        ApprovedBy      UNIQUEIDENTIFIER NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_PermitLifting_TenantId_SiteId] ON [Compliance].[PermitLifting] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_PermitLifting_IssuedAtUtc] ON [Compliance].[PermitLifting] (IssuedAtUtc);
END
");

        // Table: PermitLineBreak.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PermitLineBreak' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[PermitLineBreak] (
        PermitId         UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId         UNIQUEIDENTIFIER NOT NULL,
        SiteId           UNIQUEIDENTIFIER NULL,
        LineIdentifier   NVARCHAR(200)    NOT NULL,
        ProcessContents  NVARCHAR(200)    NOT NULL,
        Status           NVARCHAR(50)     NOT NULL,
        IssuedAtUtc      DATETIME2(3)     NOT NULL,
        ExpiresAtUtc     DATETIME2(3)     NOT NULL,
        ApprovedBy       UNIQUEIDENTIFIER NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_PermitLineBreak_TenantId_SiteId] ON [Compliance].[PermitLineBreak] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_PermitLineBreak_IssuedAtUtc] ON [Compliance].[PermitLineBreak] (IssuedAtUtc);
END
");

        // Table: PermitLockoutTagout.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PermitLockoutTagout' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[PermitLockoutTagout] (
        PermitId            UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId            UNIQUEIDENTIFIER NOT NULL,
        SiteId              UNIQUEIDENTIFIER NULL,
        EquipmentIdentifier NVARCHAR(200)    NOT NULL,
        LockHolderName      NVARCHAR(200)    NOT NULL,
        Status              NVARCHAR(50)     NOT NULL,
        IssuedAtUtc         DATETIME2(3)     NOT NULL,
        ExpiresAtUtc        DATETIME2(3)     NOT NULL,
        ApprovedBy          UNIQUEIDENTIFIER NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_PermitLockoutTagout_TenantId_SiteId] ON [Compliance].[PermitLockoutTagout] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_PermitLockoutTagout_IssuedAtUtc] ON [Compliance].[PermitLockoutTagout] (IssuedAtUtc);
END
");

        // Table: PermitPressureTesting.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PermitPressureTesting' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[PermitPressureTesting] (
        PermitId         UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId         UNIQUEIDENTIFIER NOT NULL,
        SiteId           UNIQUEIDENTIFIER NULL,
        TestArticle      NVARCHAR(200)    NOT NULL,
        TestPressurePsi  DECIMAL(12,2)    NOT NULL,
        Status           NVARCHAR(50)     NOT NULL,
        IssuedAtUtc      DATETIME2(3)     NOT NULL,
        ExpiresAtUtc     DATETIME2(3)     NOT NULL,
        ApprovedBy       UNIQUEIDENTIFIER NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_PermitPressureTesting_TenantId_SiteId] ON [Compliance].[PermitPressureTesting] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_PermitPressureTesting_IssuedAtUtc] ON [Compliance].[PermitPressureTesting] (IssuedAtUtc);
END
");

        // Table: PermitRadiography.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PermitRadiography' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[PermitRadiography] (
        PermitId             UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId             UNIQUEIDENTIFIER NOT NULL,
        SiteId               UNIQUEIDENTIFIER NULL,
        SourceIsotope        NVARCHAR(100)    NOT NULL,
        ExclusionZoneMeters  DECIMAL(10,2)    NOT NULL,
        Status               NVARCHAR(50)     NOT NULL,
        IssuedAtUtc          DATETIME2(3)     NOT NULL,
        ExpiresAtUtc         DATETIME2(3)     NOT NULL,
        ApprovedBy           UNIQUEIDENTIFIER NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_PermitRadiography_TenantId_SiteId] ON [Compliance].[PermitRadiography] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_PermitRadiography_IssuedAtUtc] ON [Compliance].[PermitRadiography] (IssuedAtUtc);
END
");

        // Table: PermitWorkatHeight.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PermitWorkatHeight' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[PermitWorkatHeight] (
        PermitId            UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId            UNIQUEIDENTIFIER NOT NULL,
        SiteId              UNIQUEIDENTIFIER NULL,
        WorkLocation        NVARCHAR(200)    NOT NULL,
        FallProtectionType  NVARCHAR(100)    NOT NULL,
        Status              NVARCHAR(50)     NOT NULL,
        IssuedAtUtc         DATETIME2(3)     NOT NULL,
        ExpiresAtUtc        DATETIME2(3)     NOT NULL,
        ApprovedBy          UNIQUEIDENTIFIER NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_PermitWorkatHeight_TenantId_SiteId] ON [Compliance].[PermitWorkatHeight] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_PermitWorkatHeight_IssuedAtUtc] ON [Compliance].[PermitWorkatHeight] (IssuedAtUtc);
END
");

        // Table: PolicyApprovals.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PolicyApprovals' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[PolicyApprovals] (
        ApprovalId     UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId       UNIQUEIDENTIFIER NOT NULL,
        SiteId         UNIQUEIDENTIFIER NULL,
        PolicyId       UNIQUEIDENTIFIER NOT NULL,
        PolicyTitle    NVARCHAR(300)    NOT NULL,
        Status         NVARCHAR(50)     NOT NULL,
        DecisionAtUtc  DATETIME2(3)     NOT NULL,
        ApproverId     UNIQUEIDENTIFIER NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_PolicyApprovals_TenantId_SiteId] ON [Compliance].[PolicyApprovals] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_PolicyApprovals_DecisionAtUtc] ON [Compliance].[PolicyApprovals] (DecisionAtUtc);
    CREATE NONCLUSTERED INDEX [IX_PolicyApprovals_PolicyId] ON [Compliance].[PolicyApprovals] (PolicyId);
END
");

        // Table: PolicyChemicalHandling.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PolicyChemicalHandling' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[PolicyChemicalHandling] (
        [PolicyId]                UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]                UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                  UNIQUEIDENTIFIER    NULL,
        [ChemicalName]            NVARCHAR(300)       NOT NULL,
        [HazardClass]             NVARCHAR(300)       NOT NULL,
        [Status]                  NVARCHAR(50)        NOT NULL,
        [EffectiveAtUtc]          DATETIME2(3)        NOT NULL,
        [OwnerId]                 UNIQUEIDENTIFIER    NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_PolicyChemicalHandling_TenantId_EffectiveAtUtc]
        ON [Compliance].[PolicyChemicalHandling] ([TenantId], [EffectiveAtUtc])
        INCLUDE ([SiteId]);
END
");

        // Table: PolicyContractor.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PolicyContractor' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[PolicyContractor] (
        [PolicyId]                UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]                UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                  UNIQUEIDENTIFIER    NULL,
        [ContractorRequirement]   NVARCHAR(300)       NOT NULL,
        [ApplicableTrade]         NVARCHAR(300)       NOT NULL,
        [Status]                  NVARCHAR(50)        NOT NULL,
        [EffectiveAtUtc]          DATETIME2(3)        NOT NULL,
        [OwnerId]                 UNIQUEIDENTIFIER    NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_PolicyContractor_TenantId_EffectiveAtUtc]
        ON [Compliance].[PolicyContractor] ([TenantId], [EffectiveAtUtc])
        INCLUDE ([SiteId]);
END
");

        // Table: PolicyDocuments.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PolicyDocuments' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[PolicyDocuments] (
        [DocumentId]              UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]                UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                  UNIQUEIDENTIFIER    NULL,
        [Title]                   NVARCHAR(300)       NOT NULL,
        [DocumentType]            NVARCHAR(300)       NOT NULL,
        [Status]                  NVARCHAR(50)        NOT NULL,
        [PublishedAtUtc]          DATETIME2(3)        NOT NULL,
        [OwnerId]                 UNIQUEIDENTIFIER    NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_PolicyDocuments_TenantId_PublishedAtUtc]
        ON [Compliance].[PolicyDocuments] ([TenantId], [PublishedAtUtc])
        INCLUDE ([SiteId]);
END
");

        // Table: PolicyEmergency.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PolicyEmergency' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[PolicyEmergency] (
        [PolicyId]                UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]                UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                  UNIQUEIDENTIFIER    NULL,
        [EmergencyType]           NVARCHAR(300)       NOT NULL,
        [ResponseProcedureRef]    NVARCHAR(300)       NOT NULL,
        [Status]                  NVARCHAR(50)        NOT NULL,
        [EffectiveAtUtc]          DATETIME2(3)        NOT NULL,
        [OwnerId]                 UNIQUEIDENTIFIER    NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_PolicyEmergency_TenantId_EffectiveAtUtc]
        ON [Compliance].[PolicyEmergency] ([TenantId], [EffectiveAtUtc])
        INCLUDE ([SiteId]);
END
");

        // Table: PolicyEnvironmental.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PolicyEnvironmental' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[PolicyEnvironmental] (
        [PolicyId]                UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]                UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                  UNIQUEIDENTIFIER    NULL,
        [EnvironmentalAspect]     NVARCHAR(300)       NOT NULL,
        [RegulatoryReference]     NVARCHAR(300)       NOT NULL,
        [Status]                  NVARCHAR(50)        NOT NULL,
        [EffectiveAtUtc]          DATETIME2(3)        NOT NULL,
        [OwnerId]                 UNIQUEIDENTIFIER    NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_PolicyEnvironmental_TenantId_EffectiveAtUtc]
        ON [Compliance].[PolicyEnvironmental] ([TenantId], [EffectiveAtUtc])
        INCLUDE ([SiteId]);
END
");

        // Table: PolicyEquipment.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PolicyEquipment' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[PolicyEquipment] (
        [PolicyId]                UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]                UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                  UNIQUEIDENTIFIER    NULL,
        [EquipmentCategory]       NVARCHAR(300)       NOT NULL,
        [MaintenanceRequirement]  NVARCHAR(300)       NOT NULL,
        [Status]                  NVARCHAR(50)        NOT NULL,
        [EffectiveAtUtc]          DATETIME2(3)        NOT NULL,
        [OwnerId]                 UNIQUEIDENTIFIER    NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_PolicyEquipment_TenantId_EffectiveAtUtc]
        ON [Compliance].[PolicyEquipment] ([TenantId], [EffectiveAtUtc])
        INCLUDE ([SiteId]);
END
");

        // Table: PolicyHR.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PolicyHR' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[PolicyHR] (
        [PolicyId]                UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]                UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                  UNIQUEIDENTIFIER    NULL,
        [PolicyArea]              NVARCHAR(300)       NOT NULL,
        [ApplicableRole]          NVARCHAR(300)       NOT NULL,
        [Status]                  NVARCHAR(50)        NOT NULL,
        [EffectiveAtUtc]          DATETIME2(3)        NOT NULL,
        [OwnerId]                 UNIQUEIDENTIFIER    NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_PolicyHR_TenantId_EffectiveAtUtc]
        ON [Compliance].[PolicyHR] ([TenantId], [EffectiveAtUtc])
        INCLUDE ([SiteId]);
END
");

        // Table: PolicyPPE.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PolicyPPE' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[PolicyPPE] (
        [PolicyId]                UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]                UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                  UNIQUEIDENTIFIER    NULL,
        [PPECategory]             NVARCHAR(300)       NOT NULL,
        [RequiredEquipment]       NVARCHAR(300)       NOT NULL,
        [Status]                  NVARCHAR(50)        NOT NULL,
        [EffectiveAtUtc]          DATETIME2(3)        NOT NULL,
        [OwnerId]                 UNIQUEIDENTIFIER    NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_PolicyPPE_TenantId_EffectiveAtUtc]
        ON [Compliance].[PolicyPPE] ([TenantId], [EffectiveAtUtc])
        INCLUDE ([SiteId]);
END
");

        // Table: PolicyRevisions.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PolicyRevisions' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[PolicyRevisions] (
        [RevisionId]              UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]                UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                  UNIQUEIDENTIFIER    NULL,
        [PolicyId]                UNIQUEIDENTIFIER    NOT NULL,
        [RevisionNumber]          NVARCHAR(50)        NOT NULL,
        [Status]                  NVARCHAR(50)        NOT NULL,
        [RevisedAtUtc]            DATETIME2(3)        NOT NULL,
        [RevisedBy]               UNIQUEIDENTIFIER    NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_PolicyRevisions_TenantId_RevisedAtUtc]
        ON [Compliance].[PolicyRevisions] ([TenantId], [RevisedAtUtc])
        INCLUDE ([SiteId]);
END
");

        // Table: PolicySafetyPolicies.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PolicySafetyPolicies' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[PolicySafetyPolicies] (
        [PolicyId]                UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]                UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                  UNIQUEIDENTIFIER    NULL,
        [PolicyName]              NVARCHAR(300)       NOT NULL,
        [Category]                NVARCHAR(300)       NOT NULL,
        [Status]                  NVARCHAR(50)        NOT NULL,
        [EffectiveAtUtc]          DATETIME2(3)        NOT NULL,
        [OwnerId]                 UNIQUEIDENTIFIER    NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_PolicySafetyPolicies_TenantId_EffectiveAtUtc]
        ON [Compliance].[PolicySafetyPolicies] ([TenantId], [EffectiveAtUtc])
        INCLUDE ([SiteId]);
END
");

        // Table: ProceduresEmergencyProcedure.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ProceduresEmergencyProcedure' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[ProceduresEmergencyProcedure] (
        [ProcedureId]             UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]                UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                  UNIQUEIDENTIFIER    NULL,
        [ProcedureName]           NVARCHAR(300)       NOT NULL,
        [EmergencyScenario]       NVARCHAR(300)       NOT NULL,
        [Status]                  NVARCHAR(50)        NOT NULL,
        [EffectiveAtUtc]          DATETIME2(3)        NOT NULL,
        [OwnerId]                 UNIQUEIDENTIFIER    NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_ProceduresEmergencyProcedure_TenantId_EffectiveAtUtc]
        ON [Compliance].[ProceduresEmergencyProcedure] ([TenantId], [EffectiveAtUtc])
        INCLUDE ([SiteId]);
END
");

        // Table: ProceduresHazMatProcedures.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ProceduresHazMatProcedures' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[ProceduresHazMatProcedures] (
        [ProcedureId]             UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]                UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                  UNIQUEIDENTIFIER    NULL,
        [HazardousMaterial]       NVARCHAR(300)       NOT NULL,
        [HandlingStep]            NVARCHAR(300)       NOT NULL,
        [Status]                  NVARCHAR(50)        NOT NULL,
        [EffectiveAtUtc]          DATETIME2(3)        NOT NULL,
        [OwnerId]                 UNIQUEIDENTIFIER    NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_ProceduresHazMatProcedures_TenantId_EffectiveAtUtc]
        ON [Compliance].[ProceduresHazMatProcedures] ([TenantId], [EffectiveAtUtc])
        INCLUDE ([SiteId]);
END
");

        // Table: ProceduresIncidentResponse.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ProceduresIncidentResponse' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[ProceduresIncidentResponse] (
        [ProcedureId]             UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]                UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                  UNIQUEIDENTIFIER    NULL,
        [IncidentType]            NVARCHAR(300)       NOT NULL,
        [ResponseStep]            NVARCHAR(300)       NOT NULL,
        [Status]                  NVARCHAR(50)        NOT NULL,
        [EffectiveAtUtc]          DATETIME2(3)        NOT NULL,
        [OwnerId]                 UNIQUEIDENTIFIER    NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_ProceduresIncidentResponse_TenantId_EffectiveAtUtc]
        ON [Compliance].[ProceduresIncidentResponse] ([TenantId], [EffectiveAtUtc])
        INCLUDE ([SiteId]);
END
");

        // Table: ProceduresInspection.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ProceduresInspection' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[ProceduresInspection] (
        [ProcedureId]             UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]                UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                  UNIQUEIDENTIFIER    NULL,
        [InspectionType]          NVARCHAR(300)       NOT NULL,
        [Frequency]               NVARCHAR(300)       NOT NULL,
        [Status]                  NVARCHAR(50)        NOT NULL,
        [EffectiveAtUtc]          DATETIME2(3)        NOT NULL,
        [OwnerId]                 UNIQUEIDENTIFIER    NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_ProceduresInspection_TenantId_EffectiveAtUtc]
        ON [Compliance].[ProceduresInspection] ([TenantId], [EffectiveAtUtc])
        INCLUDE ([SiteId]);
END
");

        // Table: ProceduresLockoutTagoutProcedure.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ProceduresLockoutTagoutProcedure' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[ProceduresLockoutTagoutProcedure] (
        [ProcedureId]             UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]                UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                  UNIQUEIDENTIFIER    NULL,
        [EquipmentType]           NVARCHAR(300)       NOT NULL,
        [LockoutStep]             NVARCHAR(300)       NOT NULL,
        [Status]                  NVARCHAR(50)        NOT NULL,
        [EffectiveAtUtc]          DATETIME2(3)        NOT NULL,
        [OwnerId]                 UNIQUEIDENTIFIER    NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_ProceduresLockoutTagoutProcedure_TenantId_EffectiveAtUtc]
        ON [Compliance].[ProceduresLockoutTagoutProcedure] ([TenantId], [EffectiveAtUtc])
        INCLUDE ([SiteId]);
END
");

        // Table: ProceduresMaintenance.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ProceduresMaintenance' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[ProceduresMaintenance] (
        [ProcedureId]             UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]                UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                  UNIQUEIDENTIFIER    NULL,
        [AssetCategory]           NVARCHAR(300)       NOT NULL,
        [MaintenanceTask]         NVARCHAR(300)       NOT NULL,
        [Status]                  NVARCHAR(50)        NOT NULL,
        [EffectiveAtUtc]          DATETIME2(3)        NOT NULL,
        [OwnerId]                 UNIQUEIDENTIFIER    NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_ProceduresMaintenance_TenantId_EffectiveAtUtc]
        ON [Compliance].[ProceduresMaintenance] ([TenantId], [EffectiveAtUtc])
        INCLUDE ([SiteId]);
END
");

        // Table: ProceduresSafety.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ProceduresSafety' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[ProceduresSafety] (
        [ProcedureId]             UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]                UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                  UNIQUEIDENTIFIER    NULL,
        [ProcedureName]           NVARCHAR(300)       NOT NULL,
        [HazardCategory]          NVARCHAR(300)       NOT NULL,
        [Status]                  NVARCHAR(50)        NOT NULL,
        [EffectiveAtUtc]          DATETIME2(3)        NOT NULL,
        [OwnerId]                 UNIQUEIDENTIFIER    NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_ProceduresSafety_TenantId_EffectiveAtUtc]
        ON [Compliance].[ProceduresSafety] ([TenantId], [EffectiveAtUtc])
        INCLUDE ([SiteId]);
END
");

        // Table: ProceduresSOPs.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ProceduresSOPs' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[ProceduresSOPs] (
        [SOPId]                   UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]                UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                  UNIQUEIDENTIFIER    NULL,
        [Title]                   NVARCHAR(300)       NOT NULL,
        [ProcessArea]             NVARCHAR(300)       NOT NULL,
        [Status]                  NVARCHAR(50)        NOT NULL,
        [EffectiveAtUtc]          DATETIME2(3)        NOT NULL,
        [OwnerId]                 UNIQUEIDENTIFIER    NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_ProceduresSOPs_TenantId_EffectiveAtUtc]
        ON [Compliance].[ProceduresSOPs] ([TenantId], [EffectiveAtUtc])
        INCLUDE ([SiteId]);
END
");

        // Table: ProceduresWorkInstructions.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ProceduresWorkInstructions' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[ProceduresWorkInstructions] (
        [InstructionId]           UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]                UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                  UNIQUEIDENTIFIER    NULL,
        [Title]                   NVARCHAR(300)       NOT NULL,
        [WorkArea]                NVARCHAR(300)       NOT NULL,
        [Status]                  NVARCHAR(50)        NOT NULL,
        [EffectiveAtUtc]          DATETIME2(3)        NOT NULL,
        [OwnerId]                 UNIQUEIDENTIFIER    NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_ProceduresWorkInstructions_TenantId_EffectiveAtUtc]
        ON [Compliance].[ProceduresWorkInstructions] ([TenantId], [EffectiveAtUtc])
        INCLUDE ([SiteId]);
END
");

        // Table: RegulatoryRequirements.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'RegulatoryRequirements' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[RegulatoryRequirements] (
        [RequirementId]           UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]                UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                  UNIQUEIDENTIFIER    NULL,
        [RequirementTitle]        NVARCHAR(300)       NOT NULL,
        [Jurisdiction]            NVARCHAR(300)       NOT NULL,
        [Status]                  NVARCHAR(50)        NOT NULL,
        [EffectiveAtUtc]          DATETIME2(3)        NOT NULL,
        [OwnerId]                 UNIQUEIDENTIFIER    NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_RegulatoryRequirements_TenantId_EffectiveAtUtc]
        ON [Compliance].[RegulatoryRequirements] ([TenantId], [EffectiveAtUtc])
        INCLUDE ([SiteId]);
END
");

        // Table: RenewalsDOT.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'RenewalsDOT' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[RenewalsDOT] (
        [RenewalId]               UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]                UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                  UNIQUEIDENTIFIER    NULL,
        [CredentialType]          NVARCHAR(300)       NOT NULL,
        [ExpiresAtUtc]            DATETIME2(3)        NOT NULL,
        [Status]                  NVARCHAR(50)        NOT NULL,
        [ResponsibleParty]        UNIQUEIDENTIFIER    NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_RenewalsDOT_TenantId_ExpiresAtUtc]
        ON [Compliance].[RenewalsDOT] ([TenantId], [ExpiresAtUtc])
        INCLUDE ([SiteId]);
END
");

        // Table: RenewalsISO.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'RenewalsISO' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[RenewalsISO] (
        [RenewalId]               UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]                UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                  UNIQUEIDENTIFIER    NULL,
        [CertificationStandard]   NVARCHAR(300)       NOT NULL,
        [ExpiresAtUtc]            DATETIME2(3)        NOT NULL,
        [Status]                  NVARCHAR(50)        NOT NULL,
        [ResponsibleParty]        UNIQUEIDENTIFIER    NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_RenewalsISO_TenantId_ExpiresAtUtc]
        ON [Compliance].[RenewalsISO] ([TenantId], [ExpiresAtUtc])
        INCLUDE ([SiteId]);
END
");

        // Table: RenewalsTraining.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'RenewalsTraining' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[RenewalsTraining] (
        [RenewalId]               UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]                UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                  UNIQUEIDENTIFIER    NULL,
        [TrainingProgram]         NVARCHAR(300)       NOT NULL,
        [ExpiresAtUtc]            DATETIME2(3)        NOT NULL,
        [Status]                  NVARCHAR(50)        NOT NULL,
        [ResponsibleParty]        UNIQUEIDENTIFIER    NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_RenewalsTraining_TenantId_ExpiresAtUtc]
        ON [Compliance].[RenewalsTraining] ([TenantId], [ExpiresAtUtc])
        INCLUDE ([SiteId]);
END
");

        // Table: Tracking.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Tracking' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[Tracking] (
        [TrackingItemId]          UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]                UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                  UNIQUEIDENTIFIER    NULL,
        [ItemDescription]         NVARCHAR(300)       NOT NULL,
        [Category]                NVARCHAR(300)       NOT NULL,
        [Status]                  NVARCHAR(50)        NOT NULL,
        [LastUpdatedAtUtc]        DATETIME2(3)        NOT NULL,
        [OwnerId]                 UNIQUEIDENTIFIER    NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_Tracking_TenantId_LastUpdatedAtUtc]
        ON [Compliance].[Tracking] ([TenantId], [LastUpdatedAtUtc])
        INCLUDE ([SiteId]);
END
");

        // Procedure: usp_AuditFindings_GetFindings.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Compliance].[usp_AuditFindings_GetFindings]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        FindingId,
        AuditingId,
        TenantId,
        SiteId,
        Title,
        Severity,
        Status,
        IdentifiedAtUtc,
        IdentifiedBy
    FROM [Compliance].[AuditFindings]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND IdentifiedAtUtc >= @FromUtc
      AND IdentifiedAtUtc <= @ToUtc
    ORDER BY IdentifiedAtUtc DESC;
END
");

        // Procedure: usp_CAPATriggers_GetTriggers.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Compliance].[usp_CAPATriggers_GetTriggers]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        TriggerId,
        AuditingId,
        SourceFindingId,
        TenantId,
        SiteId,
        Reason,
        Status,
        TriggeredAtUtc,
        TriggeredBy
    FROM [Compliance].[CAPATriggers]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND TriggeredAtUtc >= @FromUtc
      AND TriggeredAtUtc <= @ToUtc
    ORDER BY TriggeredAtUtc DESC;
END
");

        // Procedure: usp_CertificationContractor_GetContractor.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Compliance].[usp_CertificationContractor_GetContractor]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        CertificationId,
        TenantId,
        SiteId,
        ContractorName,
        CertificationType,
        Status,
        IssuedAtUtc,
        ExpiresAtUtc,
        VerifiedBy
    FROM [Compliance].[CertificationContractor]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND IssuedAtUtc >= @FromUtc
      AND IssuedAtUtc <= @ToUtc
    ORDER BY IssuedAtUtc DESC;
END
");

        // Procedure: usp_CertificationDOT_GetDOT.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Compliance].[usp_CertificationDOT_GetDOT]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        CertificationId,
        TenantId,
        SiteId,
        DriverName,
        LicenseClass,
        Status,
        IssuedAtUtc,
        ExpiresAtUtc,
        VerifiedBy
    FROM [Compliance].[CertificationDOT]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND IssuedAtUtc >= @FromUtc
      AND IssuedAtUtc <= @ToUtc
    ORDER BY IssuedAtUtc DESC;
END
");

        // Procedure: usp_CertificationEPA_GetEPA.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Compliance].[usp_CertificationEPA_GetEPA]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        CertificationId,
        TenantId,
        SiteId,
        PermitHolderName,
        EpaProgram,
        Status,
        IssuedAtUtc,
        ExpiresAtUtc,
        VerifiedBy
    FROM [Compliance].[CertificationEPA]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND IssuedAtUtc >= @FromUtc
      AND IssuedAtUtc <= @ToUtc
    ORDER BY IssuedAtUtc DESC;
END
");

        // Procedure: usp_CertificationIndustrySpecific_GetIndustrySpecific.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Compliance].[usp_CertificationIndustrySpecific_GetIndustrySpecific]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        CertificationId,
        TenantId,
        SiteId,
        HolderName,
        IndustryStandard,
        Status,
        IssuedAtUtc,
        ExpiresAtUtc,
        VerifiedBy
    FROM [Compliance].[CertificationIndustrySpecific]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND IssuedAtUtc >= @FromUtc
      AND IssuedAtUtc <= @ToUtc
    ORDER BY IssuedAtUtc DESC;
END
");

        // Procedure: usp_CertificationISO14001_GetISO14001.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Compliance].[usp_CertificationISO14001_GetISO14001]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        CertificationId,
        TenantId,
        SiteId,
        CertifyingBody,
        Scope,
        Status,
        IssuedAtUtc,
        ExpiresAtUtc,
        VerifiedBy
    FROM [Compliance].[CertificationISO14001]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND IssuedAtUtc >= @FromUtc
      AND IssuedAtUtc <= @ToUtc
    ORDER BY IssuedAtUtc DESC;
END
");

        // Procedure: usp_CertificationISO45001_GetISO45001.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Compliance].[usp_CertificationISO45001_GetISO45001]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        CertificationId,
        TenantId,
        SiteId,
        CertifyingBody,
        Scope,
        Status,
        IssuedAtUtc,
        ExpiresAtUtc,
        VerifiedBy
    FROM [Compliance].[CertificationISO45001]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND IssuedAtUtc >= @FromUtc
      AND IssuedAtUtc <= @ToUtc
    ORDER BY IssuedAtUtc DESC;
END
");

        // Procedure: usp_CertificationOSHACertifications_GetOSHACertifications.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Compliance].[usp_CertificationOSHACertifications_GetOSHACertifications]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        CertificationId,
        TenantId,
        SiteId,
        HolderName,
        OshaStandard,
        Status,
        IssuedAtUtc,
        ExpiresAtUtc,
        VerifiedBy
    FROM [Compliance].[CertificationOSHACertifications]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND IssuedAtUtc >= @FromUtc
      AND IssuedAtUtc <= @ToUtc
    ORDER BY IssuedAtUtc DESC;
END
");

        // Procedure: usp_CertificationTraining_GetTraining.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Compliance].[usp_CertificationTraining_GetTraining]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        TrainingRecordId,
        TenantId,
        SiteId,
        EmployeeName,
        CourseName,
        Status,
        CompletedAtUtc,
        ExpiresAtUtc,
        InstructorId
    FROM [Compliance].[CertificationTraining]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND CompletedAtUtc >= @FromUtc
      AND CompletedAtUtc <= @ToUtc
    ORDER BY CompletedAtUtc DESC;
END
");

        // Procedure: usp_ComplianceAudit_GetAudits.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Compliance].[usp_ComplianceAudit_GetAudits]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        AuditingId,
        TenantId,
        SiteId,
        AuditType,
        Status,
        ConductedAtUtc,
        ConductedBy
    FROM [Compliance].[ComplianceAudit]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND ConductedAtUtc >= @FromUtc
      AND ConductedAtUtc <= @ToUtc
    ORDER BY ConductedAtUtc DESC;
END
");

        // Procedure: usp_ComplianceExportItems_List.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Compliance].[usp_ComplianceExportItems_List]
    @TenantId       UNIQUEIDENTIFIER,
    @SiteId         UNIQUEIDENTIFIER = NULL,
    @FromUtc        DATETIME2(3) = NULL,
    @ToUtc          DATETIME2(3),
    @Status         NVARCHAR(50) = NULL,
    @ResourceType   NVARCHAR(100) = NULL,
    @MaxRecords     INT
AS
SET NOCOUNT ON;

SELECT TOP (@MaxRecords)
        [Id],
        [TenantId],
        [SiteId],
        [ResourceType],
        [ResourceId],
        [Title],
        [Status],
        [EffectiveAtUtc],
        [OwnerId]
FROM [Compliance].[ComplianceExportItems]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND (@FromUtc IS NULL OR [EffectiveAtUtc] >= @FromUtc)
  AND [EffectiveAtUtc] <= @ToUtc
  AND (@Status IS NULL OR [Status] = @Status)
  AND (@ResourceType IS NULL OR [ResourceType] = @ResourceType)
ORDER BY [EffectiveAtUtc] DESC;
");

        // Procedure: usp_ComplianceResources_BulkUpdate.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Compliance].[usp_ComplianceResources_BulkUpdate]
    @TenantId         UNIQUEIDENTIFIER,
    @SiteId           UNIQUEIDENTIFIER = NULL,
    @ResourceId       UNIQUEIDENTIFIER,
    @Operation        NVARCHAR(50),
    @AssignedToUserId UNIQUEIDENTIFIER = NULL,
    @Notes            NVARCHAR(MAX) = NULL,
    @NowUtc           DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;

        UPDATE [Compliance].[ComplianceResources]
        SET
            Status = CASE @Operation
                        WHEN 'Close'    THEN 'Closed'
                        WHEN 'Approve'  THEN 'Approved'
                        WHEN 'Reject'   THEN 'Rejected'
                        WHEN 'Reopen'   THEN 'Open'
                        ELSE Status
                     END,
            AssignedToUserId = CASE WHEN @Operation = 'Reassign' THEN @AssignedToUserId ELSE AssignedToUserId END,
            Notes = COALESCE(@Notes, Notes),
            UpdatedAtUtc = @NowUtc
        WHERE Id = @ResourceId
          AND TenantId = @TenantId
          AND (@SiteId IS NULL OR SiteId = @SiteId);

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
");

        // Procedure: usp_DeadlinesAnnualAudit_List.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Compliance].[usp_DeadlinesAnnualAudit_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [DeadlineId],
        [TenantId],
        [SiteId],
        [AuditScope],
        [DueAtUtc],
        [Status],
        [ResponsibleParty]
FROM [Compliance].[DeadlinesAnnualAudit]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [DueAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [DueAtUtc] DESC;
");

        // Procedure: usp_DeadlinesEPA_List.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Compliance].[usp_DeadlinesEPA_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [DeadlineId],
        [TenantId],
        [SiteId],
        [EPARegulation],
        [DueAtUtc],
        [Status],
        [ResponsibleParty]
FROM [Compliance].[DeadlinesEPA]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [DueAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [DueAtUtc] DESC;
");

        // Procedure: usp_InspectionsDOT_List.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Compliance].[usp_InspectionsDOT_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [InspectionId],
        [TenantId],
        [SiteId],
        [InspectionScope],
        [ScheduledAtUtc],
        [Status],
        [Inspector]
FROM [Compliance].[InspectionsDOT]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [ScheduledAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [ScheduledAtUtc] DESC;
");

        // Procedure: usp_InspectionsEnvironmentalSampling_List.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Compliance].[usp_InspectionsEnvironmentalSampling_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [InspectionId],
        [TenantId],
        [SiteId],
        [SampleType],
        [ScheduledAtUtc],
        [Status],
        [Inspector]
FROM [Compliance].[InspectionsEnvironmentalSampling]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [ScheduledAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [ScheduledAtUtc] DESC;
");

        // Procedure: usp_InspectionsEquipment_List.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Compliance].[usp_InspectionsEquipment_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [InspectionId],
        [TenantId],
        [SiteId],
        [EquipmentCategory],
        [ScheduledAtUtc],
        [Status],
        [Inspector]
FROM [Compliance].[InspectionsEquipment]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [ScheduledAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [ScheduledAtUtc] DESC;
");

        // Procedure: usp_InspectionsOSHA_List.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Compliance].[usp_InspectionsOSHA_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [InspectionId],
        [TenantId],
        [SiteId],
        [InspectionScope],
        [ScheduledAtUtc],
        [Status],
        [Inspector]
FROM [Compliance].[InspectionsOSHA]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [ScheduledAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [ScheduledAtUtc] DESC;
");

        // Procedure: usp_InternalSafetyAudit_GetAudits.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Compliance].[usp_InternalSafetyAudit_GetAudits]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        AuditingId,
        TenantId,
        SiteId,
        Scope,
        Status,
        ConductedAtUtc,
        ConductedBy
    FROM [Compliance].[InternalSafetyAudit]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND ConductedAtUtc >= @FromUtc
      AND ConductedAtUtc <= @ToUtc
    ORDER BY ConductedAtUtc DESC;
END
");

        // Procedure: usp_Moc_Approve.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Compliance].[usp_Moc_Approve]
    @TenantId       UNIQUEIDENTIFIER,
    @CorrelationId  UNIQUEIDENTIFIER,
    @ActorId        UNIQUEIDENTIFIER,
    @NowUtc         DATETIME2(3),
    @MocId          UNIQUEIDENTIFIER,
    @ApprovalNotes  NVARCHAR(2000) = NULL,
    @ETag           NVARCHAR(64)
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;

        UPDATE [Compliance].[Moc]
        SET
            Status = N'Approved',
            ApprovalNotes = @ApprovalNotes,
            ApprovedByActorId = @ActorId,
            ApprovedAtUtc = @NowUtc,
            LastModifiedAtUtc = @NowUtc
        WHERE MocId = @MocId
          AND TenantId = @TenantId
          AND IsDeleted = 0
          AND CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) = @ETag;

        DECLARE @RowsAffected INT = @@ROWCOUNT;

    COMMIT TRANSACTION;

    SELECT @RowsAffected;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
");

        // Procedure: usp_Moc_Create.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Compliance].[usp_Moc_Create]
    @TenantId         UNIQUEIDENTIFIER,
    @CorrelationId    UNIQUEIDENTIFIER,
    @ActorId          UNIQUEIDENTIFIER,
    @NowUtc            DATETIME2(3),
    @MocId             UNIQUEIDENTIFIER,
    @SiteId            UNIQUEIDENTIFIER,
    @Title             NVARCHAR(200),
    @Description       NVARCHAR(4000),
    @ChangeType        NVARCHAR(50),
    @RiskLevel         NVARCHAR(50),
    @InitiatorId       UNIQUEIDENTIFIER,
    @ProposedStartUtc  DATETIME2(3),
    @ProposedEndUtc    DATETIME2(3) = NULL
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;

        INSERT INTO [Compliance].[Moc]
        (
            MocId, TenantId, SiteId, Title, Description, ChangeType, RiskLevel, InitiatorId,
            ProposedStartUtc, ProposedEndUtc, Status, LastModifiedAtUtc, CreatedAtUtc, CreatedBy, IsDeleted
        )
        VALUES
        (
            @MocId, @TenantId, @SiteId, @Title, @Description, @ChangeType, @RiskLevel, @InitiatorId,
            @ProposedStartUtc, @ProposedEndUtc, N'Draft', @NowUtc, @NowUtc, @ActorId, 0
        );

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
");

        // Procedure: usp_Moc_GetById.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Compliance].[usp_Moc_GetById]
    @TenantId UNIQUEIDENTIFIER,
    @MocId    UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        MocId, TenantId, SiteId, Title, Description, ChangeType, RiskLevel, InitiatorId,
        ProposedStartUtc, ProposedEndUtc, Status, LastModifiedAtUtc,
        CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
    FROM [Compliance].[Moc]
    WHERE MocId = @MocId AND TenantId = @TenantId AND IsDeleted = 0;
END
");

        // Procedure: usp_MOCApprovals_GetApprovals.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Compliance].[usp_MOCApprovals_GetApprovals]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        ApprovalId,
        ChangeRequestId,
        TenantId,
        SiteId,
        ApproverName,
        Status,
        DecidedAtUtc,
        ApprovedBy
    FROM [Compliance].[MOCApprovals]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND DecidedAtUtc >= @FromUtc
      AND DecidedAtUtc <= @ToUtc
    ORDER BY DecidedAtUtc DESC;
END
");

        // Procedure: usp_MOCRiskAssessments_GetRiskAssessments.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Compliance].[usp_MOCRiskAssessments_GetRiskAssessments]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        RiskAssessmentId,
        ChangeRequestId,
        TenantId,
        SiteId,
        RiskLevel,
        Status,
        AssessedAtUtc,
        AssessedBy
    FROM [Compliance].[MOCRiskAssessments]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND AssessedAtUtc >= @FromUtc
      AND AssessedAtUtc <= @ToUtc
    ORDER BY AssessedAtUtc DESC;
END
");

        // Procedure: usp_MOCWorkflows_GetMOCWorkflows.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Compliance].[usp_MOCWorkflows_GetMOCWorkflows]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        WorkflowId,
        TenantId,
        SiteId,
        ChangeTitle,
        CurrentStage,
        Status,
        InitiatedAtUtc,
        InitiatedBy
    FROM [Compliance].[MOCWorkflows]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND InitiatedAtUtc >= @FromUtc
      AND InitiatedAtUtc <= @ToUtc
    ORDER BY InitiatedAtUtc DESC;
END
");

        // Procedure: usp_Obligations_GetObligations.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Compliance].[usp_Obligations_GetObligations]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        ObligationId,
        TenantId,
        SiteId,
        ObligationTitle,
        RegulatoryBody,
        Status,
        DueAtUtc,
        OwnerId
    FROM [Compliance].[Obligations]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND DueAtUtc >= @FromUtc
      AND DueAtUtc <= @ToUtc
    ORDER BY DueAtUtc DESC;
END
");

        // Procedure: usp_Permit_Approve.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Compliance].[usp_Permit_Approve]
    @TenantId       UNIQUEIDENTIFIER,
    @CorrelationId  UNIQUEIDENTIFIER,
    @ActorId        UNIQUEIDENTIFIER,
    @NowUtc         DATETIME2(3),
    @PermitId       UNIQUEIDENTIFIER,
    @ApprovalNotes  NVARCHAR(2000) = NULL,
    @ETag           NVARCHAR(64)
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;

        UPDATE [Compliance].[Permit]
        SET
            Status = N'Active',
            ApprovalNotes = @ApprovalNotes,
            ApprovedByActorId = @ActorId,
            ApprovedAtUtc = @NowUtc,
            LastModifiedAtUtc = @NowUtc
        WHERE PermitId = @PermitId
          AND TenantId = @TenantId
          AND IsDeleted = 0
          AND CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) = @ETag;

        DECLARE @RowsAffected INT = @@ROWCOUNT;

    COMMIT TRANSACTION;

    SELECT @RowsAffected;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
");

        // Procedure: usp_Permit_Create.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Compliance].[usp_Permit_Create]
    @TenantId          UNIQUEIDENTIFIER,
    @CorrelationId     UNIQUEIDENTIFIER,
    @ActorId           UNIQUEIDENTIFIER,
    @NowUtc            DATETIME2(3),
    @PermitId          UNIQUEIDENTIFIER,
    @SiteId            UNIQUEIDENTIFIER,
    @PermitType        NVARCHAR(100),
    @WorkDescription   NVARCHAR(2000),
    @WorkLocation      NVARCHAR(500),
    @WorkLeadId        UNIQUEIDENTIFIER,
    @ValidFromUtc      DATETIME2(3),
    @ValidToUtc        DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;

        DECLARE @PermitNumber NVARCHAR(50) = CONCAT(N'PTW-', UPPER(RIGHT(REPLACE(CONVERT(NVARCHAR(36), @PermitId), N'-', N''), 12)));

        INSERT INTO [Compliance].[Permit]
        (
            PermitId, TenantId, SiteId, PermitType, PermitNumber, WorkDescription, WorkLocation, WorkLeadId,
            ValidFromUtc, ValidToUtc, Status, LastModifiedAtUtc, CreatedAtUtc, CreatedBy, IsDeleted
        )
        VALUES
        (
            @PermitId, @TenantId, @SiteId, @PermitType, @PermitNumber, @WorkDescription, @WorkLocation, @WorkLeadId,
            @ValidFromUtc, @ValidToUtc, N'Draft', @NowUtc, @NowUtc, @ActorId, 0
        );

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
");

        // Procedure: usp_Permit_GetById.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Compliance].[usp_Permit_GetById]
    @TenantId UNIQUEIDENTIFIER,
    @PermitId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        PermitId, TenantId, SiteId, PermitType, PermitNumber, WorkDescription, WorkLocation, WorkLeadId,
        ValidFromUtc, ValidToUtc, Status, ApprovedByActorId, ApprovedAtUtc, LastModifiedAtUtc,
        CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
    FROM [Compliance].[Permit]
    WHERE PermitId = @PermitId AND TenantId = @TenantId AND IsDeleted = 0;
END
");

        // Procedure: usp_Permit_List.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Compliance].[usp_Permit_List]
    @TenantId      UNIQUEIDENTIFIER,
    @SiteId        UNIQUEIDENTIFIER = NULL,
    @PermitType    NVARCHAR(100) = NULL,
    @Status        NVARCHAR(50) = NULL,
    @ValidFromUtc  DATETIME2(3) = NULL,
    @ValidToUtc    DATETIME2(3) = NULL,
    @OrderBy       NVARCHAR(100) = N'ValidFromUtc',
    @SortDirection NVARCHAR(4) = N'DESC',
    @PageNumber    INT = 1,
    @PageSize      INT = 25
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        PermitId, TenantId, SiteId, PermitType, PermitNumber, WorkDescription, WorkLocation, WorkLeadId,
        ValidFromUtc, ValidToUtc, Status, ApprovedByActorId, ApprovedAtUtc, LastModifiedAtUtc,
        CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag,
        COUNT(1) OVER () AS TotalCount
    FROM [Compliance].[Permit]
    WHERE TenantId = @TenantId
      AND IsDeleted = 0
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND (@PermitType IS NULL OR PermitType = @PermitType)
      AND (@Status IS NULL OR Status = @Status)
      AND (@ValidFromUtc IS NULL OR ValidFromUtc >= @ValidFromUtc)
      AND (@ValidToUtc IS NULL OR ValidToUtc <= @ValidToUtc)
    ORDER BY
        CASE WHEN @OrderBy = N'ValidFromUtc' AND @SortDirection = N'ASC' THEN ValidFromUtc END ASC,
        CASE WHEN @OrderBy = N'ValidFromUtc' AND @SortDirection = N'DESC' THEN ValidFromUtc END DESC,
        ValidFromUtc DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
");

        // Procedure: usp_PermitChemicalHandling_GetChemicalHandling.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Compliance].[usp_PermitChemicalHandling_GetChemicalHandling]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        PermitId,
        TenantId,
        SiteId,
        ChemicalName,
        PermitHolder,
        Status,
        IssuedAtUtc,
        ExpiresAtUtc,
        ApprovedBy
    FROM [Compliance].[PermitChemicalHandling]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND IssuedAtUtc >= @FromUtc
      AND IssuedAtUtc <= @ToUtc
    ORDER BY IssuedAtUtc DESC;
END
");

        // Procedure: usp_PermitColdWork_GetColdWork.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Compliance].[usp_PermitColdWork_GetColdWork]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        PermitId,
        TenantId,
        SiteId,
        WorkDescription,
        Location,
        Status,
        IssuedAtUtc,
        ExpiresAtUtc,
        ApprovedBy
    FROM [Compliance].[PermitColdWork]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND IssuedAtUtc >= @FromUtc
      AND IssuedAtUtc <= @ToUtc
    ORDER BY IssuedAtUtc DESC;
END
");

        // Procedure: usp_PermitConfinedSpace_GetConfinedSpace.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Compliance].[usp_PermitConfinedSpace_GetConfinedSpace]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        PermitId,
        TenantId,
        SiteId,
        SpaceIdentifier,
        AttendantName,
        Status,
        IssuedAtUtc,
        ExpiresAtUtc,
        ApprovedBy
    FROM [Compliance].[PermitConfinedSpace]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND IssuedAtUtc >= @FromUtc
      AND IssuedAtUtc <= @ToUtc
    ORDER BY IssuedAtUtc DESC;
END
");

        // Procedure: usp_PermitElectrical_GetElectrical.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Compliance].[usp_PermitElectrical_GetElectrical]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        PermitId,
        TenantId,
        SiteId,
        EquipmentIdentifier,
        VoltageClass,
        Status,
        IssuedAtUtc,
        ExpiresAtUtc,
        ApprovedBy
    FROM [Compliance].[PermitElectrical]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND IssuedAtUtc >= @FromUtc
      AND IssuedAtUtc <= @ToUtc
    ORDER BY IssuedAtUtc DESC;
END
");

        // Procedure: usp_PermitExcavation_GetExcavation.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Compliance].[usp_PermitExcavation_GetExcavation]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        PermitId,
        TenantId,
        SiteId,
        ExcavationLocation,
        DepthMeters,
        Status,
        IssuedAtUtc,
        ExpiresAtUtc,
        ApprovedBy
    FROM [Compliance].[PermitExcavation]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND IssuedAtUtc >= @FromUtc
      AND IssuedAtUtc <= @ToUtc
    ORDER BY IssuedAtUtc DESC;
END
");

        // Procedure: usp_PermitHotTap_GetHotTap.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Compliance].[usp_PermitHotTap_GetHotTap]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        PermitId,
        TenantId,
        SiteId,
        LineIdentifier,
        PipelineContents,
        Status,
        IssuedAtUtc,
        ExpiresAtUtc,
        ApprovedBy
    FROM [Compliance].[PermitHotTap]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND IssuedAtUtc >= @FromUtc
      AND IssuedAtUtc <= @ToUtc
    ORDER BY IssuedAtUtc DESC;
END
");

        // Procedure: usp_PermitHotWork_GetHotWork.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Compliance].[usp_PermitHotWork_GetHotWork]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        PermitId,
        TenantId,
        SiteId,
        WorkDescription,
        FireWatchName,
        Status,
        IssuedAtUtc,
        ExpiresAtUtc,
        ApprovedBy
    FROM [Compliance].[PermitHotWork]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND IssuedAtUtc >= @FromUtc
      AND IssuedAtUtc <= @ToUtc
    ORDER BY IssuedAtUtc DESC;
END
");

        // Procedure: usp_PermitLifting_GetLifting.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Compliance].[usp_PermitLifting_GetLifting]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        PermitId,
        TenantId,
        SiteId,
        LoadDescription,
        LoadWeightKg,
        Status,
        IssuedAtUtc,
        ExpiresAtUtc,
        ApprovedBy
    FROM [Compliance].[PermitLifting]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND IssuedAtUtc >= @FromUtc
      AND IssuedAtUtc <= @ToUtc
    ORDER BY IssuedAtUtc DESC;
END
");

        // Procedure: usp_PermitLineBreak_GetLineBreak.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Compliance].[usp_PermitLineBreak_GetLineBreak]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        PermitId,
        TenantId,
        SiteId,
        LineIdentifier,
        ProcessContents,
        Status,
        IssuedAtUtc,
        ExpiresAtUtc,
        ApprovedBy
    FROM [Compliance].[PermitLineBreak]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND IssuedAtUtc >= @FromUtc
      AND IssuedAtUtc <= @ToUtc
    ORDER BY IssuedAtUtc DESC;
END
");

        // Procedure: usp_PermitLockoutTagout_GetLockoutTagout.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Compliance].[usp_PermitLockoutTagout_GetLockoutTagout]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        PermitId,
        TenantId,
        SiteId,
        EquipmentIdentifier,
        LockHolderName,
        Status,
        IssuedAtUtc,
        ExpiresAtUtc,
        ApprovedBy
    FROM [Compliance].[PermitLockoutTagout]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND IssuedAtUtc >= @FromUtc
      AND IssuedAtUtc <= @ToUtc
    ORDER BY IssuedAtUtc DESC;
END
");

        // Procedure: usp_PermitPressureTesting_GetPressureTesting.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Compliance].[usp_PermitPressureTesting_GetPressureTesting]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        PermitId,
        TenantId,
        SiteId,
        TestArticle,
        TestPressurePsi,
        Status,
        IssuedAtUtc,
        ExpiresAtUtc,
        ApprovedBy
    FROM [Compliance].[PermitPressureTesting]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND IssuedAtUtc >= @FromUtc
      AND IssuedAtUtc <= @ToUtc
    ORDER BY IssuedAtUtc DESC;
END
");

        // Procedure: usp_PermitRadiography_GetRadiography.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Compliance].[usp_PermitRadiography_GetRadiography]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        PermitId,
        TenantId,
        SiteId,
        SourceIsotope,
        ExclusionZoneMeters,
        Status,
        IssuedAtUtc,
        ExpiresAtUtc,
        ApprovedBy
    FROM [Compliance].[PermitRadiography]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND IssuedAtUtc >= @FromUtc
      AND IssuedAtUtc <= @ToUtc
    ORDER BY IssuedAtUtc DESC;
END
");

        // Procedure: usp_PermitWorkatHeight_GetWorkatHeight.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Compliance].[usp_PermitWorkatHeight_GetWorkatHeight]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        PermitId,
        TenantId,
        SiteId,
        WorkLocation,
        FallProtectionType,
        Status,
        IssuedAtUtc,
        ExpiresAtUtc,
        ApprovedBy
    FROM [Compliance].[PermitWorkatHeight]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND IssuedAtUtc >= @FromUtc
      AND IssuedAtUtc <= @ToUtc
    ORDER BY IssuedAtUtc DESC;
END
");

        // Procedure: usp_PolicyApprovals_GetApprovals.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Compliance].[usp_PolicyApprovals_GetApprovals]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        ApprovalId,
        TenantId,
        SiteId,
        PolicyId,
        PolicyTitle,
        Status,
        DecisionAtUtc,
        ApproverId
    FROM [Compliance].[PolicyApprovals]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND DecisionAtUtc >= @FromUtc
      AND DecisionAtUtc <= @ToUtc
    ORDER BY DecisionAtUtc DESC;
END
");

        // Procedure: usp_PolicyChemicalHandling_List.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Compliance].[usp_PolicyChemicalHandling_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [PolicyId],
        [TenantId],
        [SiteId],
        [ChemicalName],
        [HazardClass],
        [Status],
        [EffectiveAtUtc],
        [OwnerId]
FROM [Compliance].[PolicyChemicalHandling]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [EffectiveAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [EffectiveAtUtc] DESC;
");

        // Procedure: usp_PolicyContractor_List.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Compliance].[usp_PolicyContractor_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [PolicyId],
        [TenantId],
        [SiteId],
        [ContractorRequirement],
        [ApplicableTrade],
        [Status],
        [EffectiveAtUtc],
        [OwnerId]
FROM [Compliance].[PolicyContractor]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [EffectiveAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [EffectiveAtUtc] DESC;
");

        // Procedure: usp_PolicyDocuments_List.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Compliance].[usp_PolicyDocuments_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [DocumentId],
        [TenantId],
        [SiteId],
        [Title],
        [DocumentType],
        [Status],
        [PublishedAtUtc],
        [OwnerId]
FROM [Compliance].[PolicyDocuments]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [PublishedAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [PublishedAtUtc] DESC;
");

        // Procedure: usp_PolicyEmergency_List.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Compliance].[usp_PolicyEmergency_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [PolicyId],
        [TenantId],
        [SiteId],
        [EmergencyType],
        [ResponseProcedureRef],
        [Status],
        [EffectiveAtUtc],
        [OwnerId]
FROM [Compliance].[PolicyEmergency]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [EffectiveAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [EffectiveAtUtc] DESC;
");

        // Procedure: usp_PolicyEnvironmental_List.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Compliance].[usp_PolicyEnvironmental_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [PolicyId],
        [TenantId],
        [SiteId],
        [EnvironmentalAspect],
        [RegulatoryReference],
        [Status],
        [EffectiveAtUtc],
        [OwnerId]
FROM [Compliance].[PolicyEnvironmental]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [EffectiveAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [EffectiveAtUtc] DESC;
");

        // Procedure: usp_PolicyEquipment_List.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Compliance].[usp_PolicyEquipment_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [PolicyId],
        [TenantId],
        [SiteId],
        [EquipmentCategory],
        [MaintenanceRequirement],
        [Status],
        [EffectiveAtUtc],
        [OwnerId]
FROM [Compliance].[PolicyEquipment]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [EffectiveAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [EffectiveAtUtc] DESC;
");

        // Procedure: usp_PolicyHR_List.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Compliance].[usp_PolicyHR_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [PolicyId],
        [TenantId],
        [SiteId],
        [PolicyArea],
        [ApplicableRole],
        [Status],
        [EffectiveAtUtc],
        [OwnerId]
FROM [Compliance].[PolicyHR]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [EffectiveAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [EffectiveAtUtc] DESC;
");

        // Procedure: usp_PolicyPPE_List.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Compliance].[usp_PolicyPPE_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [PolicyId],
        [TenantId],
        [SiteId],
        [PPECategory],
        [RequiredEquipment],
        [Status],
        [EffectiveAtUtc],
        [OwnerId]
FROM [Compliance].[PolicyPPE]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [EffectiveAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [EffectiveAtUtc] DESC;
");

        // Procedure: usp_PolicyRevisions_List.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Compliance].[usp_PolicyRevisions_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [RevisionId],
        [TenantId],
        [SiteId],
        [PolicyId],
        [RevisionNumber],
        [Status],
        [RevisedAtUtc],
        [RevisedBy]
FROM [Compliance].[PolicyRevisions]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [RevisedAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [RevisedAtUtc] DESC;
");

        // Procedure: usp_PolicySafetyPolicies_List.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Compliance].[usp_PolicySafetyPolicies_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [PolicyId],
        [TenantId],
        [SiteId],
        [PolicyName],
        [Category],
        [Status],
        [EffectiveAtUtc],
        [OwnerId]
FROM [Compliance].[PolicySafetyPolicies]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [EffectiveAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [EffectiveAtUtc] DESC;
");

        // Procedure: usp_ProceduresEmergencyProcedure_List.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Compliance].[usp_ProceduresEmergencyProcedure_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [ProcedureId],
        [TenantId],
        [SiteId],
        [ProcedureName],
        [EmergencyScenario],
        [Status],
        [EffectiveAtUtc],
        [OwnerId]
FROM [Compliance].[ProceduresEmergencyProcedure]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [EffectiveAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [EffectiveAtUtc] DESC;
");

        // Procedure: usp_ProceduresHazMatProcedures_List.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Compliance].[usp_ProceduresHazMatProcedures_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [ProcedureId],
        [TenantId],
        [SiteId],
        [HazardousMaterial],
        [HandlingStep],
        [Status],
        [EffectiveAtUtc],
        [OwnerId]
FROM [Compliance].[ProceduresHazMatProcedures]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [EffectiveAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [EffectiveAtUtc] DESC;
");

        // Procedure: usp_ProceduresIncidentResponse_List.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Compliance].[usp_ProceduresIncidentResponse_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [ProcedureId],
        [TenantId],
        [SiteId],
        [IncidentType],
        [ResponseStep],
        [Status],
        [EffectiveAtUtc],
        [OwnerId]
FROM [Compliance].[ProceduresIncidentResponse]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [EffectiveAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [EffectiveAtUtc] DESC;
");

        // Procedure: usp_ProceduresInspection_List.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Compliance].[usp_ProceduresInspection_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [ProcedureId],
        [TenantId],
        [SiteId],
        [InspectionType],
        [Frequency],
        [Status],
        [EffectiveAtUtc],
        [OwnerId]
FROM [Compliance].[ProceduresInspection]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [EffectiveAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [EffectiveAtUtc] DESC;
");

        // Procedure: usp_ProceduresLockoutTagoutProcedure_List.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Compliance].[usp_ProceduresLockoutTagoutProcedure_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [ProcedureId],
        [TenantId],
        [SiteId],
        [EquipmentType],
        [LockoutStep],
        [Status],
        [EffectiveAtUtc],
        [OwnerId]
FROM [Compliance].[ProceduresLockoutTagoutProcedure]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [EffectiveAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [EffectiveAtUtc] DESC;
");

        // Procedure: usp_ProceduresMaintenance_List.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Compliance].[usp_ProceduresMaintenance_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [ProcedureId],
        [TenantId],
        [SiteId],
        [AssetCategory],
        [MaintenanceTask],
        [Status],
        [EffectiveAtUtc],
        [OwnerId]
FROM [Compliance].[ProceduresMaintenance]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [EffectiveAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [EffectiveAtUtc] DESC;
");

        // Procedure: usp_ProceduresSafety_List.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Compliance].[usp_ProceduresSafety_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [ProcedureId],
        [TenantId],
        [SiteId],
        [ProcedureName],
        [HazardCategory],
        [Status],
        [EffectiveAtUtc],
        [OwnerId]
FROM [Compliance].[ProceduresSafety]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [EffectiveAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [EffectiveAtUtc] DESC;
");

        // Procedure: usp_ProceduresSOPs_List.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Compliance].[usp_ProceduresSOPs_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [SOPId],
        [TenantId],
        [SiteId],
        [Title],
        [ProcessArea],
        [Status],
        [EffectiveAtUtc],
        [OwnerId]
FROM [Compliance].[ProceduresSOPs]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [EffectiveAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [EffectiveAtUtc] DESC;
");

        // Procedure: usp_ProceduresWorkInstructions_List.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Compliance].[usp_ProceduresWorkInstructions_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [InstructionId],
        [TenantId],
        [SiteId],
        [Title],
        [WorkArea],
        [Status],
        [EffectiveAtUtc],
        [OwnerId]
FROM [Compliance].[ProceduresWorkInstructions]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [EffectiveAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [EffectiveAtUtc] DESC;
");

        // Procedure: usp_RegulatoryRequirements_List.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Compliance].[usp_RegulatoryRequirements_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [RequirementId],
        [TenantId],
        [SiteId],
        [RequirementTitle],
        [Jurisdiction],
        [Status],
        [EffectiveAtUtc],
        [OwnerId]
FROM [Compliance].[RegulatoryRequirements]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [EffectiveAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [EffectiveAtUtc] DESC;
");

        // Procedure: usp_RenewalsDOT_List.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Compliance].[usp_RenewalsDOT_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [RenewalId],
        [TenantId],
        [SiteId],
        [CredentialType],
        [ExpiresAtUtc],
        [Status],
        [ResponsibleParty]
FROM [Compliance].[RenewalsDOT]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [ExpiresAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [ExpiresAtUtc] DESC;
");

        // Procedure: usp_RenewalsISO_List.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Compliance].[usp_RenewalsISO_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [RenewalId],
        [TenantId],
        [SiteId],
        [CertificationStandard],
        [ExpiresAtUtc],
        [Status],
        [ResponsibleParty]
FROM [Compliance].[RenewalsISO]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [ExpiresAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [ExpiresAtUtc] DESC;
");

        // Procedure: usp_RenewalsTraining_List.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Compliance].[usp_RenewalsTraining_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [RenewalId],
        [TenantId],
        [SiteId],
        [TrainingProgram],
        [ExpiresAtUtc],
        [Status],
        [ResponsibleParty]
FROM [Compliance].[RenewalsTraining]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [ExpiresAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [ExpiresAtUtc] DESC;
");

        // Procedure: usp_Tracking_List.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Compliance].[usp_Tracking_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [TrackingItemId],
        [TenantId],
        [SiteId],
        [ItemDescription],
        [Category],
        [Status],
        [LastUpdatedAtUtc],
        [OwnerId]
FROM [Compliance].[Tracking]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [LastUpdatedAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [LastUpdatedAtUtc] DESC;
");

    }


}

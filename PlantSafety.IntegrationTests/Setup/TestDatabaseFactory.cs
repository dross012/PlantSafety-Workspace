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
                    SchemaVersion   NVARCHAR(50)     NOT NULL CONSTRAINT DF_OutboxMessages_SchemaVersion DEFAULT ('2026.1'),
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
    /// UPDATED 2026-07-27 (Claude Code migration-commander-agent, IncidentManagement Phase 1
    /// modernization): also creates a minimal <c>dbo.ErrorLog</c> table (column set matches every
    /// module's canonical `INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog(...)` CATCH-block shape --
    /// ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate,
    /// ErrorContext, ErrorParameters, SourceDatabase, Module). Every write procedure's embedded copy
    /// in this file (e.g. usp_Incident_Create/Update/SoftDelete below) already wraps its ErrorLog
    /// INSERT in a nested TRY/CATCH that silently no-ops if the table is missing -- adding the real
    /// table here is purely additive (turns a documented no-op into a real, assertable row) and does
    /// not change any existing procedure's behavior or any existing test's pass/fail outcome; it only
    /// makes "ErrorLog row exists on failure" integration assertions possible, which the
    /// SQL-modernization Phase 1 checklist (docs\architecture\sql-modernization-phase1-execution-
    /// checklists.md, Section 4.4) flags as the one validation step no existing test suite covers.
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
            IF OBJECT_ID(N'dbo.ErrorLog', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.ErrorLog
                (
                    ErrorNumber     int             NOT NULL,
                    ErrorSeverity   int             NULL,
                    ErrorState      int             NULL,
                    ErrorProcedure  nvarchar(128)   NULL,
                    ErrorLine       int             NULL,
                    ErrorMessage    nvarchar(max)   NOT NULL,
                    ErrorDate       datetime2       NOT NULL,
                    ErrorContext    nvarchar(400)   NULL,
                    ErrorParameters nvarchar(max)   NULL,
                    SourceDatabase  nvarchar(128)   NOT NULL,
                    Module          nvarchar(128)   NOT NULL
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
    /// matching the single-database Testcontainers topology (both dbo.AuditLog and, as of the
    /// 2026-07-27 IncidentManagement Phase 1 modernization pass, dbo.ErrorLog are created by
    /// <see cref="ApplySharedAuditLoggingSchemaAsync"/>, so both writes are real, assertable rows
    /// rather than no-ops in this test topology).
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
        IdentifiedBy    UNIQUEIDENTIFIER NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
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
        TriggeredBy     UNIQUEIDENTIFIER NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
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
        VerifiedBy        UNIQUEIDENTIFIER NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
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
        VerifiedBy      UNIQUEIDENTIFIER NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
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
        VerifiedBy       UNIQUEIDENTIFIER NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
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
        VerifiedBy       UNIQUEIDENTIFIER NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
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
        VerifiedBy      UNIQUEIDENTIFIER NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
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
        VerifiedBy      UNIQUEIDENTIFIER NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
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
        VerifiedBy      UNIQUEIDENTIFIER NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
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
        InstructorId     UNIQUEIDENTIFIER NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
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
        ConductedBy     UNIQUEIDENTIFIER NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
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
        [OwnerId]           UNIQUEIDENTIFIER    NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
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
        UpdatedBy        UNIQUEIDENTIFIER NULL,
        RowVersion       ROWVERSION       NOT NULL
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
        [ResponsibleParty]        UNIQUEIDENTIFIER    NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
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
        [ResponsibleParty]        UNIQUEIDENTIFIER    NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
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
        [Inspector]               UNIQUEIDENTIFIER    NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
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
        [Inspector]               UNIQUEIDENTIFIER    NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
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
        [Inspector]               UNIQUEIDENTIFIER    NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
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
        [Inspector]               UNIQUEIDENTIFIER    NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
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
        ConductedBy     UNIQUEIDENTIFIER NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
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
        ApprovedBy      UNIQUEIDENTIFIER NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
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
        AssessedBy       UNIQUEIDENTIFIER NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
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
        InitiatedBy   UNIQUEIDENTIFIER NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
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
        OwnerId         UNIQUEIDENTIFIER NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
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
        ApprovedBy    UNIQUEIDENTIFIER NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
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
        ApprovedBy      UNIQUEIDENTIFIER NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
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
        ApprovedBy      UNIQUEIDENTIFIER NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
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
        ApprovedBy           UNIQUEIDENTIFIER NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
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
        ApprovedBy        UNIQUEIDENTIFIER NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
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
        ApprovedBy        UNIQUEIDENTIFIER NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
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
        ApprovedBy      UNIQUEIDENTIFIER NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
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
        ApprovedBy      UNIQUEIDENTIFIER NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
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
        ApprovedBy       UNIQUEIDENTIFIER NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
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
        ApprovedBy          UNIQUEIDENTIFIER NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
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
        ApprovedBy       UNIQUEIDENTIFIER NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
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
        ApprovedBy           UNIQUEIDENTIFIER NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
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
        ApprovedBy          UNIQUEIDENTIFIER NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
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
        ApproverId     UNIQUEIDENTIFIER NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
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
        [OwnerId]                 UNIQUEIDENTIFIER    NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
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
        [OwnerId]                 UNIQUEIDENTIFIER    NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
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
        [OwnerId]                 UNIQUEIDENTIFIER    NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
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
        [OwnerId]                 UNIQUEIDENTIFIER    NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
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
        [OwnerId]                 UNIQUEIDENTIFIER    NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
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
        [OwnerId]                 UNIQUEIDENTIFIER    NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
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
        [OwnerId]                 UNIQUEIDENTIFIER    NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
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
        [OwnerId]                 UNIQUEIDENTIFIER    NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
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
        [RevisedBy]               UNIQUEIDENTIFIER    NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
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
        [OwnerId]                 UNIQUEIDENTIFIER    NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
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
        [OwnerId]                 UNIQUEIDENTIFIER    NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
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
        [OwnerId]                 UNIQUEIDENTIFIER    NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
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
        [OwnerId]                 UNIQUEIDENTIFIER    NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
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
        [OwnerId]                 UNIQUEIDENTIFIER    NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
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
        [OwnerId]                 UNIQUEIDENTIFIER    NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
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
        [OwnerId]                 UNIQUEIDENTIFIER    NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
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
        [OwnerId]                 UNIQUEIDENTIFIER    NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
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
        [OwnerId]                 UNIQUEIDENTIFIER    NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
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
        [OwnerId]                 UNIQUEIDENTIFIER    NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
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
        [OwnerId]                 UNIQUEIDENTIFIER    NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
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
        [ResponsibleParty]        UNIQUEIDENTIFIER    NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
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
        [ResponsibleParty]        UNIQUEIDENTIFIER    NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
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
        [ResponsibleParty]        UNIQUEIDENTIFIER    NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
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
        [OwnerId]                 UNIQUEIDENTIFIER    NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
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

        DECLARE @RowsAffected INT = @@ROWCOUNT;

    COMMIT TRANSACTION;

    BEGIN TRY
        INSERT INTO dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
        VALUES (N'ComplianceResources', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'ResourceId=', @ResourceId, N'; TenantId=', @TenantId, N'; Operation=', @Operation), 0, OBJECT_NAME(@@PROCID));
    END TRY
    BEGIN CATCH
    END CATCH;

    SELECT @RowsAffected;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;

    DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ResourceId=', @ResourceId, N'; TenantId=', @TenantId, N'; Operation=', @Operation, N'; User=', SYSTEM_USER);
    DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

    BEGIN TRY
        INSERT INTO dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));
    END TRY
    BEGIN CATCH
    END CATCH;

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

    BEGIN TRY
        INSERT INTO dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
        VALUES (N'Moc', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'MocId=', @MocId, N'; TenantId=', @TenantId, N'; CorrelationId=', @CorrelationId, N'; ActorId=', @ActorId), 0, OBJECT_NAME(@@PROCID));
    END TRY
    BEGIN CATCH
    END CATCH;

    SELECT @RowsAffected;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;

    DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'MocId=', @MocId, N'; TenantId=', @TenantId, N'; CorrelationId=', @CorrelationId, N'; ActorId=', @ActorId, N'; User=', SYSTEM_USER);
    DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

    BEGIN TRY
        INSERT INTO dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));
    END TRY
    BEGIN CATCH
    END CATCH;

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

    BEGIN TRY
        INSERT INTO dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
        VALUES (N'Moc', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'MocId=', @MocId, N'; TenantId=', @TenantId, N'; CorrelationId=', @CorrelationId, N'; ActorId=', @ActorId), 0, OBJECT_NAME(@@PROCID));
    END TRY
    BEGIN CATCH
    END CATCH;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;

    DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'MocId=', @MocId, N'; TenantId=', @TenantId, N'; CorrelationId=', @CorrelationId, N'; ActorId=', @ActorId, N'; User=', SYSTEM_USER);
    DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

    BEGIN TRY
        INSERT INTO dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));
    END TRY
    BEGIN CATCH
    END CATCH;

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

    BEGIN TRY
        INSERT INTO dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
        VALUES (N'Permit', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'PermitId=', @PermitId, N'; TenantId=', @TenantId, N'; CorrelationId=', @CorrelationId, N'; ActorId=', @ActorId), 0, OBJECT_NAME(@@PROCID));
    END TRY
    BEGIN CATCH
    END CATCH;

    SELECT @RowsAffected;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;

    DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'PermitId=', @PermitId, N'; TenantId=', @TenantId, N'; CorrelationId=', @CorrelationId, N'; ActorId=', @ActorId, N'; User=', SYSTEM_USER);
    DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

    BEGIN TRY
        INSERT INTO dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));
    END TRY
    BEGIN CATCH
    END CATCH;

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

    BEGIN TRY
        INSERT INTO dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
        VALUES (N'Permit', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'PermitId=', @PermitId, N'; TenantId=', @TenantId, N'; CorrelationId=', @CorrelationId, N'; ActorId=', @ActorId), 0, OBJECT_NAME(@@PROCID));
    END TRY
    BEGIN CATCH
    END CATCH;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;

    DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'PermitId=', @PermitId, N'; TenantId=', @TenantId, N'; CorrelationId=', @CorrelationId, N'; ActorId=', @ActorId, N'; User=', SYSTEM_USER);
    DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

    BEGIN TRY
        INSERT INTO dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));
    END TRY
    BEGIN CATCH
    END CATCH;

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



    public static async Task ApplyAnalyticsSchemaAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken);

        await conn.ExecuteAsync(@"
            IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Analytics')
                EXEC('CREATE SCHEMA Analytics AUTHORIZATION dbo;');");

        // Table: AnalyticsExportItems.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AnalyticsExportItems' AND schema_id = SCHEMA_ID('Analytics'))
BEGIN
    CREATE TABLE [Analytics].[AnalyticsExportItems] (
        [Id]                UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]          UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]            UNIQUEIDENTIFIER    NULL,
        [ResourceType]      NVARCHAR(100)       NOT NULL,
        [ResourceId]        UNIQUEIDENTIFIER    NOT NULL,
        [Title]             NVARCHAR(300)       NOT NULL,
        [Status]            NVARCHAR(50)        NOT NULL,
        [EffectiveAtUtc]    DATETIME2(3)        NOT NULL,
        [OwnerId]           UNIQUEIDENTIFIER    NOT NULL,
        [RowVersion]        ROWVERSION          NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_AnalyticsExportItems_TenantId_EffectiveAtUtc]
        ON [Analytics].[AnalyticsExportItems] ([TenantId], [EffectiveAtUtc])
        INCLUDE ([SiteId], [Status], [ResourceType]);
END
");

        // Table: Behavior.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Behavior' AND schema_id = SCHEMA_ID('Analytics'))
BEGIN
    CREATE TABLE [Analytics].[Behavior] (
        [ObservationId]     UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]          UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]            UNIQUEIDENTIFIER    NULL,
        [EmployeeId]        UNIQUEIDENTIFIER    NOT NULL,
        [BehaviorCategory]  NVARCHAR(200)       NOT NULL,
        [ObservationType]   NVARCHAR(50)        NOT NULL,
        [Status]            NVARCHAR(50)        NOT NULL,
        [ObservedAtUtc]     DATETIME2(3)        NOT NULL,
        [ObservedBy]        UNIQUEIDENTIFIER    NOT NULL,
        [RowVersion]        ROWVERSION          NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_Behavior_TenantId_ObservedAtUtc]
        ON [Analytics].[Behavior] ([TenantId], [ObservedAtUtc])
        INCLUDE ([SiteId], [Status], [BehaviorCategory]);
END
");

        // Table: ComplianceFailure.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ComplianceFailure' AND schema_id = SCHEMA_ID('Analytics'))
BEGIN
    CREATE TABLE [Analytics].[ComplianceFailure] (
        [PredictionId]       UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]           UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]             UNIQUEIDENTIFIER    NULL,
        [ComplianceArea]     NVARCHAR(200)       NOT NULL,
        [FailureProbability] DECIMAL(18,4)       NOT NULL,
        [RiskLevel]          NVARCHAR(50)        NOT NULL,
        [Status]             NVARCHAR(50)        NOT NULL,
        [PredictedAtUtc]     DATETIME2(3)        NOT NULL,
        [RowVersion]         ROWVERSION          NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_ComplianceFailure_TenantId_PredictedAtUtc]
        ON [Analytics].[ComplianceFailure] ([TenantId], [PredictedAtUtc])
        INCLUDE ([SiteId], [Status], [RiskLevel]);
END
");

        // Table: Culture.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Culture' AND schema_id = SCHEMA_ID('Analytics'))
BEGIN
    CREATE TABLE [Analytics].[Culture] (
        CultureId          UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Culture PRIMARY KEY,
        TenantId            UNIQUEIDENTIFIER NOT NULL,
        SiteId              UNIQUEIDENTIFIER NULL,
        Status              NVARCHAR(30)     NOT NULL,
        SurveyPeriod        NVARCHAR(20)     NOT NULL,
        CultureIndexScore   DECIMAL(5,2)     NOT NULL,
        EngagementScore     DECIMAL(5,2)     NOT NULL,
        ParticipationRate   DECIMAL(5,2)     NOT NULL,
        TopRiskTheme        NVARCHAR(200)    NOT NULL,
        CreatedAtUtc        DATETIME2(3)     NOT NULL,
        CreatedBy           UNIQUEIDENTIFIER NOT NULL,
        RowVersion          ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_Culture_TenantId] ON [Analytics].[Culture] (TenantId);
END
");

        // Table: CultureIndex.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CultureIndex' AND schema_id = SCHEMA_ID('Analytics'))
BEGIN
    CREATE TABLE [Analytics].[CultureIndex] (
        [IndexSnapshotId]   UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]          UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]            UNIQUEIDENTIFIER    NULL,
        [CultureDimension]  NVARCHAR(200)       NOT NULL,
        [IndexScore]        DECIMAL(18,4)       NOT NULL,
        [TrendDirection]    NVARCHAR(50)        NOT NULL,
        [Status]            NVARCHAR(50)        NOT NULL,
        [CalculatedAtUtc]   DATETIME2(3)        NOT NULL,
        [RowVersion]        ROWVERSION          NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_CultureIndex_TenantId_CalculatedAtUtc]
        ON [Analytics].[CultureIndex] ([TenantId], [CalculatedAtUtc])
        INCLUDE ([SiteId], [Status], [CultureDimension]);
END
");

        // Table: DashboardTrendAnalysis.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'DashboardTrendAnalysis' AND schema_id = SCHEMA_ID('Analytics'))
BEGIN
    CREATE TABLE [Analytics].[DashboardTrendAnalysis] (
        [TrendPointId]      UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]          UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]            UNIQUEIDENTIFIER    NULL,
        [MetricName]        NVARCHAR(200)       NOT NULL,
        [MetricValue]       DECIMAL(18,4)       NOT NULL,
        [TrendDirection]    NVARCHAR(50)        NOT NULL,
        [Status]            NVARCHAR(50)        NOT NULL,
        [CalculatedAtUtc]   DATETIME2(3)        NOT NULL,
        [RowVersion]        ROWVERSION          NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_DashboardTrendAnalysis_TenantId_CalculatedAtUtc]
        ON [Analytics].[DashboardTrendAnalysis] ([TenantId], [CalculatedAtUtc])
        INCLUDE ([SiteId], [Status], [MetricName]);
END
");

        // Table: Department.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Department' AND schema_id = SCHEMA_ID('Analytics'))
BEGIN
    CREATE TABLE [Analytics].[Department] (
        [ScoreCardId]       UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]          UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]            UNIQUEIDENTIFIER    NULL,
        [DepartmentName]    NVARCHAR(200)       NOT NULL,
        [ScoreValue]        DECIMAL(18,4)       NOT NULL,
        [RankPercentile]    DECIMAL(18,4)       NULL,
        [Status]            NVARCHAR(50)        NOT NULL,
        [ScoredAtUtc]       DATETIME2(3)        NOT NULL,
        [RowVersion]        ROWVERSION          NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_Department_TenantId_ScoredAtUtc]
        ON [Analytics].[Department] ([TenantId], [ScoredAtUtc])
        INCLUDE ([SiteId], [Status], [DepartmentName]);
END
");

        // Table: EngagementDataPoints.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'EngagementDataPoints' AND schema_id = SCHEMA_ID('Analytics'))
BEGIN
    CREATE TABLE [Analytics].[EngagementDataPoints] (
        [DataPointId]       UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]          UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]            UNIQUEIDENTIFIER    NULL,
        [Label]              NVARCHAR(300)       NOT NULL,
        [Value]              DECIMAL(18,4)       NOT NULL,
        [Trend]              NVARCHAR(50)        NULL,
        [RecordedAtUtc]      DATETIME2(3)        NOT NULL,
        [RowVersion]         ROWVERSION          NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_EngagementDataPoints_TenantId_RecordedAtUtc]
        ON [Analytics].[EngagementDataPoints] ([TenantId], [RecordedAtUtc])
        INCLUDE ([SiteId], [Label]);
END
");

        // Table: EngagementScoring.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'EngagementScoring' AND schema_id = SCHEMA_ID('Analytics'))
BEGIN
    CREATE TABLE [Analytics].[EngagementScoring] (
        [ScoreId]           UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]          UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]            UNIQUEIDENTIFIER    NULL,
        [EmployeeId]        UNIQUEIDENTIFIER    NOT NULL,
        [EngagementScore]   DECIMAL(18,4)       NOT NULL,
        [ScoreCategory]     NVARCHAR(100)       NOT NULL,
        [Status]            NVARCHAR(50)        NOT NULL,
        [ScoredAtUtc]       DATETIME2(3)        NOT NULL,
        [RowVersion]        ROWVERSION          NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_EngagementScoring_TenantId_ScoredAtUtc]
        ON [Analytics].[EngagementScoring] ([TenantId], [ScoredAtUtc])
        INCLUDE ([SiteId], [Status], [ScoreCategory]);
END
");

        // Table: Executive.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Executive' AND schema_id = SCHEMA_ID('Analytics'))
BEGIN
    CREATE TABLE [Analytics].[Executive] (
        [SummaryId]         UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]          UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]            UNIQUEIDENTIFIER    NULL,
        [MetricName]        NVARCHAR(200)       NOT NULL,
        [MetricValue]       DECIMAL(18,4)       NOT NULL,
        [TargetValue]       DECIMAL(18,4)       NULL,
        [Status]            NVARCHAR(50)        NOT NULL,
        [SnapshotAtUtc]     DATETIME2(3)        NOT NULL,
        [RowVersion]        ROWVERSION          NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_Executive_TenantId_SnapshotAtUtc]
        ON [Analytics].[Executive] ([TenantId], [SnapshotAtUtc])
        INCLUDE ([SiteId], [Status], [MetricName]);
END
");

        // Table: FatigueRiskModeling.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'FatigueRiskModeling' AND schema_id = SCHEMA_ID('Analytics'))
BEGIN
    CREATE TABLE [Analytics].[FatigueRiskModeling] (
        [RiskAssessmentId]   UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]           UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]             UNIQUEIDENTIFIER    NULL,
        [EmployeeId]         UNIQUEIDENTIFIER    NOT NULL,
        [FatigueRiskScore]   DECIMAL(18,4)       NOT NULL,
        [RiskLevel]          NVARCHAR(50)        NOT NULL,
        [Status]             NVARCHAR(50)        NOT NULL,
        [AssessedAtUtc]      DATETIME2(3)        NOT NULL,
        [RowVersion]         ROWVERSION          NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_FatigueRiskModeling_TenantId_AssessedAtUtc]
        ON [Analytics].[FatigueRiskModeling] ([TenantId], [AssessedAtUtc])
        INCLUDE ([SiteId], [Status], [RiskLevel]);
END
");

        // Table: IncidentPrediction.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'IncidentPrediction' AND schema_id = SCHEMA_ID('Analytics'))
BEGIN
    CREATE TABLE [Analytics].[IncidentPrediction] (
        [PredictionId]         UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]             UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]               UNIQUEIDENTIFIER    NULL,
        [IncidentCategory]     NVARCHAR(200)       NOT NULL,
        [PredictedProbability] DECIMAL(18,4)       NOT NULL,
        [RiskLevel]            NVARCHAR(50)        NOT NULL,
        [Status]               NVARCHAR(50)        NOT NULL,
        [PredictedAtUtc]       DATETIME2(3)        NOT NULL,
        [RowVersion]           ROWVERSION          NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_IncidentPrediction_TenantId_PredictedAtUtc]
        ON [Analytics].[IncidentPrediction] ([TenantId], [PredictedAtUtc])
        INCLUDE ([SiteId], [Status], [RiskLevel]);
END
");

        // Table: KPISnapshots.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'KPISnapshots' AND schema_id = SCHEMA_ID('Analytics'))
BEGIN
    CREATE TABLE [Analytics].[KPISnapshots] (
        [SnapshotId]        UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]          UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]            UNIQUEIDENTIFIER    NULL,
        [KpiName]           NVARCHAR(200)       NOT NULL,
        [KpiValue]          DECIMAL(18,4)       NOT NULL,
        [UnitOfMeasure]     NVARCHAR(50)        NOT NULL,
        [Status]            NVARCHAR(50)        NOT NULL,
        [SnapshotAtUtc]     DATETIME2(3)        NOT NULL,
        [RowVersion]        ROWVERSION          NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_KPISnapshots_TenantId_SnapshotAtUtc]
        ON [Analytics].[KPISnapshots] ([TenantId], [SnapshotAtUtc])
        INCLUDE ([SiteId], [Status], [KpiName]);
END
");

        // Table: PredictiveAnalytics.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PredictiveAnalytics' AND schema_id = SCHEMA_ID('Analytics'))
BEGIN
    CREATE TABLE [Analytics].[PredictiveAnalytics] (
        [PredictionId]      UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]          UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]            UNIQUEIDENTIFIER    NULL,
        [ModelName]         NVARCHAR(200)       NOT NULL,
        [Score]             DECIMAL(18,4)       NOT NULL,
        [Status]            NVARCHAR(50)        NOT NULL,
        [GeneratedAtUtc]    DATETIME2(3)        NOT NULL,
        [GeneratedBy]       UNIQUEIDENTIFIER    NOT NULL,
        [RowVersion]        ROWVERSION          NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_PredictiveAnalytics_TenantId_GeneratedAtUtc]
        ON [Analytics].[PredictiveAnalytics] ([TenantId], [GeneratedAtUtc])
        INCLUDE ([SiteId], [Status], [ModelName]);
END
");

        // Table: ReadModel.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ReadModel' AND schema_id = SCHEMA_ID('Analytics'))
BEGIN
    CREATE TABLE [Analytics].[ReadModel] (
        [RowId]             UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]          UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]            UNIQUEIDENTIFIER    NULL,
        [WidgetName]        NVARCHAR(200)       NOT NULL,
        [WidgetValue]       NVARCHAR(400)       NOT NULL,
        [DisplayOrder]      INT                 NOT NULL,
        [Status]            NVARCHAR(50)        NOT NULL,
        [RefreshedAtUtc]    DATETIME2(3)        NOT NULL,
        [RowVersion]        ROWVERSION          NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_ReadModel_TenantId_RefreshedAtUtc]
        ON [Analytics].[ReadModel] ([TenantId], [RefreshedAtUtc])
        INCLUDE ([SiteId], [Status], [WidgetName], [DisplayOrder]);
END
");

        // Table: Reporting.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Reporting' AND schema_id = SCHEMA_ID('Analytics'))
BEGIN
    CREATE TABLE [Analytics].[Reporting] (
        [ReportItemId]      UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]          UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]            UNIQUEIDENTIFIER    NULL,
        [ReportName]        NVARCHAR(300)       NOT NULL,
        [ReportCategory]    NVARCHAR(100)       NOT NULL,
        [Status]            NVARCHAR(50)        NOT NULL,
        [GeneratedAtUtc]    DATETIME2(3)        NOT NULL,
        [GeneratedBy]       UNIQUEIDENTIFIER    NOT NULL,
        [RowVersion]        ROWVERSION          NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_Reporting_TenantId_GeneratedAtUtc]
        ON [Analytics].[Reporting] ([TenantId], [GeneratedAtUtc])
        INCLUDE ([SiteId], [Status], [ReportCategory]);
END
");

        // Table: RiskForecasting.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'RiskForecasting' AND schema_id = SCHEMA_ID('Analytics'))
BEGIN
    CREATE TABLE [Analytics].[RiskForecasting] (
        [ForecastId]           UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]             UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]               UNIQUEIDENTIFIER    NULL,
        [RiskCategory]         NVARCHAR(200)       NOT NULL,
        [ForecastValue]        DECIMAL(18,4)       NOT NULL,
        [ForecastHorizonDays]  INT                 NOT NULL,
        [Status]               NVARCHAR(50)        NOT NULL,
        [ForecastedAtUtc]      DATETIME2(3)        NOT NULL,
        [RowVersion]           ROWVERSION          NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_RiskForecasting_TenantId_ForecastedAtUtc]
        ON [Analytics].[RiskForecasting] ([TenantId], [ForecastedAtUtc])
        INCLUDE ([SiteId], [Status], [RiskCategory]);
END
");

        // Table: Schedules.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Schedules' AND schema_id = SCHEMA_ID('Analytics'))
BEGIN
    CREATE TABLE [Analytics].[Schedules] (
        [ScheduleItemId]    UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]          UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]            UNIQUEIDENTIFIER    NULL,
        [ScheduleName]      NVARCHAR(200)       NOT NULL,
        [Frequency]         NVARCHAR(50)        NOT NULL,
        [NextRunAtUtc]      DATETIME2(3)        NOT NULL,
        [Status]            NVARCHAR(50)        NOT NULL,
        [CreatedAtUtc]      DATETIME2(3)        NOT NULL,
        [CreatedBy]         UNIQUEIDENTIFIER    NOT NULL,
        [RowVersion]        ROWVERSION          NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_Schedules_TenantId_NextRunAtUtc]
        ON [Analytics].[Schedules] ([TenantId], [NextRunAtUtc])
        INCLUDE ([SiteId], [Status], [ScheduleName]);
END
");

        // Table: ScoreCardsCompliance.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ScoreCardsCompliance' AND schema_id = SCHEMA_ID('Analytics'))
BEGIN
    CREATE TABLE [Analytics].[ScoreCardsCompliance] (
        [ScoreCardId]       UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]          UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]            UNIQUEIDENTIFIER    NULL,
        [ComplianceArea]    NVARCHAR(200)       NOT NULL,
        [ScoreValue]        DECIMAL(18,4)       NOT NULL,
        [MaxScore]          DECIMAL(18,4)       NOT NULL,
        [Status]            NVARCHAR(50)        NOT NULL,
        [ScoredAtUtc]       DATETIME2(3)        NOT NULL,
        [RowVersion]        ROWVERSION          NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_ScoreCardsCompliance_TenantId_ScoredAtUtc]
        ON [Analytics].[ScoreCardsCompliance] ([TenantId], [ScoredAtUtc])
        INCLUDE ([SiteId], [Status], [ComplianceArea]);
END
");

        // Table: ScoreCardsTraining.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ScoreCardsTraining' AND schema_id = SCHEMA_ID('Analytics'))
BEGIN
    CREATE TABLE [Analytics].[ScoreCardsTraining] (
        [ScoreCardId]       UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]          UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]            UNIQUEIDENTIFIER    NULL,
        [EmployeeId]        UNIQUEIDENTIFIER    NOT NULL,
        [CompletionRate]    DECIMAL(18,4)       NOT NULL,
        [CoursesCompleted]  INT                 NOT NULL,
        [Status]            NVARCHAR(50)        NOT NULL,
        [ScoredAtUtc]       DATETIME2(3)        NOT NULL,
        [RowVersion]        ROWVERSION          NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_ScoreCardsTraining_TenantId_ScoredAtUtc]
        ON [Analytics].[ScoreCardsTraining] ([TenantId], [ScoredAtUtc])
        INCLUDE ([SiteId], [Status], [EmployeeId]);
END
");

        // Table: Supervisor.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Supervisor' AND schema_id = SCHEMA_ID('Analytics'))
BEGIN
    CREATE TABLE [Analytics].[Supervisor] (
        [ScoreCardId]       UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]          UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]            UNIQUEIDENTIFIER    NULL,
        [SupervisorId]      UNIQUEIDENTIFIER    NOT NULL,
        [ScoreValue]        DECIMAL(18,4)       NOT NULL,
        [TeamSize]          INT                 NOT NULL,
        [Status]            NVARCHAR(50)        NOT NULL,
        [ScoredAtUtc]       DATETIME2(3)        NOT NULL,
        [RowVersion]        ROWVERSION          NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_Supervisor_TenantId_ScoredAtUtc]
        ON [Analytics].[Supervisor] ([TenantId], [ScoredAtUtc])
        INCLUDE ([SiteId], [Status], [SupervisorId]);
END
");

        // Table: TrendAnalysis.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'TrendAnalysis' AND schema_id = SCHEMA_ID('Analytics'))
BEGIN
    CREATE TABLE [Analytics].[TrendAnalysis] (
        [TrendPointId]      UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]          UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]            UNIQUEIDENTIFIER    NULL,
        [MetricName]        NVARCHAR(200)       NOT NULL,
        [MetricValue]       DECIMAL(18,4)       NOT NULL,
        [TrendDirection]    NVARCHAR(50)        NOT NULL,
        [Status]            NVARCHAR(50)        NOT NULL,
        [CalculatedAtUtc]   DATETIME2(3)        NOT NULL,
        [RowVersion]        ROWVERSION          NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_TrendAnalysis_TenantId_CalculatedAtUtc]
        ON [Analytics].[TrendAnalysis] ([TenantId], [CalculatedAtUtc])
        INCLUDE ([SiteId], [Status], [MetricName]);
END
");

        // Table: TrendModeling.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'TrendModeling' AND schema_id = SCHEMA_ID('Analytics'))
BEGIN
    CREATE TABLE [Analytics].[TrendModeling] (
        [TrendModelId]      UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]          UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]            UNIQUEIDENTIFIER    NULL,
        [MetricName]        NVARCHAR(200)       NOT NULL,
        [PredictedValue]    DECIMAL(18,4)       NOT NULL,
        [ModelConfidence]   DECIMAL(18,4)       NOT NULL,
        [Status]            NVARCHAR(50)        NOT NULL,
        [GeneratedAtUtc]    DATETIME2(3)        NOT NULL,
        [RowVersion]        ROWVERSION          NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_TrendModeling_TenantId_GeneratedAtUtc]
        ON [Analytics].[TrendModeling] ([TenantId], [GeneratedAtUtc])
        INCLUDE ([SiteId], [Status], [MetricName]);
END
");

        // Procedure: usp_AnalyticsExportItems_List.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Analytics].[usp_AnalyticsExportItems_List]
    @TenantId       UNIQUEIDENTIFIER,
    @SiteId         UNIQUEIDENTIFIER = NULL,
    @FromUtc        DATETIME2(3) = NULL,
    @ToUtc          DATETIME2(3),
    @Status         NVARCHAR(50) = NULL,
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
FROM [Analytics].[AnalyticsExportItems]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND (@FromUtc IS NULL OR [EffectiveAtUtc] >= @FromUtc)
  AND [EffectiveAtUtc] <= @ToUtc
  AND (@Status IS NULL OR [Status] = @Status)
ORDER BY [EffectiveAtUtc] DESC;
");

        // Procedure: usp_Behavior_List.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Analytics].[usp_Behavior_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [ObservationId],
        [TenantId],
        [SiteId],
        [EmployeeId],
        [BehaviorCategory],
        [ObservationType],
        [Status],
        [ObservedAtUtc],
        [ObservedBy]
FROM [Analytics].[Behavior]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [ObservedAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [ObservedAtUtc] DESC;
");

        // Procedure: usp_ComplianceFailure_List.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Analytics].[usp_ComplianceFailure_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [PredictionId],
        [TenantId],
        [SiteId],
        [ComplianceArea],
        [FailureProbability],
        [RiskLevel],
        [Status],
        [PredictedAtUtc]
FROM [Analytics].[ComplianceFailure]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [PredictedAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [PredictedAtUtc] DESC;
");

        // Procedure: usp_Culture_GetAll.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Analytics].[usp_Culture_GetAll]
    @TenantId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        CultureId, TenantId, SiteId, Status, SurveyPeriod, CultureIndexScore, EngagementScore,
        ParticipationRate, TopRiskTheme, CreatedAtUtc, CreatedBy,
        CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
    FROM [Analytics].[Culture]
    WHERE TenantId = @TenantId
    ORDER BY CreatedAtUtc DESC;
END
");

        // Procedure: usp_Culture_GetById.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Analytics].[usp_Culture_GetById]
    @TenantId  UNIQUEIDENTIFIER,
    @CultureId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        CultureId, TenantId, SiteId, Status, SurveyPeriod, CultureIndexScore, EngagementScore,
        ParticipationRate, TopRiskTheme, CreatedAtUtc, CreatedBy,
        CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
    FROM [Analytics].[Culture]
    WHERE CultureId = @CultureId AND TenantId = @TenantId;
END
");

        // Procedure: usp_Culture_Insert.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Analytics].[usp_Culture_Insert]
    @CultureId          UNIQUEIDENTIFIER,
    @TenantId           UNIQUEIDENTIFIER,
    @SiteId             UNIQUEIDENTIFIER = NULL,
    @Status             NVARCHAR(30),
    @SurveyPeriod       NVARCHAR(20),
    @CultureIndexScore  DECIMAL(5,2),
    @EngagementScore    DECIMAL(5,2),
    @ParticipationRate  DECIMAL(5,2),
    @TopRiskTheme       NVARCHAR(200),
    @CreatedAtUtc       DATETIME2(3),
    @CreatedBy          UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        INSERT INTO [Analytics].[Culture]
        (
            CultureId, TenantId, SiteId, Status, SurveyPeriod, CultureIndexScore, EngagementScore,
            ParticipationRate, TopRiskTheme, CreatedAtUtc, CreatedBy
        )
        VALUES
        (
            @CultureId, @TenantId, @SiteId, @Status, @SurveyPeriod, @CultureIndexScore, @EngagementScore,
            @ParticipationRate, @TopRiskTheme, @CreatedAtUtc, @CreatedBy
        );

        BEGIN TRY
            INSERT INTO dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'Culture', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'CultureId=', @CultureId, N'; TenantId=', @TenantId, N'; CreatedBy=', @CreatedBy), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'CultureId=', @CultureId, N'; TenantId=', @TenantId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        BEGIN TRY
            INSERT INTO dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;

        THROW;
    END CATCH
END
");

        // Procedure: usp_CultureIndex_List.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Analytics].[usp_CultureIndex_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [IndexSnapshotId],
        [TenantId],
        [SiteId],
        [CultureDimension],
        [IndexScore],
        [TrendDirection],
        [Status],
        [CalculatedAtUtc]
FROM [Analytics].[CultureIndex]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [CalculatedAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [CalculatedAtUtc] DESC;
");

        // Procedure: usp_DashboardTrendAnalysis_List.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Analytics].[usp_DashboardTrendAnalysis_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [TrendPointId],
        [TenantId],
        [SiteId],
        [MetricName],
        [MetricValue],
        [TrendDirection],
        [Status],
        [CalculatedAtUtc]
FROM [Analytics].[DashboardTrendAnalysis]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [CalculatedAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [CalculatedAtUtc] DESC;
");

        // Procedure: usp_Department_List.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Analytics].[usp_Department_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [ScoreCardId],
        [TenantId],
        [SiteId],
        [DepartmentName],
        [ScoreValue],
        [RankPercentile],
        [Status],
        [ScoredAtUtc]
FROM [Analytics].[Department]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [ScoredAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [ScoredAtUtc] DESC;
");

        // Procedure: usp_EngagementDataPoints_List.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Analytics].[usp_EngagementDataPoints_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [Label],
        [Value],
        [Trend]
FROM [Analytics].[EngagementDataPoints]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [RecordedAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [RecordedAtUtc] DESC;
");

        // Procedure: usp_EngagementScoring_List.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Analytics].[usp_EngagementScoring_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [ScoreId],
        [TenantId],
        [SiteId],
        [EmployeeId],
        [EngagementScore],
        [ScoreCategory],
        [Status],
        [ScoredAtUtc]
FROM [Analytics].[EngagementScoring]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [ScoredAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [ScoredAtUtc] DESC;
");

        // Procedure: usp_Executive_List.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Analytics].[usp_Executive_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [SummaryId],
        [TenantId],
        [SiteId],
        [MetricName],
        [MetricValue],
        [TargetValue],
        [Status],
        [SnapshotAtUtc]
FROM [Analytics].[Executive]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [SnapshotAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [SnapshotAtUtc] DESC;
");

        // Procedure: usp_FatigueRiskModeling_List.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Analytics].[usp_FatigueRiskModeling_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [RiskAssessmentId],
        [TenantId],
        [SiteId],
        [EmployeeId],
        [FatigueRiskScore],
        [RiskLevel],
        [Status],
        [AssessedAtUtc]
FROM [Analytics].[FatigueRiskModeling]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [AssessedAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [AssessedAtUtc] DESC;
");

        // Procedure: usp_IncidentPrediction_List.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Analytics].[usp_IncidentPrediction_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [PredictionId],
        [TenantId],
        [SiteId],
        [IncidentCategory],
        [PredictedProbability],
        [RiskLevel],
        [Status],
        [PredictedAtUtc]
FROM [Analytics].[IncidentPrediction]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [PredictedAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [PredictedAtUtc] DESC;
");

        // Procedure: usp_KPISnapshots_List.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Analytics].[usp_KPISnapshots_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [SnapshotId],
        [TenantId],
        [SiteId],
        [KpiName],
        [KpiValue],
        [UnitOfMeasure],
        [Status],
        [SnapshotAtUtc]
FROM [Analytics].[KPISnapshots]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [SnapshotAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [SnapshotAtUtc] DESC;
");

        // Procedure: usp_PredictiveAnalytics_List.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Analytics].[usp_PredictiveAnalytics_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [PredictionId],
        [TenantId],
        [SiteId],
        [ModelName],
        [Score],
        [Status],
        [GeneratedAtUtc],
        [GeneratedBy]
FROM [Analytics].[PredictiveAnalytics]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [GeneratedAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [GeneratedAtUtc] DESC;
");

        // Procedure: usp_ReadModel_List.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Analytics].[usp_ReadModel_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [RowId],
        [TenantId],
        [SiteId],
        [WidgetName],
        [WidgetValue],
        [DisplayOrder],
        [Status],
        [RefreshedAtUtc]
FROM [Analytics].[ReadModel]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [RefreshedAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [DisplayOrder] ASC, [RefreshedAtUtc] DESC;
");

        // Procedure: usp_Reporting_List.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Analytics].[usp_Reporting_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [ReportItemId],
        [TenantId],
        [SiteId],
        [ReportName],
        [ReportCategory],
        [Status],
        [GeneratedAtUtc],
        [GeneratedBy]
FROM [Analytics].[Reporting]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [GeneratedAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [GeneratedAtUtc] DESC;
");

        // Procedure: usp_RiskForecasting_List.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Analytics].[usp_RiskForecasting_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [ForecastId],
        [TenantId],
        [SiteId],
        [RiskCategory],
        [ForecastValue],
        [ForecastHorizonDays],
        [Status],
        [ForecastedAtUtc]
FROM [Analytics].[RiskForecasting]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [ForecastedAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [ForecastedAtUtc] DESC;
");

        // Procedure: usp_Schedules_List.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Analytics].[usp_Schedules_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [ScheduleItemId],
        [TenantId],
        [SiteId],
        [ScheduleName],
        [Frequency],
        [NextRunAtUtc],
        [Status],
        [CreatedAtUtc],
        [CreatedBy]
FROM [Analytics].[Schedules]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [NextRunAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [NextRunAtUtc] ASC;
");

        // Procedure: usp_ScoreCardsCompliance_List.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Analytics].[usp_ScoreCardsCompliance_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [ScoreCardId],
        [TenantId],
        [SiteId],
        [ComplianceArea],
        [ScoreValue],
        [MaxScore],
        [Status],
        [ScoredAtUtc]
FROM [Analytics].[ScoreCardsCompliance]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [ScoredAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [ScoredAtUtc] DESC;
");

        // Procedure: usp_ScoreCardsTraining_List.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Analytics].[usp_ScoreCardsTraining_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [ScoreCardId],
        [TenantId],
        [SiteId],
        [EmployeeId],
        [CompletionRate],
        [CoursesCompleted],
        [Status],
        [ScoredAtUtc]
FROM [Analytics].[ScoreCardsTraining]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [ScoredAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [ScoredAtUtc] DESC;
");

        // Procedure: usp_Supervisor_List.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Analytics].[usp_Supervisor_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [ScoreCardId],
        [TenantId],
        [SiteId],
        [SupervisorId],
        [ScoreValue],
        [TeamSize],
        [Status],
        [ScoredAtUtc]
FROM [Analytics].[Supervisor]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [ScoredAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [ScoredAtUtc] DESC;
");

        // Procedure: usp_TrendAnalysis_List.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Analytics].[usp_TrendAnalysis_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [TrendPointId],
        [TenantId],
        [SiteId],
        [MetricName],
        [MetricValue],
        [TrendDirection],
        [Status],
        [CalculatedAtUtc]
FROM [Analytics].[TrendAnalysis]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [CalculatedAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [CalculatedAtUtc] DESC;
");

        // Procedure: usp_TrendModeling_List.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Analytics].[usp_TrendModeling_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [TrendModelId],
        [TenantId],
        [SiteId],
        [MetricName],
        [PredictedValue],
        [ModelConfidence],
        [Status],
        [GeneratedAtUtc]
FROM [Analytics].[TrendModeling]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [GeneratedAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [GeneratedAtUtc] DESC;
");

    }



    public static async Task ApplyOperationalSchemaAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken);

        await conn.ExecuteAsync(@"
            IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Operational')
                EXEC('CREATE SCHEMA Operational AUTHORIZATION dbo;');");

        // Table: AlertsReadModel.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AlertsReadModel' AND schema_id = SCHEMA_ID('Operational'))
BEGIN
    CREATE TABLE [Operational].[AlertsReadModel] (
        [AlertId]         UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]              UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                UNIQUEIDENTIFIER    NULL,
        [Message]                 NVARCHAR(400)       NOT NULL,
        [Severity]                NVARCHAR(400)       NOT NULL,
        [Status]                NVARCHAR(50)        NOT NULL,
        [TriggeredAtUtc]  DATETIME2(3)        NOT NULL,
        [AcknowledgedBy]  UNIQUEIDENTIFIER    NULL,
        RowVersion       ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_AlertsReadModel_TenantId_TriggeredAtUtc]
        ON [Operational].[AlertsReadModel] ([TenantId], [TriggeredAtUtc])
        INCLUDE ([SiteId]);
END
");

        // Table: ChecklistsReadModel.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ChecklistsReadModel' AND schema_id = SCHEMA_ID('Operational'))
BEGIN
    CREATE TABLE [Operational].[ChecklistsReadModel] (
        [ChecklistId]     UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]              UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                UNIQUEIDENTIFIER    NULL,
        [Name]                    NVARCHAR(400)       NOT NULL,
        [Category]                NVARCHAR(400)       NOT NULL,
        [Status]                NVARCHAR(50)        NOT NULL,
        [CompletedAtUtc]  DATETIME2(3)        NOT NULL,
        [CompletedBy]     UNIQUEIDENTIFIER    NULL,
        RowVersion       ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_ChecklistsReadModel_TenantId_CompletedAtUtc]
        ON [Operational].[ChecklistsReadModel] ([TenantId], [CompletedAtUtc])
        INCLUDE ([SiteId]);
END
");

        // Table: ClassesReadModel.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ClassesReadModel' AND schema_id = SCHEMA_ID('Operational'))
BEGIN
    CREATE TABLE [Operational].[ClassesReadModel] (
        [ClassId]         UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]              UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                UNIQUEIDENTIFIER    NULL,
        [Title]                   NVARCHAR(400)       NOT NULL,
        [InstructorId]            UNIQUEIDENTIFIER    NOT NULL,
        [Status]                NVARCHAR(50)        NOT NULL,
        [ScheduledAtUtc]  DATETIME2(3)        NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_ClassesReadModel_TenantId_ScheduledAtUtc]
        ON [Operational].[ClassesReadModel] ([TenantId], [ScheduledAtUtc])
        INCLUDE ([SiteId]);
END
");

        // Table: CompetencyManagementReadModel.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CompetencyManagementReadModel' AND schema_id = SCHEMA_ID('Operational'))
BEGIN
    CREATE TABLE [Operational].[CompetencyManagementReadModel] (
        [CompetencyRecordId] UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]              UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                UNIQUEIDENTIFIER    NULL,
        [EmployeeId]              UNIQUEIDENTIFIER    NOT NULL,
        [CompetencyArea]          NVARCHAR(400)       NOT NULL,
        [Status]                NVARCHAR(50)        NOT NULL,
        [AssessedAtUtc]   DATETIME2(3)        NOT NULL,
        [AssessedBy]      UNIQUEIDENTIFIER    NULL,
        RowVersion       ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_CompetencyManagementReadModel_TenantId_AssessedAtUtc]
        ON [Operational].[CompetencyManagementReadModel] ([TenantId], [AssessedAtUtc])
        INCLUDE ([SiteId]);
END
");

        // Table: ContractorReadModel.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ContractorReadModel' AND schema_id = SCHEMA_ID('Operational'))
BEGIN
    CREATE TABLE [Operational].[ContractorReadModel] (
        [ContractorRecordId] UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]              UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                UNIQUEIDENTIFIER    NULL,
        [ContractorName]          NVARCHAR(400)       NOT NULL,
        [ServiceType]             NVARCHAR(400)       NOT NULL,
        [Status]                NVARCHAR(50)        NOT NULL,
        [OnboardedAtUtc]  DATETIME2(3)        NOT NULL,
        [ApprovedBy]      UNIQUEIDENTIFIER    NULL,
        RowVersion       ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_ContractorReadModel_TenantId_OnboardedAtUtc]
        ON [Operational].[ContractorReadModel] ([TenantId], [OnboardedAtUtc])
        INCLUDE ([SiteId]);
END
");

        // Table: CoursesReadModel.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CoursesReadModel' AND schema_id = SCHEMA_ID('Operational'))
BEGIN
    CREATE TABLE [Operational].[CoursesReadModel] (
        [CourseId]        UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]              UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                UNIQUEIDENTIFIER    NULL,
        [Title]                   NVARCHAR(400)       NOT NULL,
        [Category]                NVARCHAR(400)       NOT NULL,
        [Status]                NVARCHAR(50)        NOT NULL,
        [PublishedAtUtc]  DATETIME2(3)        NOT NULL,
        [PublishedBy]     UNIQUEIDENTIFIER    NULL,
        RowVersion       ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_CoursesReadModel_TenantId_PublishedAtUtc]
        ON [Operational].[CoursesReadModel] ([TenantId], [PublishedAtUtc])
        INCLUDE ([SiteId]);
END
");

        // Table: EquipmentSafetyReadModel.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'EquipmentSafetyReadModel' AND schema_id = SCHEMA_ID('Operational'))
BEGIN
    CREATE TABLE [Operational].[EquipmentSafetyReadModel] (
        [EquipmentSafetyRecordId] UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]              UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                UNIQUEIDENTIFIER    NULL,
        [EquipmentId]             UNIQUEIDENTIFIER    NOT NULL,
        [EquipmentName]           NVARCHAR(400)       NOT NULL,
        [Status]                NVARCHAR(50)        NOT NULL,
        [InspectedAtUtc]  DATETIME2(3)        NOT NULL,
        [InspectedBy]     UNIQUEIDENTIFIER    NULL,
        RowVersion       ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_EquipmentSafetyReadModel_TenantId_InspectedAtUtc]
        ON [Operational].[EquipmentSafetyReadModel] ([TenantId], [InspectedAtUtc])
        INCLUDE ([SiteId]);
END
");

        // Table: ErgonomicsReadModel.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ErgonomicsReadModel' AND schema_id = SCHEMA_ID('Operational'))
BEGIN
    CREATE TABLE [Operational].[ErgonomicsReadModel] (
        [ErgonomicsAssessmentId] UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]              UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                UNIQUEIDENTIFIER    NULL,
        [WorkstationId]           UNIQUEIDENTIFIER    NOT NULL,
        [RiskLevel]               NVARCHAR(400)       NOT NULL,
        [Status]                NVARCHAR(50)        NOT NULL,
        [AssessedAtUtc]   DATETIME2(3)        NOT NULL,
        [AssessedBy]      UNIQUEIDENTIFIER    NULL,
        RowVersion       ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_ErgonomicsReadModel_TenantId_AssessedAtUtc]
        ON [Operational].[ErgonomicsReadModel] ([TenantId], [AssessedAtUtc])
        INCLUDE ([SiteId]);
END
");

        // Table: FatigueManagementReadModel.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'FatigueManagementReadModel' AND schema_id = SCHEMA_ID('Operational'))
BEGIN
    CREATE TABLE [Operational].[FatigueManagementReadModel] (
        [FatigueRecordId] UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]              UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                UNIQUEIDENTIFIER    NULL,
        [EmployeeId]              UNIQUEIDENTIFIER    NOT NULL,
        [FatigueRiskLevel]        NVARCHAR(400)       NOT NULL,
        [Status]                NVARCHAR(50)        NOT NULL,
        [RecordedAtUtc]   DATETIME2(3)        NOT NULL,
        [RecordedBy]      UNIQUEIDENTIFIER    NULL,
        RowVersion       ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_FatigueManagementReadModel_TenantId_RecordedAtUtc]
        ON [Operational].[FatigueManagementReadModel] ([TenantId], [RecordedAtUtc])
        INCLUDE ([SiteId]);
END
");

        // Table: HazardAssessmentsReadModel.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'HazardAssessmentsReadModel' AND schema_id = SCHEMA_ID('Operational'))
BEGIN
    CREATE TABLE [Operational].[HazardAssessmentsReadModel] (
        [HazardAssessmentId] UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]              UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                UNIQUEIDENTIFIER    NULL,
        [HazardDescription]       NVARCHAR(400)       NOT NULL,
        [RiskLevel]               NVARCHAR(400)       NOT NULL,
        [Status]                NVARCHAR(50)        NOT NULL,
        [AssessedAtUtc]   DATETIME2(3)        NOT NULL,
        [AssessedBy]      UNIQUEIDENTIFIER    NULL,
        RowVersion       ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_HazardAssessmentsReadModel_TenantId_AssessedAtUtc]
        ON [Operational].[HazardAssessmentsReadModel] ([TenantId], [AssessedAtUtc])
        INCLUDE ([SiteId]);
END
");

        // Table: HazardousMaterialsReadModel.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'HazardousMaterialsReadModel' AND schema_id = SCHEMA_ID('Operational'))
BEGIN
    CREATE TABLE [Operational].[HazardousMaterialsReadModel] (
        [HazardousMaterialId] UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]              UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                UNIQUEIDENTIFIER    NULL,
        [MaterialName]            NVARCHAR(400)       NOT NULL,
        [HazardClass]             NVARCHAR(400)       NOT NULL,
        [Status]                NVARCHAR(50)        NOT NULL,
        [RegisteredAtUtc] DATETIME2(3)        NOT NULL,
        [RegisteredBy]    UNIQUEIDENTIFIER    NULL,
        RowVersion       ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_HazardousMaterialsReadModel_TenantId_RegisteredAtUtc]
        ON [Operational].[HazardousMaterialsReadModel] ([TenantId], [RegisteredAtUtc])
        INCLUDE ([SiteId]);
END
");

        // Table: IndustrialHygieneReadModel.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'IndustrialHygieneReadModel' AND schema_id = SCHEMA_ID('Operational'))
BEGIN
    CREATE TABLE [Operational].[IndustrialHygieneReadModel] (
        [IndustrialHygieneSampleId] UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]              UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                UNIQUEIDENTIFIER    NULL,
        [SampleType]              NVARCHAR(400)       NOT NULL,
        [ExposureLevel]           NVARCHAR(400)       NOT NULL,
        [Status]                NVARCHAR(50)        NOT NULL,
        [SampledAtUtc]    DATETIME2(3)        NOT NULL,
        [SampledBy]       UNIQUEIDENTIFIER    NULL,
        RowVersion       ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_IndustrialHygieneReadModel_TenantId_SampledAtUtc]
        ON [Operational].[IndustrialHygieneReadModel] ([TenantId], [SampledAtUtc])
        INCLUDE ([SiteId]);
END
");

        // Table: InspectionsReadModel.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'InspectionsReadModel' AND schema_id = SCHEMA_ID('Operational'))
BEGIN
    CREATE TABLE [Operational].[InspectionsReadModel] (
        [InspectionId]    UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]              UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                UNIQUEIDENTIFIER    NULL,
        [InspectionType]          NVARCHAR(400)       NOT NULL,
        [Outcome]                 NVARCHAR(400)       NOT NULL,
        [Status]                NVARCHAR(50)        NOT NULL,
        [InspectedAtUtc]  DATETIME2(3)        NOT NULL,
        [InspectedBy]     UNIQUEIDENTIFIER    NULL,
        RowVersion       ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_InspectionsReadModel_TenantId_InspectedAtUtc]
        ON [Operational].[InspectionsReadModel] ([TenantId], [InspectedAtUtc])
        INCLUDE ([SiteId]);
END
");

        // Table: JobSafetyAnalysisReadModel.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'JobSafetyAnalysisReadModel' AND schema_id = SCHEMA_ID('Operational'))
BEGIN
    CREATE TABLE [Operational].[JobSafetyAnalysisReadModel] (
        [JobSafetyAnalysisId] UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]              UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                UNIQUEIDENTIFIER    NULL,
        [TaskName]                NVARCHAR(400)       NOT NULL,
        [RiskLevel]               NVARCHAR(400)       NOT NULL,
        [Status]                NVARCHAR(50)        NOT NULL,
        [AnalyzedAtUtc]   DATETIME2(3)        NOT NULL,
        [AnalyzedBy]      UNIQUEIDENTIFIER    NULL,
        RowVersion       ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_JobSafetyAnalysisReadModel_TenantId_AnalyzedAtUtc]
        ON [Operational].[JobSafetyAnalysisReadModel] ([TenantId], [AnalyzedAtUtc])
        INCLUDE ([SiteId]);
END
");

        // Table: LockoutTagoutReadModel.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'LockoutTagoutReadModel' AND schema_id = SCHEMA_ID('Operational'))
BEGIN
    CREATE TABLE [Operational].[LockoutTagoutReadModel] (
        [LockoutTagoutId] UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]              UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                UNIQUEIDENTIFIER    NULL,
        [EquipmentId]             UNIQUEIDENTIFIER    NOT NULL,
        [LockType]                NVARCHAR(400)       NOT NULL,
        [Status]                NVARCHAR(50)        NOT NULL,
        [LockedAtUtc]     DATETIME2(3)        NOT NULL,
        [LockedBy]        UNIQUEIDENTIFIER    NULL,
        RowVersion       ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_LockoutTagoutReadModel_TenantId_LockedAtUtc]
        ON [Operational].[LockoutTagoutReadModel] ([TenantId], [LockedAtUtc])
        INCLUDE ([SiteId]);
END
");

        // Table: OperationalExportItems.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'OperationalExportItems' AND schema_id = SCHEMA_ID('Operational'))
BEGIN
    CREATE TABLE [Operational].[OperationalExportItems] (
        [Id]                UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]          UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]            UNIQUEIDENTIFIER    NULL,
        [ResourceType]      NVARCHAR(100)       NOT NULL,
        [ResourceId]        UNIQUEIDENTIFIER    NOT NULL,
        [Title]             NVARCHAR(300)       NOT NULL,
        [Status]            NVARCHAR(50)        NOT NULL,
        [EffectiveAtUtc]    DATETIME2(3)        NOT NULL,
        [OwnerId]           UNIQUEIDENTIFIER    NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_OperationalExportItems_TenantId_EffectiveAtUtc]
        ON [Operational].[OperationalExportItems] ([TenantId], [EffectiveAtUtc])
        INCLUDE ([SiteId], [Status], [ResourceType]);
END
");

        // Table: OperationalResources.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'OperationalResources' AND schema_id = SCHEMA_ID('Operational'))
BEGIN
    CREATE TABLE [Operational].[OperationalResources] (
        Id               UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId         UNIQUEIDENTIFIER NOT NULL,
        SiteId           UNIQUEIDENTIFIER NULL,
        ResourceType     NVARCHAR(100)    NOT NULL,
        Status           NVARCHAR(50)     NOT NULL,
        AssignedToUserId UNIQUEIDENTIFIER NULL,
        Notes            NVARCHAR(MAX)    NULL,
        UpdatedAtUtc     DATETIME2(3)     NOT NULL,
        UpdatedBy        UNIQUEIDENTIFIER NULL,
        RowVersion       ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_OperationalResources_TenantId_SiteId] ON [Operational].[OperationalResources] (TenantId, SiteId);
END
");

        // Table: PermitsToWorkReadModel.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PermitsToWorkReadModel' AND schema_id = SCHEMA_ID('Operational'))
BEGIN
    CREATE TABLE [Operational].[PermitsToWorkReadModel] (
        [PermitId]        UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]              UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                UNIQUEIDENTIFIER    NULL,
        [PermitType]              NVARCHAR(400)       NOT NULL,
        [WorkDescription]         NVARCHAR(400)       NOT NULL,
        [Status]                NVARCHAR(50)        NOT NULL,
        [IssuedAtUtc]     DATETIME2(3)        NOT NULL,
        [IssuedBy]        UNIQUEIDENTIFIER    NULL,
        RowVersion       ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_PermitsToWorkReadModel_TenantId_IssuedAtUtc]
        ON [Operational].[PermitsToWorkReadModel] ([TenantId], [IssuedAtUtc])
        INCLUDE ([SiteId]);
END
");

        // Table: PPEManagementReadModel.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PPEManagementReadModel' AND schema_id = SCHEMA_ID('Operational'))
BEGIN
    CREATE TABLE [Operational].[PPEManagementReadModel] (
        [PPERecordId]     UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]              UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                UNIQUEIDENTIFIER    NULL,
        [EmployeeId]              UNIQUEIDENTIFIER    NOT NULL,
        [PPEType]                 NVARCHAR(400)       NOT NULL,
        [Status]                NVARCHAR(50)        NOT NULL,
        [IssuedAtUtc]     DATETIME2(3)        NOT NULL,
        [IssuedBy]        UNIQUEIDENTIFIER    NULL,
        RowVersion       ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_PPEManagementReadModel_TenantId_IssuedAtUtc]
        ON [Operational].[PPEManagementReadModel] ([TenantId], [IssuedAtUtc])
        INCLUDE ([SiteId]);
END
");

        // Table: PredictiveMaintenanceReadModel.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PredictiveMaintenanceReadModel' AND schema_id = SCHEMA_ID('Operational'))
BEGIN
    CREATE TABLE [Operational].[PredictiveMaintenanceReadModel] (
        [PredictiveMaintenanceAlertId] UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]              UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                UNIQUEIDENTIFIER    NULL,
        [EquipmentId]             UNIQUEIDENTIFIER    NOT NULL,
        [PredictedFailureType]    NVARCHAR(400)       NOT NULL,
        [Status]                NVARCHAR(50)        NOT NULL,
        [PredictedAtUtc]  DATETIME2(3)        NOT NULL,
        [ReviewedBy]      UNIQUEIDENTIFIER    NULL,
        RowVersion       ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_PredictiveMaintenanceReadModel_TenantId_PredictedAtUtc]
        ON [Operational].[PredictiveMaintenanceReadModel] ([TenantId], [PredictedAtUtc])
        INCLUDE ([SiteId]);
END
");

        // Table: PreventiveMaintenanceReadModel.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PreventiveMaintenanceReadModel' AND schema_id = SCHEMA_ID('Operational'))
BEGIN
    CREATE TABLE [Operational].[PreventiveMaintenanceReadModel] (
        [PreventiveMaintenanceTaskId] UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]              UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                UNIQUEIDENTIFIER    NULL,
        [EquipmentId]             UNIQUEIDENTIFIER    NOT NULL,
        [TaskDescription]         NVARCHAR(400)       NOT NULL,
        [Status]                NVARCHAR(50)        NOT NULL,
        [ScheduledAtUtc]  DATETIME2(3)        NOT NULL,
        [CompletedBy]     UNIQUEIDENTIFIER    NULL,
        RowVersion       ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_PreventiveMaintenanceReadModel_TenantId_ScheduledAtUtc]
        ON [Operational].[PreventiveMaintenanceReadModel] ([TenantId], [ScheduledAtUtc])
        INCLUDE ([SiteId]);
END
");

        // Table: RiskManagementReadModel.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'RiskManagementReadModel' AND schema_id = SCHEMA_ID('Operational'))
BEGIN
    CREATE TABLE [Operational].[RiskManagementReadModel] (
        [RiskRecordId]    UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]              UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                UNIQUEIDENTIFIER    NULL,
        [RiskDescription]         NVARCHAR(400)       NOT NULL,
        [RiskLevel]               NVARCHAR(400)       NOT NULL,
        [Status]                NVARCHAR(50)        NOT NULL,
        [IdentifiedAtUtc] DATETIME2(3)        NOT NULL,
        [IdentifiedBy]    UNIQUEIDENTIFIER    NULL,
        RowVersion       ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_RiskManagementReadModel_TenantId_IdentifiedAtUtc]
        ON [Operational].[RiskManagementReadModel] ([TenantId], [IdentifiedAtUtc])
        INCLUDE ([SiteId]);
END
");

        // Table: SchedulesReadModel.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'SchedulesReadModel' AND schema_id = SCHEMA_ID('Operational'))
BEGIN
    CREATE TABLE [Operational].[SchedulesReadModel] (
        [ScheduleId]      UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]              UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                UNIQUEIDENTIFIER    NULL,
        [ShiftName]               NVARCHAR(400)       NOT NULL,
        [ScheduleType]            NVARCHAR(400)       NOT NULL,
        [Status]                NVARCHAR(50)        NOT NULL,
        [ScheduledAtUtc]  DATETIME2(3)        NOT NULL,
        [AssignedBy]      UNIQUEIDENTIFIER    NULL,
        RowVersion       ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_SchedulesReadModel_TenantId_ScheduledAtUtc]
        ON [Operational].[SchedulesReadModel] ([TenantId], [ScheduledAtUtc])
        INCLUDE ([SiteId]);
END
");

        // Table: SeminarsReadModel.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'SeminarsReadModel' AND schema_id = SCHEMA_ID('Operational'))
BEGIN
    CREATE TABLE [Operational].[SeminarsReadModel] (
        [SeminarId]       UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]              UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                UNIQUEIDENTIFIER    NULL,
        [Title]                   NVARCHAR(400)       NOT NULL,
        [PresenterId]             UNIQUEIDENTIFIER    NOT NULL,
        [Status]                NVARCHAR(50)        NOT NULL,
        [ScheduledAtUtc]  DATETIME2(3)        NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_SeminarsReadModel_TenantId_ScheduledAtUtc]
        ON [Operational].[SeminarsReadModel] ([TenantId], [ScheduledAtUtc])
        INCLUDE ([SiteId]);
END
");

        // Table: TrainingReadModel.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'TrainingReadModel' AND schema_id = SCHEMA_ID('Operational'))
BEGIN
    CREATE TABLE [Operational].[TrainingReadModel] (
        [TrainingRecordId] UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]              UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                UNIQUEIDENTIFIER    NULL,
        [EmployeeId]              UNIQUEIDENTIFIER    NOT NULL,
        [CourseName]              NVARCHAR(400)       NOT NULL,
        [Status]                NVARCHAR(50)        NOT NULL,
        [CompletedAtUtc]  DATETIME2(3)        NOT NULL,
        [CertifiedBy]     UNIQUEIDENTIFIER    NULL,
        RowVersion       ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_TrainingReadModel_TenantId_CompletedAtUtc]
        ON [Operational].[TrainingReadModel] ([TenantId], [CompletedAtUtc])
        INCLUDE ([SiteId]);
END
");

        // Table: WebinarsReadModel.sql
        await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'WebinarsReadModel' AND schema_id = SCHEMA_ID('Operational'))
BEGIN
    CREATE TABLE [Operational].[WebinarsReadModel] (
        [WebinarId]       UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]              UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                UNIQUEIDENTIFIER    NULL,
        [Title]                   NVARCHAR(400)       NOT NULL,
        [HostId]                  UNIQUEIDENTIFIER    NOT NULL,
        [Status]                NVARCHAR(50)        NOT NULL,
        [ScheduledAtUtc]  DATETIME2(3)        NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_WebinarsReadModel_TenantId_ScheduledAtUtc]
        ON [Operational].[WebinarsReadModel] ([TenantId], [ScheduledAtUtc])
        INCLUDE ([SiteId]);
END
");

        // Procedure: usp_Alerts_List.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Operational].[usp_Alerts_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [AlertId],
        [TenantId],
        [SiteId],
        [Message],
        [Severity],
        [Status],
        [TriggeredAtUtc],
        [AcknowledgedBy]
FROM [Operational].[AlertsReadModel]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [TriggeredAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [TriggeredAtUtc] DESC;
");

        // Procedure: usp_Checklists_List.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Operational].[usp_Checklists_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [ChecklistId],
        [TenantId],
        [SiteId],
        [Name],
        [Category],
        [Status],
        [CompletedAtUtc],
        [CompletedBy]
FROM [Operational].[ChecklistsReadModel]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [CompletedAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [CompletedAtUtc] DESC;
");

        // Procedure: usp_Classes_List.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Operational].[usp_Classes_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [ClassId],
        [TenantId],
        [SiteId],
        [Title],
        [InstructorId],
        [Status],
        [ScheduledAtUtc]
FROM [Operational].[ClassesReadModel]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [ScheduledAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [ScheduledAtUtc] DESC;
");

        // Procedure: usp_CompetencyManagement_List.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Operational].[usp_CompetencyManagement_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [CompetencyRecordId],
        [TenantId],
        [SiteId],
        [EmployeeId],
        [CompetencyArea],
        [Status],
        [AssessedAtUtc],
        [AssessedBy]
FROM [Operational].[CompetencyManagementReadModel]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [AssessedAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [AssessedAtUtc] DESC;
");

        // Procedure: usp_Contractor_List.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Operational].[usp_Contractor_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [ContractorRecordId],
        [TenantId],
        [SiteId],
        [ContractorName],
        [ServiceType],
        [Status],
        [OnboardedAtUtc],
        [ApprovedBy]
FROM [Operational].[ContractorReadModel]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [OnboardedAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [OnboardedAtUtc] DESC;
");

        // Procedure: usp_Courses_List.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Operational].[usp_Courses_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [CourseId],
        [TenantId],
        [SiteId],
        [Title],
        [Category],
        [Status],
        [PublishedAtUtc],
        [PublishedBy]
FROM [Operational].[CoursesReadModel]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [PublishedAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [PublishedAtUtc] DESC;
");

        // Procedure: usp_EquipmentSafety_List.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Operational].[usp_EquipmentSafety_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [EquipmentSafetyRecordId],
        [TenantId],
        [SiteId],
        [EquipmentId],
        [EquipmentName],
        [Status],
        [InspectedAtUtc],
        [InspectedBy]
FROM [Operational].[EquipmentSafetyReadModel]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [InspectedAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [InspectedAtUtc] DESC;
");

        // Procedure: usp_Ergonomics_List.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Operational].[usp_Ergonomics_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [ErgonomicsAssessmentId],
        [TenantId],
        [SiteId],
        [WorkstationId],
        [RiskLevel],
        [Status],
        [AssessedAtUtc],
        [AssessedBy]
FROM [Operational].[ErgonomicsReadModel]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [AssessedAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [AssessedAtUtc] DESC;
");

        // Procedure: usp_FatigueManagement_List.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Operational].[usp_FatigueManagement_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [FatigueRecordId],
        [TenantId],
        [SiteId],
        [EmployeeId],
        [FatigueRiskLevel],
        [Status],
        [RecordedAtUtc],
        [RecordedBy]
FROM [Operational].[FatigueManagementReadModel]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [RecordedAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [RecordedAtUtc] DESC;
");

        // Procedure: usp_HazardAssessments_List.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Operational].[usp_HazardAssessments_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [HazardAssessmentId],
        [TenantId],
        [SiteId],
        [HazardDescription],
        [RiskLevel],
        [Status],
        [AssessedAtUtc],
        [AssessedBy]
FROM [Operational].[HazardAssessmentsReadModel]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [AssessedAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [AssessedAtUtc] DESC;
");

        // Procedure: usp_HazardousMaterials_List.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Operational].[usp_HazardousMaterials_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [HazardousMaterialId],
        [TenantId],
        [SiteId],
        [MaterialName],
        [HazardClass],
        [Status],
        [RegisteredAtUtc],
        [RegisteredBy]
FROM [Operational].[HazardousMaterialsReadModel]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [RegisteredAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [RegisteredAtUtc] DESC;
");

        // Procedure: usp_IndustrialHygiene_List.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Operational].[usp_IndustrialHygiene_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [IndustrialHygieneSampleId],
        [TenantId],
        [SiteId],
        [SampleType],
        [ExposureLevel],
        [Status],
        [SampledAtUtc],
        [SampledBy]
FROM [Operational].[IndustrialHygieneReadModel]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [SampledAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [SampledAtUtc] DESC;
");

        // Procedure: usp_Inspections_List.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Operational].[usp_Inspections_List]
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
        [InspectionType],
        [Outcome],
        [Status],
        [InspectedAtUtc],
        [InspectedBy]
FROM [Operational].[InspectionsReadModel]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [InspectedAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [InspectedAtUtc] DESC;
");

        // Procedure: usp_JobSafetyAnalysis_List.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Operational].[usp_JobSafetyAnalysis_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [JobSafetyAnalysisId],
        [TenantId],
        [SiteId],
        [TaskName],
        [RiskLevel],
        [Status],
        [AnalyzedAtUtc],
        [AnalyzedBy]
FROM [Operational].[JobSafetyAnalysisReadModel]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [AnalyzedAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [AnalyzedAtUtc] DESC;
");

        // Procedure: usp_LockoutTagout_List.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Operational].[usp_LockoutTagout_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [LockoutTagoutId],
        [TenantId],
        [SiteId],
        [EquipmentId],
        [LockType],
        [Status],
        [LockedAtUtc],
        [LockedBy]
FROM [Operational].[LockoutTagoutReadModel]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [LockedAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [LockedAtUtc] DESC;
");

        // Procedure: usp_OperationalExportItems_List.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Operational].[usp_OperationalExportItems_List]
    @TenantId       UNIQUEIDENTIFIER,
    @SiteId         UNIQUEIDENTIFIER = NULL,
    @FromUtc        DATETIME2(3) = NULL,
    @ToUtc          DATETIME2(3),
    @Status         NVARCHAR(50) = NULL,
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
FROM [Operational].[OperationalExportItems]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND (@FromUtc IS NULL OR [EffectiveAtUtc] >= @FromUtc)
  AND [EffectiveAtUtc] <= @ToUtc
  AND (@Status IS NULL OR [Status] = @Status)
ORDER BY [EffectiveAtUtc] DESC;
");

        // Procedure: usp_OperationalResources_BulkUpdate.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Operational].[usp_OperationalResources_BulkUpdate]
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

        UPDATE [Operational].[OperationalResources]
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

        DECLARE @RowsAffected INT = @@ROWCOUNT;

    COMMIT TRANSACTION;

    BEGIN TRY
        INSERT INTO dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
        VALUES (N'OperationalResources', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'ResourceId=', @ResourceId, N'; TenantId=', @TenantId, N'; Operation=', @Operation), 0, OBJECT_NAME(@@PROCID));
    END TRY
    BEGIN CATCH
    END CATCH;

    SELECT @RowsAffected;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;

    DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ResourceId=', @ResourceId, N'; TenantId=', @TenantId, N'; Operation=', @Operation, N'; User=', SYSTEM_USER);
    DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

    BEGIN TRY
        INSERT INTO dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));
    END TRY
    BEGIN CATCH
    END CATCH;

    THROW;
END CATCH;
");

        // Procedure: usp_PermitsToWork_List.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Operational].[usp_PermitsToWork_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [PermitId],
        [TenantId],
        [SiteId],
        [PermitType],
        [WorkDescription],
        [Status],
        [IssuedAtUtc],
        [IssuedBy]
FROM [Operational].[PermitsToWorkReadModel]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [IssuedAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [IssuedAtUtc] DESC;
");

        // Procedure: usp_PPEManagement_List.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Operational].[usp_PPEManagement_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [PPERecordId],
        [TenantId],
        [SiteId],
        [EmployeeId],
        [PPEType],
        [Status],
        [IssuedAtUtc],
        [IssuedBy]
FROM [Operational].[PPEManagementReadModel]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [IssuedAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [IssuedAtUtc] DESC;
");

        // Procedure: usp_PredictiveMaintenance_List.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Operational].[usp_PredictiveMaintenance_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [PredictiveMaintenanceAlertId],
        [TenantId],
        [SiteId],
        [EquipmentId],
        [PredictedFailureType],
        [Status],
        [PredictedAtUtc],
        [ReviewedBy]
FROM [Operational].[PredictiveMaintenanceReadModel]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [PredictedAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [PredictedAtUtc] DESC;
");

        // Procedure: usp_PreventiveMaintenance_List.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Operational].[usp_PreventiveMaintenance_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [PreventiveMaintenanceTaskId],
        [TenantId],
        [SiteId],
        [EquipmentId],
        [TaskDescription],
        [Status],
        [ScheduledAtUtc],
        [CompletedBy]
FROM [Operational].[PreventiveMaintenanceReadModel]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [ScheduledAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [ScheduledAtUtc] DESC;
");

        // Procedure: usp_RiskManagement_List.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Operational].[usp_RiskManagement_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [RiskRecordId],
        [TenantId],
        [SiteId],
        [RiskDescription],
        [RiskLevel],
        [Status],
        [IdentifiedAtUtc],
        [IdentifiedBy]
FROM [Operational].[RiskManagementReadModel]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [IdentifiedAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [IdentifiedAtUtc] DESC;
");

        // Procedure: usp_Schedules_List.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Operational].[usp_Schedules_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [ScheduleId],
        [TenantId],
        [SiteId],
        [ShiftName],
        [ScheduleType],
        [Status],
        [ScheduledAtUtc],
        [AssignedBy]
FROM [Operational].[SchedulesReadModel]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [ScheduledAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [ScheduledAtUtc] DESC;
");

        // Procedure: usp_Seminars_List.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Operational].[usp_Seminars_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [SeminarId],
        [TenantId],
        [SiteId],
        [Title],
        [PresenterId],
        [Status],
        [ScheduledAtUtc]
FROM [Operational].[SeminarsReadModel]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [ScheduledAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [ScheduledAtUtc] DESC;
");

        // Procedure: usp_Training_List.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Operational].[usp_Training_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [TrainingRecordId],
        [TenantId],
        [SiteId],
        [EmployeeId],
        [CourseName],
        [Status],
        [CompletedAtUtc],
        [CertifiedBy]
FROM [Operational].[TrainingReadModel]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [CompletedAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [CompletedAtUtc] DESC;
");

        // Procedure: usp_Webinars_List.sql
        await conn.ExecuteAsync(@"
CREATE OR ALTER PROCEDURE [Operational].[usp_Webinars_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [WebinarId],
        [TenantId],
        [SiteId],
        [Title],
        [HostId],
        [Status],
        [ScheduledAtUtc]
FROM [Operational].[WebinarsReadModel]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [ScheduledAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [ScheduledAtUtc] DESC;
");

    }


}

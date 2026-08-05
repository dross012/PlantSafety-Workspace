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
public static partial class TestDatabaseFactory
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
    /// Creates <c>[System].[CrossModuleDedupKeys]</c>, the business-key dedup ledger backing
    /// <c>CrossModuleIdempotentConsumerBase.TryExecuteOnceAsync</c> (Phase 6 blocker fix, 2026-08-02
    /// -- see PlantSafety.Infrastructure\Persistence\Scripts\Tables\CrossModuleDedupKeys.sql for the
    /// full rationale; mirrored here verbatim since this factory embeds its schema rather than
    /// executing the on-disk Scripts folder, matching every other ApplyXSchemaAsync method above).
    /// </summary>
    public static async Task ApplyCrossModuleDedupSchemaAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken);

        await conn.ExecuteAsync(@"
            IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'System')
                EXEC('CREATE SCHEMA [System]');

            IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES
                           WHERE TABLE_SCHEMA = 'System' AND TABLE_NAME = 'CrossModuleDedupKeys')
            BEGIN
                CREATE TABLE [System].[CrossModuleDedupKeys] (
                    ConsumerName   NVARCHAR(200)    NOT NULL,
                    BusinessKey    NVARCHAR(400)    NOT NULL,
                    EventId        UNIQUEIDENTIFIER NOT NULL,
                    ProcessedAtUtc DATETIME2(3)     NOT NULL CONSTRAINT DF_CrossModuleDedupKeys_ProcessedAtUtc DEFAULT (SYSUTCDATETIME()),
                    CONSTRAINT PK_CrossModuleDedupKeys PRIMARY KEY (ConsumerName, BusinessKey)
                );

                CREATE INDEX IX_CrossModuleDedupKeys_ProcessedAtUtc
                    ON [System].[CrossModuleDedupKeys] (ProcessedAtUtc);
            END;");
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
            IF COL_LENGTH('[RiskMgmt].[Risk]', 'RowVersion') IS NULL
            BEGIN
                ALTER TABLE [RiskMgmt].[Risk] ADD RowVersion ROWVERSION NOT NULL;
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
            IF COL_LENGTH('[RiskMgmt].[ControlMeasure]', 'RowVersion') IS NULL
            BEGIN
                ALTER TABLE [RiskMgmt].[ControlMeasure] ADD RowVersion ROWVERSION NOT NULL;
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

                    SELECT CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
                    FROM [RiskMgmt].[Risk]
                    WHERE RiskId = @RiskId;
                END TRY
                BEGIN CATCH
                    DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'RiskId=', @RiskId, N'; User=', SYSTEM_USER);
                    DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

                    INSERT INTO dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
                    VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

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

                    SELECT CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
                    FROM [RiskMgmt].[Risk]
                    WHERE RiskId = @RiskId;
                END TRY
                BEGIN CATCH
                    DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'RiskId=', @RiskId, N'; User=', SYSTEM_USER);
                    DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

                    INSERT INTO dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
                    VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

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

                    SELECT CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
                    FROM [RiskMgmt].[ControlMeasure]
                    WHERE ControlMeasureId = @ControlMeasureId;
                END TRY
                BEGIN CATCH
                    DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ControlMeasureId=', @ControlMeasureId, N'; RiskId=', @RiskId, N'; User=', SYSTEM_USER);
                    DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

                    INSERT INTO dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
                    VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

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

        // Additive: native ROWVERSION concurrency column, alongside the pre-existing (vestigial,
        // never read/written outside NEWID() churn) ConcurrencyToken UNIQUEIDENTIFIER column.
        await conn.ExecuteAsync(@"
            IF COL_LENGTH('[AccidentManagement].[Accident]', 'RowVersion') IS NULL
            BEGIN
                ALTER TABLE [AccidentManagement].[Accident] ADD RowVersion ROWVERSION NOT NULL;
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

        // Additive: native ROWVERSION concurrency column (no prior concurrency column existed).
        await conn.ExecuteAsync(@"
            IF COL_LENGTH('[AccidentManagement].[CorrectiveAction]', 'RowVersion') IS NULL
            BEGIN
                ALTER TABLE [AccidentManagement].[CorrectiveAction] ADD RowVersion ROWVERSION NOT NULL;
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

                    SELECT CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
                    FROM [AccidentManagement].[Accident]
                    WHERE AccidentId = @AccidentId;
                END TRY
                BEGIN CATCH
                    DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'AccidentId=', @AccidentId, N'; User=', SYSTEM_USER);
                    DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

                    INSERT INTO dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
                    VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

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

                    SELECT CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
                    FROM [AccidentManagement].[Accident]
                    WHERE AccidentId = @AccidentId AND TenantId = @TenantId;
                END TRY
                BEGIN CATCH
                    DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'AccidentId=', @AccidentId, N'; User=', SYSTEM_USER);
                    DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

                    INSERT INTO dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
                    VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

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
                    CreatedAtUtc, CreatedBy,
                    CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
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
                    CreatedAtUtc, CreatedBy,
                    CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
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

                    SELECT CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
                    FROM [AccidentManagement].[CorrectiveAction]
                    WHERE CorrectiveActionId = @CorrectiveActionId;
                END TRY
                BEGIN CATCH
                    DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'CorrectiveActionId=', @CorrectiveActionId, N'; AccidentId=', @AccidentId, N'; User=', SYSTEM_USER);
                    DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

                    INSERT INTO dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
                    VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

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
                    VerificationSteps, CompletionEvidence, CompletedAtUtc, VerifiedAtUtc, VerifiedBy, CreatedAtUtc,
                    CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
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

    /// <summary>
    /// Creates the <c>[Financials].[Financials]</c> read-model table + <c>usp_Financials_GetRecords</c>
    /// (mirroring PlantSafety.Infrastructure\Financials\Sql\Tables\Financials.sql and
    /// \Procedures\usp_Financials_GetRecords.sql verbatim) and the live ROI write path --
    /// <c>[Financials].[Roi]</c> + <c>usp_Roi_Create/GetByFiscalYear/List</c> (mirroring
    /// PlantSafety.Infrastructure\Features\Financials\ROI\Sql\Tables\Roi.sql and
    /// \Procedures\usp_Roi_*.sql verbatim) -- against the Testcontainers database. Per
    /// <see cref="ApplyIncidentManagementSchemaAsync"/>'s remarks, the
    /// <c>[PlantSafety.AuditDB]</c>/<c>[PlantSafety.ErrorDB]</c> three-part prefixes are stripped
    /// down to <c>dbo.AuditLog</c>/<c>dbo.ErrorLog</c> here, matching the single-database
    /// Testcontainers topology. Deliberately does NOT create the dead legacy
    /// <c>[Financials].[usp_Financial_GetById]</c>/RoiCalculation.sql tree (PlantSafety.Infrastructure\
    /// Financials\Persistence\FinancialsRepository.cs/FinancialsService.cs are registered as concrete
    /// classes in InfrastructureServiceRegistration but consumed by no Application-layer handler --
    /// flagged for separate cleanup, out of scope here).
    /// </summary>
    public static async Task ApplyFinancialsSchemaAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken);

        await conn.ExecuteAsync(@"
            IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Financials')
                EXEC('CREATE SCHEMA [Financials]');");

        // Table: Financials.sql
        await conn.ExecuteAsync(@"
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Financials' AND schema_id = SCHEMA_ID('Financials'))
            BEGIN
                CREATE TABLE [Financials].[Financials] (
                    RecordId       UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                    TenantId       UNIQUEIDENTIFIER NOT NULL,
                    SiteId         UNIQUEIDENTIFIER NULL,
                    Category       NVARCHAR(50)     NOT NULL,
                    Amount         DECIMAL(18,2)    NOT NULL,
                    Status         NVARCHAR(50)     NOT NULL,
                    RecordedAtUtc  DATETIME2(3)     NOT NULL,
                    RecordedBy     UNIQUEIDENTIFIER NOT NULL,
                    IsDeleted      BIT              NOT NULL CONSTRAINT DF_Financials_IsDeleted DEFAULT (0),
                    CreatedAtUtc   DATETIME2(3)     NOT NULL CONSTRAINT DF_Financials_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
                    CreatedBy      UNIQUEIDENTIFIER NULL,
                    ModifiedAtUtc  DATETIME2(3)     NULL,
                    ModifiedBy     UNIQUEIDENTIFIER NULL
                );

                CREATE NONCLUSTERED INDEX [IX_Financials_TenantId_SiteId] ON [Financials].[Financials] (TenantId, SiteId);
                CREATE NONCLUSTERED INDEX [IX_Financials_RecordedAtUtc] ON [Financials].[Financials] (RecordedAtUtc);
            END;");

        // Procedure: usp_Financials_GetRecords.sql
        await conn.ExecuteAsync(@"
            CREATE OR ALTER PROCEDURE [Financials].[usp_Financials_GetRecords]
                @TenantId UNIQUEIDENTIFIER,
                @SiteId   UNIQUEIDENTIFIER = NULL,
                @FromUtc  DATETIME2(3),
                @ToUtc    DATETIME2(3)
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT
                    RecordId,
                    TenantId,
                    SiteId,
                    Category,
                    Amount,
                    Status,
                    RecordedAtUtc,
                    RecordedBy
                FROM [Financials].[Financials]
                WHERE TenantId = @TenantId
                  AND IsDeleted = 0
                  AND (@SiteId IS NULL OR SiteId = @SiteId)
                  AND RecordedAtUtc >= @FromUtc
                  AND RecordedAtUtc <= @ToUtc
                ORDER BY RecordedAtUtc DESC;
            END;");

        // Table: Features\Financials\ROI\Sql\Tables\Roi.sql
        await conn.ExecuteAsync(@"
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Roi' AND schema_id = SCHEMA_ID('Financials'))
            BEGIN
                CREATE TABLE [Financials].[Roi] (
                    RoiCalculationId       UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                    TenantId               UNIQUEIDENTIFIER NOT NULL,
                    SiteId                 UNIQUEIDENTIFIER NULL,
                    FiscalYear             INT              NOT NULL,
                    TotalInvestment        DECIMAL(18,2)    NOT NULL,
                    EstimatedAvoidedCosts  DECIMAL(18,2)    NOT NULL,
                    NetBenefit             DECIMAL(18,2)    NOT NULL,
                    RoiPercentage          DECIMAL(9,4)     NOT NULL,
                    CurrencyCode           CHAR(3)          NOT NULL,
                    CalculatedAtUtc        DATETIMEOFFSET(3) NOT NULL,
                    IsDeleted              BIT              NOT NULL CONSTRAINT DF_Financials_Roi_IsDeleted DEFAULT (0),
                    CorrelationId          UNIQUEIDENTIFIER NULL,
                    CreatedAtUtc           DATETIME2(3)     NOT NULL CONSTRAINT DF_Financials_Roi_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
                    CreatedBy              UNIQUEIDENTIFIER NULL,
                    ModifiedAtUtc          DATETIME2(3)     NULL,
                    ModifiedBy             UNIQUEIDENTIFIER NULL
                );

                CREATE NONCLUSTERED INDEX [IX_Financials_Roi_TenantId_SiteId] ON [Financials].[Roi] (TenantId, SiteId);
                CREATE NONCLUSTERED INDEX [IX_Financials_Roi_FiscalYear] ON [Financials].[Roi] (TenantId, FiscalYear);
            END;");

        await conn.ExecuteAsync(@"
            IF COL_LENGTH('[Financials].[Roi]', 'RowVersion') IS NULL
            BEGIN
                ALTER TABLE [Financials].[Roi] ADD RowVersion ROWVERSION NOT NULL;
            END;");

        // Procedure: usp_Roi_Create.sql
        await conn.ExecuteAsync(@"
            CREATE OR ALTER PROCEDURE [Financials].[usp_Roi_Create]
                @TenantId               UNIQUEIDENTIFIER,
                @CorrelationId          UNIQUEIDENTIFIER,
                @ActorId                UNIQUEIDENTIFIER,
                @NowUtc                 DATETIME2(3),
                @RoiCalculationId       UNIQUEIDENTIFIER,
                @SiteId                 UNIQUEIDENTIFIER = NULL,
                @FiscalYear             INT,
                @TotalInvestment        DECIMAL(18,2),
                @EstimatedAvoidedCosts  DECIMAL(18,2),
                @NetBenefit             DECIMAL(18,2),
                @RoiPercentage          DECIMAL(18,2),
                @CurrencyCode           NVARCHAR(3)
            AS
            SET NOCOUNT ON;
            BEGIN
                BEGIN TRY
                    INSERT INTO [Financials].[Roi]
                        (RoiCalculationId, TenantId, SiteId, FiscalYear, TotalInvestment, EstimatedAvoidedCosts, NetBenefit,
                         RoiPercentage, CurrencyCode, CalculatedAtUtc, IsDeleted, CorrelationId, CreatedAtUtc, CreatedBy)
                    VALUES
                        (@RoiCalculationId, @TenantId, @SiteId, @FiscalYear, @TotalInvestment, @EstimatedAvoidedCosts, @NetBenefit,
                         @RoiPercentage, @CurrencyCode, SYSDATETIMEOFFSET(), 0, @CorrelationId, @NowUtc, @ActorId);

                    BEGIN TRY
                        INSERT INTO dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
                        VALUES (N'Roi', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'RoiCalculationId=', @RoiCalculationId), 0, OBJECT_NAME(@@PROCID));
                    END TRY
                    BEGIN CATCH
                    END CATCH;

                    SELECT CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
                    FROM [Financials].[Roi]
                    WHERE RoiCalculationId = @RoiCalculationId;
                END TRY
                BEGIN CATCH
                    DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'TenantId=', @TenantId, N'; RoiCalculationId=', @RoiCalculationId, N'; User=', SYSTEM_USER);
                    DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

                    INSERT INTO dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
                    VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

                    THROW;
                END CATCH
            END;");

        // Procedure: usp_Roi_GetByFiscalYear.sql
        await conn.ExecuteAsync(@"
            CREATE OR ALTER PROCEDURE [Financials].[usp_Roi_GetByFiscalYear]
                @TenantId   UNIQUEIDENTIFIER,
                @SiteId     UNIQUEIDENTIFIER = NULL,
                @FiscalYear INT
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT TOP (1)
                    RoiCalculationId,
                    TenantId,
                    SiteId,
                    FiscalYear,
                    TotalInvestment,
                    EstimatedAvoidedCosts,
                    NetBenefit,
                    RoiPercentage,
                    CurrencyCode,
                    CalculatedAtUtc,
                    CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
                FROM [Financials].[Roi]
                WHERE TenantId = @TenantId
                  AND IsDeleted = 0
                  AND (@SiteId IS NULL OR SiteId = @SiteId)
                  AND FiscalYear = @FiscalYear
                ORDER BY CalculatedAtUtc DESC;
            END;");

        // Procedure: usp_Roi_List.sql
        await conn.ExecuteAsync(@"
            CREATE OR ALTER PROCEDURE [Financials].[usp_Roi_List]
                @TenantId      UNIQUEIDENTIFIER,
                @SiteId        UNIQUEIDENTIFIER = NULL,
                @OrderBy       NVARCHAR(100) = N'CalculatedAtUtc',
                @SortDirection NVARCHAR(4)   = N'DESC',
                @PageNumber    INT,
                @PageSize      INT
            AS
            SET NOCOUNT ON;
            BEGIN
                DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
                IF @Offset < 0 SET @Offset = 0;

                SELECT
                    RoiCalculationId,
                    TenantId,
                    SiteId,
                    FiscalYear,
                    TotalInvestment,
                    EstimatedAvoidedCosts,
                    NetBenefit,
                    RoiPercentage,
                    CurrencyCode,
                    CalculatedAtUtc,
                    CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag,
                    COUNT(*) OVER () AS TotalCount
                FROM [Financials].[Roi]
                WHERE TenantId = @TenantId
                  AND IsDeleted = 0
                  AND (@SiteId IS NULL OR SiteId = @SiteId)
                ORDER BY
                    CASE WHEN @OrderBy = N'FiscalYear' AND @SortDirection = N'ASC' THEN FiscalYear END ASC,
                    CASE WHEN @OrderBy = N'FiscalYear' AND @SortDirection = N'DESC' THEN FiscalYear END DESC,
                    CASE WHEN @OrderBy = N'RoiPercentage' AND @SortDirection = N'ASC' THEN RoiPercentage END ASC,
                    CASE WHEN @OrderBy = N'RoiPercentage' AND @SortDirection = N'DESC' THEN RoiPercentage END DESC,
                    CASE WHEN (@OrderBy NOT IN (N'FiscalYear', N'RoiPercentage') OR @OrderBy IS NULL) AND @SortDirection = N'ASC' THEN CalculatedAtUtc END ASC,
                    CASE WHEN (@OrderBy NOT IN (N'FiscalYear', N'RoiPercentage') OR @OrderBy IS NULL) AND @SortDirection <> N'ASC' THEN CalculatedAtUtc END DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
            END;");
    }

    /// <summary>
    /// Creates the <c>[PlanningProjects].[PlanningProjects]</c> read-model table +
    /// <c>usp_PlanningProjects_GetProjects</c> (mirroring PlantSafety.Infrastructure\PlanningProjects\
    /// Sql\Tables\PlanningProjects.sql and \Procedures\usp_PlanningProjects_GetProjects.sql verbatim)
    /// and the live SafetyProject write path -- <c>[PlanningProjects].[Project]</c> +
    /// usp_Project_Create/GetById (mirroring PlantSafety.Infrastructure\Features\PlanningProjects\
    /// Projects\Sql\Tables\Project.sql and \Procedures\usp_Project_*.sql verbatim) -- against the
    /// Testcontainers database. Per <see cref="ApplyIncidentManagementSchemaAsync"/>'s remarks, the
    /// <c>[PlantSafety.AuditDB]</c>/<c>[PlantSafety.ErrorDB]</c> three-part prefixes are stripped
    /// down to <c>dbo.AuditLog</c>/<c>dbo.ErrorLog</c> here, matching the single-database
    /// Testcontainers topology. Deliberately does NOT create the dead legacy
    /// <c>PlanningProjects\Sql\Tables\Projects.sql</c> / <c>\Procedures\usp_Project_Insert.sql</c>
    /// tree (both are unimplemented "TODO" template stubs, never bound to any repository -- flagged
    /// separately, out of scope here) nor the concrete
    /// <c>PlantSafety.Infrastructure\PlanningProjects\Persistence\PlanningProjectsRepository.cs</c> /
    /// <c>Services\PlanningProjectsService.cs</c> classes (registered directly in
    /// InfrastructureServiceRegistration but consumed by no Application-layer handler -- likewise
    /// flagged separately).
    /// </summary>
    public static async Task ApplyPlanningProjectsSchemaAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken);

        // Schema
        await conn.ExecuteAsync(@"
            IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'PlanningProjects')
                EXEC('CREATE SCHEMA [PlanningProjects]');");

        // Table: PlanningProjects.sql (read-only)
        await conn.ExecuteAsync(@"
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PlanningProjects' AND schema_id = SCHEMA_ID('PlanningProjects'))
            BEGIN
                CREATE TABLE [PlanningProjects].[PlanningProjects] (
                    ProjectId             UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                    TenantId              UNIQUEIDENTIFIER NOT NULL,
                    SiteId                UNIQUEIDENTIFIER NULL,
                    Name                  NVARCHAR(200)    NOT NULL,
                    Status                NVARCHAR(50)     NOT NULL,
                    TargetCompletionUtc   DATETIME2(3)     NOT NULL,
                    OwnerId               UNIQUEIDENTIFIER NOT NULL,
                    IsDeleted             BIT              NOT NULL CONSTRAINT DF_PlanningProjects_IsDeleted DEFAULT (0),
                    CreatedAtUtc          DATETIME2(3)     NOT NULL CONSTRAINT DF_PlanningProjects_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
                    CreatedBy             UNIQUEIDENTIFIER NULL,
                    ModifiedAtUtc         DATETIME2(3)     NULL,
                    ModifiedBy            UNIQUEIDENTIFIER NULL
                );

                CREATE NONCLUSTERED INDEX [IX_PlanningProjects_TenantId_SiteId] ON [PlanningProjects].[PlanningProjects] (TenantId, SiteId);
                CREATE NONCLUSTERED INDEX [IX_PlanningProjects_TargetCompletionUtc] ON [PlanningProjects].[PlanningProjects] (TargetCompletionUtc);
            END");

        // Procedure: usp_PlanningProjects_GetProjects.sql
        await conn.ExecuteAsync(@"
            CREATE OR ALTER PROCEDURE [PlanningProjects].[usp_PlanningProjects_GetProjects]
                @TenantId UNIQUEIDENTIFIER,
                @SiteId   UNIQUEIDENTIFIER = NULL,
                @FromUtc  DATETIME2(3),
                @ToUtc    DATETIME2(3)
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT
                    ProjectId,
                    TenantId,
                    SiteId,
                    Name,
                    Status,
                    TargetCompletionUtc,
                    OwnerId
                FROM [PlanningProjects].[PlanningProjects]
                WHERE TenantId = @TenantId
                  AND IsDeleted = 0
                  AND (@SiteId IS NULL OR SiteId = @SiteId)
                  AND TargetCompletionUtc >= @FromUtc
                  AND TargetCompletionUtc <= @ToUtc
                ORDER BY TargetCompletionUtc DESC;
            END;");

        // Table: Features\PlanningProjects\Projects\Sql\Tables\Project.sql
        await conn.ExecuteAsync(@"
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Project' AND schema_id = SCHEMA_ID('PlanningProjects'))
            BEGIN
                CREATE TABLE [PlanningProjects].[Project] (
                    ProjectId            UNIQUEIDENTIFIER  NOT NULL PRIMARY KEY,
                    TenantId             UNIQUEIDENTIFIER  NOT NULL,
                    SiteId               UNIQUEIDENTIFIER  NOT NULL,
                    Name                 NVARCHAR(200)     NOT NULL,
                    Description          NVARCHAR(4000)    NOT NULL,
                    Status               NVARCHAR(30)      NOT NULL CONSTRAINT DF_Project_Status DEFAULT (N'Draft'),
                    Priority             NVARCHAR(20)      NOT NULL,
                    ProjectOwnerId       UNIQUEIDENTIFIER  NOT NULL,
                    ProjectManagerId     UNIQUEIDENTIFIER  NULL,
                    PlannedStartUtc      DATETIMEOFFSET(3) NOT NULL,
                    PlannedEndUtc        DATETIMEOFFSET(3) NOT NULL,
                    ActualStartUtc       DATETIMEOFFSET(3) NULL,
                    ActualEndUtc         DATETIMEOFFSET(3) NULL,
                    PlannedBudget        DECIMAL(18,2)     NULL,
                    OpenMilestoneCount   INT               NOT NULL CONSTRAINT DF_Project_OpenMilestoneCount DEFAULT (0),
                    LastModifiedAtUtc    DATETIMEOFFSET(3) NOT NULL,
                    IsDeleted            BIT               NOT NULL CONSTRAINT DF_Project_IsDeleted DEFAULT (0),
                    CorrelationId        UNIQUEIDENTIFIER  NULL,
                    CreatedAtUtc         DATETIME2(3)      NOT NULL CONSTRAINT DF_Project_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
                    CreatedBy            UNIQUEIDENTIFIER  NULL,
                    ModifiedAtUtc        DATETIME2(3)      NULL,
                    ModifiedBy           UNIQUEIDENTIFIER  NULL
                );

                CREATE NONCLUSTERED INDEX [IX_Project_TenantId_SiteId] ON [PlanningProjects].[Project] (TenantId, SiteId);
                CREATE NONCLUSTERED INDEX [IX_Project_TenantId_Status] ON [PlanningProjects].[Project] (TenantId, Status);
            END");

        await conn.ExecuteAsync(@"
            IF COL_LENGTH('[PlanningProjects].[Project]', 'RowVersion') IS NULL
            BEGIN
                ALTER TABLE [PlanningProjects].[Project] ADD RowVersion ROWVERSION NOT NULL;
            END");

        // Procedure: usp_Project_Create.sql
        await conn.ExecuteAsync(@"
            CREATE OR ALTER PROCEDURE [PlanningProjects].[usp_Project_Create]
                @TenantId          UNIQUEIDENTIFIER,
                @CorrelationId     UNIQUEIDENTIFIER,
                @ActorId           UNIQUEIDENTIFIER,
                @NowUtc            DATETIME2(3),
                @ProjectId         UNIQUEIDENTIFIER,
                @SiteId            UNIQUEIDENTIFIER,
                @Name              NVARCHAR(200),
                @Description       NVARCHAR(4000),
                @Priority          NVARCHAR(20),
                @ProjectOwnerId    UNIQUEIDENTIFIER,
                @ProjectManagerId  UNIQUEIDENTIFIER = NULL,
                @PlannedStartUtc   DATETIME2(3),
                @PlannedEndUtc     DATETIME2(3),
                @PlannedBudget     DECIMAL(18,2) = NULL
            AS
            SET NOCOUNT ON;
            BEGIN
                BEGIN TRY
                    INSERT INTO [PlanningProjects].[Project]
                        (ProjectId, TenantId, SiteId, Name, Description, Status, Priority, ProjectOwnerId, ProjectManagerId,
                         PlannedStartUtc, PlannedEndUtc, ActualStartUtc, ActualEndUtc, PlannedBudget, OpenMilestoneCount,
                         LastModifiedAtUtc, IsDeleted, CorrelationId, CreatedAtUtc, CreatedBy)
                    VALUES
                        (@ProjectId, @TenantId, @SiteId, @Name, @Description, N'Draft', @Priority, @ProjectOwnerId, @ProjectManagerId,
                         @PlannedStartUtc, @PlannedEndUtc, NULL, NULL, @PlannedBudget, 0,
                         @NowUtc, 0, @CorrelationId, @NowUtc, @ActorId);

                    BEGIN TRY
                        INSERT INTO dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
                        VALUES (N'Project', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'ProjectId=', @ProjectId), 0, OBJECT_NAME(@@PROCID));
                    END TRY
                    BEGIN CATCH
                    END CATCH;

                    SELECT CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
                    FROM [PlanningProjects].[Project]
                    WHERE ProjectId = @ProjectId;
                END TRY
                BEGIN CATCH
                    DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'TenantId=', @TenantId, N'; ProjectId=', @ProjectId, N'; User=', SYSTEM_USER);
                    DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

                    INSERT INTO dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
                    VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

                    THROW;
                END CATCH
            END;");

        // Procedure: usp_Project_GetById.sql
        await conn.ExecuteAsync(@"
            CREATE OR ALTER PROCEDURE [PlanningProjects].[usp_Project_GetById]
                @TenantId  UNIQUEIDENTIFIER,
                @ProjectId UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT
                    ProjectId,
                    TenantId,
                    SiteId,
                    Name,
                    Description,
                    Status,
                    Priority,
                    ProjectOwnerId,
                    ProjectManagerId,
                    PlannedStartUtc,
                    PlannedEndUtc,
                    ActualStartUtc,
                    ActualEndUtc,
                    PlannedBudget,
                    OpenMilestoneCount,
                    LastModifiedAtUtc,
                    CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
                FROM [PlanningProjects].[Project]
                WHERE ProjectId = @ProjectId
                  AND TenantId = @TenantId
                  AND IsDeleted = 0;
            END;");
    }

    /// <summary>
    /// Creates the <c>[ContinuousImprovement]</c> schema's four tables + six stored procedures
    /// against the Testcontainers database, mirroring the real
    /// <c>PlantSafety.Infrastructure\ContinuousImprovement\Sql\Tables\{CAPA,RCA,CorrectiveAction}.sql</c>
    /// and <c>\Procedures\usp_{CAPA_GetActions,RCA_GetRcaRecords,
    /// ContinuousImprovementExportItems_List,CorrectiveAction_Create,CorrectiveAction_GetById,
    /// CorrectiveAction_List}.sql</c> verbatim.
    ///
    /// Three distinctly-scoped tables/procedures live in this one schema -- do not conflate them:
    ///   1. <c>[ContinuousImprovement].[CAPA]</c> / <c>usp_CAPA_GetActions</c> -- read-only,
    ///      backs <c>ICAPARepository.GetActionsAsync</c> / <c>CAPADapperRepository</c>. No C# write
    ///      path exists for this table.
    ///   2. <c>[ContinuousImprovement].[RCA]</c> / <c>usp_RCA_GetRcaRecords</c> -- read-only,
    ///      backs <c>IRCARepository.GetRcaRecordsAsync</c> / <c>RCADapperRepository</c>. No C# write
    ///      path exists for this table either.
    ///   3. <c>[ContinuousImprovement].[CorrectiveAction]</c> / <c>usp_CorrectiveAction_Create/
    ///      GetById/List</c> -- the ONLY real write path in this module, backing
    ///      <c>ICorrectiveActionRepository</c> / <c>CorrectiveActionDapperRepository</c>
    ///      (PlantSafety.Infrastructure.Features.ContinuousImprovement.CAPA.Repositories, wired via
    ///      AddFeaturesLegacyRepositories()). This table is DISTINCT from both
    ///      <c>[ContinuousImprovement].[CAPA]</c> above and from
    ///      <c>[AccidentManagement].[CorrectiveAction]</c> (an unrelated child entity of Accident,
    ///      already modernized in Phase 2 -- see <see cref="ApplyAccidentManagementSchemaAsync"/>).
    ///      Confirmed already fully canonical as of this pass's SQL audit: RowVersion ROWVERSION
    ///      column present and genuinely live (not vestigial), ETag projected via
    ///      CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) in both GetById and List,
    ///      usp_CorrectiveAction_Create already has TRY/CATCH + AuditLog INSERT (best-effort, own
    ///      nested TRY/CATCH) + CATCH-block ErrorLog INSERT + THROW. No SQL changes were required;
    ///      this method only makes the pre-existing real schema testable. Per
    ///      <see cref="ApplyIncidentManagementSchemaAsync"/>'s remarks, the
    ///      <c>[PlantSafety.AuditDB]</c>/<c>[PlantSafety.ErrorDB]</c> three-part prefixes are
    ///      stripped down to <c>dbo.AuditLog</c>/<c>dbo.ErrorLog</c> here, matching the
    ///      single-database Testcontainers topology.
    ///
    /// Export (<c>usp_ContinuousImprovementExportItems_List</c>) is a read-only UNION ALL over CAPA
    /// and RCA, backing <c>IExportContinuousImprovementRepository.GenerateExportAsync</c> /
    /// <c>ExportContinuousImprovementDapperRepository</c> (uploads via IBlobStorageService to
    /// Azurite -- no additional schema needed beyond CAPA/RCA).
    /// </summary>
    public static async Task ApplyContinuousImprovementSchemaAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken);

        // Schema
        await conn.ExecuteAsync(@"
            IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'ContinuousImprovement')
                EXEC('CREATE SCHEMA [ContinuousImprovement]');");

        // Table: ContinuousImprovement\Sql\Tables\CAPA.sql (read-only)
        await conn.ExecuteAsync(@"
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CAPA' AND schema_id = SCHEMA_ID('ContinuousImprovement'))
            BEGIN
                CREATE TABLE [ContinuousImprovement].[CAPA] (
                    CapaId        UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                    TenantId      UNIQUEIDENTIFIER NOT NULL,
                    SiteId        UNIQUEIDENTIFIER NULL,
                    ActionType    NVARCHAR(20)     NOT NULL,
                    Title         NVARCHAR(200)    NOT NULL,
                    Status        NVARCHAR(50)     NOT NULL,
                    DueDateUtc    DATETIME2(3)     NOT NULL,
                    AssignedTo    UNIQUEIDENTIFIER NOT NULL,
                    IsDeleted     BIT              NOT NULL CONSTRAINT DF_CAPA_IsDeleted DEFAULT (0),
                    CreatedAtUtc  DATETIME2(3)     NOT NULL CONSTRAINT DF_CAPA_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
                    CreatedBy     UNIQUEIDENTIFIER NULL,
                    ModifiedAtUtc DATETIME2(3)     NULL,
                    ModifiedBy    UNIQUEIDENTIFIER NULL
                );

                CREATE NONCLUSTERED INDEX [IX_CAPA_TenantId_SiteId] ON [ContinuousImprovement].[CAPA] (TenantId, SiteId);
                CREATE NONCLUSTERED INDEX [IX_CAPA_DueDateUtc] ON [ContinuousImprovement].[CAPA] (DueDateUtc);
            END");

        // Procedure: usp_CAPA_GetActions.sql
        await conn.ExecuteAsync(@"
            CREATE OR ALTER PROCEDURE [ContinuousImprovement].[usp_CAPA_GetActions]
                @TenantId UNIQUEIDENTIFIER,
                @SiteId   UNIQUEIDENTIFIER = NULL,
                @FromUtc  DATETIME2(3),
                @ToUtc    DATETIME2(3)
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT
                    CapaId,
                    TenantId,
                    SiteId,
                    ActionType,
                    Title,
                    Status,
                    DueDateUtc,
                    AssignedTo
                FROM [ContinuousImprovement].[CAPA]
                WHERE TenantId = @TenantId
                  AND IsDeleted = 0
                  AND (@SiteId IS NULL OR SiteId = @SiteId)
                  AND DueDateUtc >= @FromUtc
                  AND DueDateUtc <= @ToUtc
                ORDER BY DueDateUtc DESC;
            END");

        // Table: ContinuousImprovement\Sql\Tables\RCA.sql (read-only)
        await conn.ExecuteAsync(@"
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'RCA' AND schema_id = SCHEMA_ID('ContinuousImprovement'))
            BEGIN
                CREATE TABLE [ContinuousImprovement].[RCA] (
                    RcaId             UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                    TenantId          UNIQUEIDENTIFIER NOT NULL,
                    SiteId            UNIQUEIDENTIFIER NULL,
                    ProblemStatement  NVARCHAR(MAX)    NOT NULL,
                    Methodology       NVARCHAR(50)     NOT NULL,
                    Status            NVARCHAR(50)     NOT NULL,
                    InitiatedAtUtc    DATETIME2(3)     NOT NULL,
                    OwnerUserId       UNIQUEIDENTIFIER NOT NULL,
                    IsDeleted         BIT              NOT NULL CONSTRAINT DF_RCA_IsDeleted DEFAULT (0),
                    CreatedAtUtc      DATETIME2(3)     NOT NULL CONSTRAINT DF_RCA_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
                    CreatedBy         UNIQUEIDENTIFIER NULL,
                    ModifiedAtUtc     DATETIME2(3)     NULL,
                    ModifiedBy        UNIQUEIDENTIFIER NULL
                );

                CREATE NONCLUSTERED INDEX [IX_RCA_TenantId_SiteId] ON [ContinuousImprovement].[RCA] (TenantId, SiteId);
                CREATE NONCLUSTERED INDEX [IX_RCA_InitiatedAtUtc] ON [ContinuousImprovement].[RCA] (InitiatedAtUtc);
            END");

        // Procedure: usp_RCA_GetRcaRecords.sql
        await conn.ExecuteAsync(@"
            CREATE OR ALTER PROCEDURE [ContinuousImprovement].[usp_RCA_GetRcaRecords]
                @TenantId UNIQUEIDENTIFIER,
                @SiteId   UNIQUEIDENTIFIER = NULL,
                @FromUtc  DATETIME2(3),
                @ToUtc    DATETIME2(3)
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT
                    RcaId,
                    TenantId,
                    SiteId,
                    ProblemStatement,
                    Methodology,
                    Status,
                    InitiatedAtUtc,
                    OwnerUserId
                FROM [ContinuousImprovement].[RCA]
                WHERE TenantId = @TenantId
                  AND IsDeleted = 0
                  AND (@SiteId IS NULL OR SiteId = @SiteId)
                  AND InitiatedAtUtc >= @FromUtc
                  AND InitiatedAtUtc <= @ToUtc
                ORDER BY InitiatedAtUtc DESC;
            END");

        // Procedure: usp_ContinuousImprovementExportItems_List.sql (read-only UNION ALL of CAPA + RCA)
        await conn.ExecuteAsync(@"
            CREATE OR ALTER PROCEDURE [ContinuousImprovement].[usp_ContinuousImprovementExportItems_List]
                @TenantId    UNIQUEIDENTIFIER,
                @SiteId      UNIQUEIDENTIFIER = NULL,
                @FromUtc     DATETIME2(3) = NULL,
                @ToUtc       DATETIME2(3),
                @Status      NVARCHAR(50) = NULL,
                @MaxRecords  INT
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT TOP (@MaxRecords) *
                FROM (
                    SELECT
                        CapaId       AS Id,
                        'CAPA'       AS ResourceType,
                        CapaId       AS ResourceId,
                        Title        AS Title,
                        Status       AS Status,
                        DueDateUtc   AS EffectiveAtUtc,
                        AssignedTo   AS OwnerId,
                        SiteId,
                        TenantId
                    FROM [ContinuousImprovement].[CAPA]
                    WHERE TenantId = @TenantId
                      AND IsDeleted = 0
                      AND (@SiteId IS NULL OR SiteId = @SiteId)
                      AND (@FromUtc IS NULL OR DueDateUtc >= @FromUtc)
                      AND DueDateUtc <= @ToUtc
                      AND (@Status IS NULL OR Status = @Status)

                    UNION ALL

                    SELECT
                        RcaId                                   AS Id,
                        'RCA'                                    AS ResourceType,
                        RcaId                                    AS ResourceId,
                        LEFT(ProblemStatement, 200)               AS Title,
                        Status                                    AS Status,
                        InitiatedAtUtc                            AS EffectiveAtUtc,
                        OwnerUserId                               AS OwnerId,
                        SiteId,
                        TenantId
                    FROM [ContinuousImprovement].[RCA]
                    WHERE TenantId = @TenantId
                      AND IsDeleted = 0
                      AND (@SiteId IS NULL OR SiteId = @SiteId)
                      AND (@FromUtc IS NULL OR InitiatedAtUtc >= @FromUtc)
                      AND InitiatedAtUtc <= @ToUtc
                      AND (@Status IS NULL OR Status = @Status)
                ) AS Combined
                ORDER BY EffectiveAtUtc DESC;
            END");

        // Table: Features\ContinuousImprovement\CAPA\Sql\Tables\CorrectiveAction.sql -- the ONLY real
        // write path in this module (see method-level remarks above). Already includes RowVersion.
        await conn.ExecuteAsync(@"
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CorrectiveAction' AND schema_id = SCHEMA_ID('ContinuousImprovement'))
            BEGIN
                CREATE TABLE [ContinuousImprovement].[CorrectiveAction] (
                    CaId                   UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                    TenantId               UNIQUEIDENTIFIER NOT NULL,
                    SourceEventId          UNIQUEIDENTIFIER NOT NULL,
                    SourceEventType        NVARCHAR(100)    NOT NULL,
                    Title                  NVARCHAR(200)    NOT NULL,
                    Description            NVARCHAR(4000)   NOT NULL,
                    ActionType             NVARCHAR(50)     NOT NULL,
                    Priority               NVARCHAR(50)     NOT NULL,
                    AssignedToWorkerId     UNIQUEIDENTIFIER NOT NULL,
                    DueAtUtc               DATETIME2(3)     NOT NULL,
                    Status                 NVARCHAR(50)     NOT NULL CONSTRAINT DF_CorrectiveAction_Status DEFAULT (N'Open'),
                    CompletedAtUtc         DATETIME2(3)     NULL,
                    IsDeleted              BIT              NOT NULL CONSTRAINT DF_CorrectiveAction_IsDeleted DEFAULT (0),
                    DeletedAtUtc           DATETIME2(3)     NULL,
                    DeletionReason         NVARCHAR(400)    NULL,
                    LastModifiedByActorId  UNIQUEIDENTIFIER NULL,
                    LastModifiedAtUtc      DATETIME2(3)     NOT NULL CONSTRAINT DF_CorrectiveAction_LastModifiedAtUtc DEFAULT (SYSUTCDATETIME()),
                    CreatedAtUtc           DATETIME2(3)     NOT NULL CONSTRAINT DF_CorrectiveAction_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
                    CreatedBy              UNIQUEIDENTIFIER NULL,
                    RowVersion             ROWVERSION       NOT NULL
                );

                CREATE NONCLUSTERED INDEX [IX_CorrectiveAction_TenantId_SourceEventId] ON [ContinuousImprovement].[CorrectiveAction] (TenantId, SourceEventId);
                CREATE NONCLUSTERED INDEX [IX_CorrectiveAction_TenantId_Status_DueAtUtc] ON [ContinuousImprovement].[CorrectiveAction] (TenantId, Status, DueAtUtc);
            END");

        // Procedure: usp_CorrectiveAction_Create.sql (TRY/CATCH + AuditLog + ErrorLog, already canonical)
        await conn.ExecuteAsync(@"
            CREATE OR ALTER PROCEDURE [ContinuousImprovement].[usp_CorrectiveAction_Create]
                @TenantId           UNIQUEIDENTIFIER,
                @CorrelationId      UNIQUEIDENTIFIER,
                @ActorId            UNIQUEIDENTIFIER,
                @NowUtc             DATETIME2(3),
                @CaId               UNIQUEIDENTIFIER,
                @SourceEventId      UNIQUEIDENTIFIER,
                @SourceEventType    NVARCHAR(100),
                @Title              NVARCHAR(200),
                @Description        NVARCHAR(4000),
                @ActionType         NVARCHAR(50),
                @Priority           NVARCHAR(50),
                @AssignedToWorkerId UNIQUEIDENTIFIER,
                @DueAtUtc           DATETIME2(3)
            AS
            SET NOCOUNT ON;
            BEGIN
                BEGIN TRY
                    INSERT INTO [ContinuousImprovement].[CorrectiveAction]
                    (
                        CaId, TenantId, SourceEventId, SourceEventType, Title, Description, ActionType,
                        Priority, AssignedToWorkerId, DueAtUtc, Status, CompletedAtUtc,
                        IsDeleted, DeletedAtUtc, DeletionReason,
                        LastModifiedByActorId, LastModifiedAtUtc, CreatedAtUtc, CreatedBy
                    )
                    VALUES
                    (
                        @CaId, @TenantId, @SourceEventId, @SourceEventType, @Title, @Description, @ActionType,
                        @Priority, @AssignedToWorkerId, @DueAtUtc, N'Open', NULL,
                        0, NULL, NULL,
                        @ActorId, @NowUtc, @NowUtc, @ActorId
                    );

                    BEGIN TRY
                        INSERT INTO dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
                        VALUES (N'CorrectiveAction', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'CaId=', @CaId, N'; CorrelationId=', @CorrelationId), 0, OBJECT_NAME(@@PROCID));
                    END TRY
                    BEGIN CATCH
                    END CATCH;
                END TRY
                BEGIN CATCH
                    DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'CaId=', @CaId, N'; CorrelationId=', @CorrelationId, N'; User=', SYSTEM_USER);
                    DECLARE @__ErrorDetails nvarchar(max) = CONCAT(
                        N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

                    BEGIN TRY
                        INSERT INTO dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
                        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));
                    END TRY
                    BEGIN CATCH
                    END CATCH;

                    THROW;
                END CATCH
            END");

        // Procedure: usp_CorrectiveAction_GetById.sql (ETag projected from ROWVERSION)
        await conn.ExecuteAsync(@"
            CREATE OR ALTER PROCEDURE [ContinuousImprovement].[usp_CorrectiveAction_GetById]
            (
                @TenantId UNIQUEIDENTIFIER,
                @CaId     UNIQUEIDENTIFIER
            )
            AS
            BEGIN
                SET NOCOUNT ON;

                BEGIN TRY
                    SELECT
                        CaId,
                        TenantId,
                        SourceEventId,
                        SourceEventType,
                        Title,
                        Description,
                        ActionType,
                        Priority,
                        AssignedToWorkerId,
                        DueAtUtc,
                        Status,
                        CompletedAtUtc,
                        LastModifiedAtUtc,
                        CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
                    FROM [ContinuousImprovement].[CorrectiveAction]
                    WHERE CaId = @CaId
                      AND TenantId = @TenantId
                      AND IsDeleted = 0;

                    RETURN 0;
                END TRY
                BEGIN CATCH
                    DECLARE @ErrorNumber INT = ERROR_NUMBER();
                    THROW;
                    RETURN @ErrorNumber;
                END CATCH;
            END");

        // Procedure: usp_CorrectiveAction_List.sql (ETag projected from ROWVERSION, windowed TotalCount)
        await conn.ExecuteAsync(@"
            CREATE OR ALTER PROCEDURE [ContinuousImprovement].[usp_CorrectiveAction_List]
            (
                @TenantId       UNIQUEIDENTIFIER,
                @SourceEventId  UNIQUEIDENTIFIER = NULL,
                @ActionType     NVARCHAR(50)     = NULL,
                @Priority       NVARCHAR(50)     = NULL,
                @Status         NVARCHAR(50)     = NULL,
                @OrderBy        NVARCHAR(100)    = 'DueAtUtc',
                @SortDirection  NVARCHAR(4)      = 'ASC',
                @PageNumber     INT              = 1,
                @PageSize       INT              = 25
            )
            AS
            BEGIN
                SET NOCOUNT ON;

                BEGIN TRY
                    SELECT
                        CaId,
                        TenantId,
                        SourceEventId,
                        SourceEventType,
                        Title,
                        Description,
                        ActionType,
                        Priority,
                        AssignedToWorkerId,
                        DueAtUtc,
                        Status,
                        CompletedAtUtc,
                        LastModifiedAtUtc,
                        CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag,
                        COUNT(*) OVER() AS TotalCount
                    FROM [ContinuousImprovement].[CorrectiveAction]
                    WHERE TenantId = @TenantId
                      AND IsDeleted = 0
                      AND (@SourceEventId IS NULL OR SourceEventId = @SourceEventId)
                      AND (@ActionType IS NULL OR ActionType = @ActionType)
                      AND (@Priority IS NULL OR Priority = @Priority)
                      AND (@Status IS NULL OR Status = @Status)
                    ORDER BY
                        CASE WHEN @OrderBy = 'DueAtUtc' AND @SortDirection = 'ASC'  THEN DueAtUtc END ASC,
                        CASE WHEN @OrderBy = 'DueAtUtc' AND @SortDirection = 'DESC' THEN DueAtUtc END DESC,
                        CASE WHEN @OrderBy = 'Priority' AND @SortDirection = 'ASC'  THEN Priority END ASC,
                        CASE WHEN @OrderBy = 'Priority' AND @SortDirection = 'DESC' THEN Priority END DESC,
                        CASE WHEN @OrderBy = 'Status'   AND @SortDirection = 'ASC'  THEN Status END ASC,
                        CASE WHEN @OrderBy = 'Status'   AND @SortDirection = 'DESC' THEN Status END DESC,
                        CASE WHEN @OrderBy = 'Title'    AND @SortDirection = 'ASC'  THEN Title END ASC,
                        CASE WHEN @OrderBy = 'Title'    AND @SortDirection = 'DESC' THEN Title END DESC,
                        DueAtUtc ASC
                    OFFSET (@PageNumber - 1) * @PageSize ROWS
                    FETCH NEXT @PageSize ROWS ONLY;

                    RETURN 0;
                END TRY
                BEGIN CATCH
                    DECLARE @ErrorNumber INT = ERROR_NUMBER();
                    THROW;
                    RETURN @ErrorNumber;
                END CATCH;
            END");
    }

    /// <summary>
    /// Creates the <c>[Intelligence]</c> schema's four read-model tables + four read-only stored
    /// procedures, mirroring <c>PlantSafety.Infrastructure\Intelligence\Sql\Tables\{AITraining,
    /// AllInvestigations,DataLake,PredictiveSafety}.sql</c> and
    /// <c>PlantSafety.Infrastructure\Intelligence\Sql\Procedures\usp_{AITraining_GetTrainingJobs,
    /// AllInvestigations_GetInvestigations,DataLake_GetDataLakeRecords,
    /// PredictiveSafety_GetPredictions}.sql</c> byte-for-byte (aside from the
    /// <c>[PlantSafety.ErrorDB].dbo.ErrorLog</c> -&gt; <c>dbo.ErrorLog</c> three-part-prefix
    /// stripping applied uniformly to every embedded copy in this file -- see
    /// ApplyRiskManagementSchemaAsync's remarks for why).
    /// <para>
    /// All four tables/procedures are genuinely read-only: there is no INSERT/UPDATE/DELETE path
    /// anywhere in this module (confirmed by direct inspection of every procedure body), so there
    /// is no ROWVERSION/ETag concept to add here. As a deliberate, explicit exception to this
    /// codebase's normal convention (read-only Get/List procs are intentionally TRY-free
    /// everywhere else in the program), all four read procedures below are wrapped in
    /// BEGIN TRY/BEGIN CATCH with a canonical ErrorLog insert on CATCH, then THROW to re-raise.
    /// No AuditLog insert (reads do not carry an audit requirement for this module).
    /// </para>
    /// </summary>
    public static async Task ApplyIntelligenceSchemaAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken);

        // Schema
        await conn.ExecuteAsync(@"
            IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Intelligence')
                EXEC('CREATE SCHEMA [Intelligence]');");

        // Table: Intelligence\Sql\Tables\AITraining.sql (read-only)
        await conn.ExecuteAsync(@"
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AITraining' AND schema_id = SCHEMA_ID('Intelligence'))
            BEGIN
                CREATE TABLE [Intelligence].[AITraining] (
                    TrainingJobId    UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                    TenantId         UNIQUEIDENTIFIER NOT NULL,
                    SiteId           UNIQUEIDENTIFIER NULL,
                    ModelName        NVARCHAR(200)    NOT NULL,
                    ModelVersion     NVARCHAR(50)     NOT NULL,
                    DatasetReference NVARCHAR(400)    NULL,
                    Status           NVARCHAR(50)     NOT NULL,
                    StartedAtUtc     DATETIME2(3)     NOT NULL,
                    InitiatedBy      UNIQUEIDENTIFIER NOT NULL,
                    IsDeleted        BIT              NOT NULL CONSTRAINT DF_AITraining_IsDeleted DEFAULT (0),
                    CreatedAtUtc     DATETIME2(3)     NOT NULL CONSTRAINT DF_AITraining_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
                    CreatedBy        UNIQUEIDENTIFIER NULL,
                    ModifiedAtUtc    DATETIME2(3)     NULL,
                    ModifiedBy       UNIQUEIDENTIFIER NULL
                );

                CREATE NONCLUSTERED INDEX [IX_AITraining_TenantId_SiteId] ON [Intelligence].[AITraining] (TenantId, SiteId);
                CREATE NONCLUSTERED INDEX [IX_AITraining_StartedAtUtc] ON [Intelligence].[AITraining] (StartedAtUtc);
            END");

        // Procedure: usp_AITraining_GetTrainingJobs.sql
        await conn.ExecuteAsync(@"
            CREATE OR ALTER PROCEDURE [Intelligence].[usp_AITraining_GetTrainingJobs]
                @TenantId UNIQUEIDENTIFIER,
                @SiteId   UNIQUEIDENTIFIER = NULL,
                @FromUtc  DATETIME2(3),
                @ToUtc    DATETIME2(3)
            AS
            SET NOCOUNT ON;
            BEGIN
                BEGIN TRY
                    SELECT
                        TrainingJobId,
                        TenantId,
                        SiteId,
                        ModelName,
                        ModelVersion,
                        DatasetReference,
                        Status,
                        StartedAtUtc,
                        InitiatedBy
                    FROM [Intelligence].[AITraining]
                    WHERE TenantId = @TenantId
                      AND IsDeleted = 0
                      AND (@SiteId IS NULL OR SiteId = @SiteId)
                      AND StartedAtUtc >= @FromUtc
                      AND StartedAtUtc <= @ToUtc
                    ORDER BY StartedAtUtc DESC;
                END TRY
                BEGIN CATCH
                    DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'TenantId=', @TenantId, N'; SiteId=', @SiteId, N'; User=', SYSTEM_USER);
                    DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

                    INSERT INTO dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
                    VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

                    THROW;
                END CATCH
            END");

        // Table: Intelligence\Sql\Tables\AllInvestigations.sql (read-only)
        await conn.ExecuteAsync(@"
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AllInvestigations' AND schema_id = SCHEMA_ID('Intelligence'))
            BEGIN
                CREATE TABLE [Intelligence].[AllInvestigations] (
                    InvestigationId    UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                    TenantId           UNIQUEIDENTIFIER NOT NULL,
                    SiteId             UNIQUEIDENTIFIER NULL,
                    IncidentReference  NVARCHAR(100)    NOT NULL,
                    AiConfidenceScore  FLOAT            NOT NULL,
                    Summary            NVARCHAR(MAX)    NULL,
                    Status             NVARCHAR(50)     NOT NULL,
                    OpenedAtUtc        DATETIME2(3)     NOT NULL,
                    AssignedTo         UNIQUEIDENTIFIER NOT NULL,
                    IsDeleted          BIT              NOT NULL CONSTRAINT DF_AllInvestigations_IsDeleted DEFAULT (0),
                    CreatedAtUtc       DATETIME2(3)     NOT NULL CONSTRAINT DF_AllInvestigations_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
                    CreatedBy          UNIQUEIDENTIFIER NULL,
                    ModifiedAtUtc      DATETIME2(3)     NULL,
                    ModifiedBy         UNIQUEIDENTIFIER NULL
                );

                CREATE NONCLUSTERED INDEX [IX_AllInvestigations_TenantId_SiteId] ON [Intelligence].[AllInvestigations] (TenantId, SiteId);
                CREATE NONCLUSTERED INDEX [IX_AllInvestigations_OpenedAtUtc] ON [Intelligence].[AllInvestigations] (OpenedAtUtc);
            END");

        // Procedure: usp_AllInvestigations_GetInvestigations.sql
        await conn.ExecuteAsync(@"
            CREATE OR ALTER PROCEDURE [Intelligence].[usp_AllInvestigations_GetInvestigations]
                @TenantId UNIQUEIDENTIFIER,
                @SiteId   UNIQUEIDENTIFIER = NULL,
                @FromUtc  DATETIME2(3),
                @ToUtc    DATETIME2(3)
            AS
            SET NOCOUNT ON;
            BEGIN
                BEGIN TRY
                    SELECT
                        InvestigationId,
                        TenantId,
                        SiteId,
                        IncidentReference,
                        AiConfidenceScore,
                        Summary,
                        Status,
                        OpenedAtUtc,
                        AssignedTo
                    FROM [Intelligence].[AllInvestigations]
                    WHERE TenantId = @TenantId
                      AND IsDeleted = 0
                      AND (@SiteId IS NULL OR SiteId = @SiteId)
                      AND OpenedAtUtc >= @FromUtc
                      AND OpenedAtUtc <= @ToUtc
                    ORDER BY OpenedAtUtc DESC;
                END TRY
                BEGIN CATCH
                    DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'TenantId=', @TenantId, N'; SiteId=', @SiteId, N'; User=', SYSTEM_USER);
                    DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

                    INSERT INTO dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
                    VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

                    THROW;
                END CATCH
            END");

        // Table: Intelligence\Sql\Tables\DataLake.sql (read-only)
        await conn.ExecuteAsync(@"
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'DataLake' AND schema_id = SCHEMA_ID('Intelligence'))
            BEGIN
                CREATE TABLE [Intelligence].[DataLake] (
                    DataLakeRecordId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                    TenantId         UNIQUEIDENTIFIER NOT NULL,
                    SiteId           UNIQUEIDENTIFIER NULL,
                    DatasetName      NVARCHAR(200)    NOT NULL,
                    Zone             NVARCHAR(50)     NOT NULL,
                    RecordCount      BIGINT           NULL,
                    Status           NVARCHAR(50)     NOT NULL,
                    ProcessedAtUtc   DATETIME2(3)     NOT NULL,
                    ProcessedBy      UNIQUEIDENTIFIER NOT NULL,
                    IsDeleted        BIT              NOT NULL CONSTRAINT DF_DataLake_IsDeleted DEFAULT (0),
                    CreatedAtUtc     DATETIME2(3)     NOT NULL CONSTRAINT DF_DataLake_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
                    CreatedBy        UNIQUEIDENTIFIER NULL,
                    ModifiedAtUtc    DATETIME2(3)     NULL,
                    ModifiedBy       UNIQUEIDENTIFIER NULL
                );

                CREATE NONCLUSTERED INDEX [IX_DataLake_TenantId_SiteId] ON [Intelligence].[DataLake] (TenantId, SiteId);
                CREATE NONCLUSTERED INDEX [IX_DataLake_ProcessedAtUtc] ON [Intelligence].[DataLake] (ProcessedAtUtc);
            END");

        // Procedure: usp_DataLake_GetDataLakeRecords.sql
        await conn.ExecuteAsync(@"
            CREATE OR ALTER PROCEDURE [Intelligence].[usp_DataLake_GetDataLakeRecords]
                @TenantId UNIQUEIDENTIFIER,
                @SiteId   UNIQUEIDENTIFIER = NULL,
                @FromUtc  DATETIME2(3),
                @ToUtc    DATETIME2(3)
            AS
            SET NOCOUNT ON;
            BEGIN
                BEGIN TRY
                    SELECT
                        DataLakeRecordId,
                        TenantId,
                        SiteId,
                        DatasetName,
                        Zone,
                        RecordCount,
                        Status,
                        ProcessedAtUtc,
                        ProcessedBy
                    FROM [Intelligence].[DataLake]
                    WHERE TenantId = @TenantId
                      AND IsDeleted = 0
                      AND (@SiteId IS NULL OR SiteId = @SiteId)
                      AND ProcessedAtUtc >= @FromUtc
                      AND ProcessedAtUtc <= @ToUtc
                    ORDER BY ProcessedAtUtc DESC;
                END TRY
                BEGIN CATCH
                    DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'TenantId=', @TenantId, N'; SiteId=', @SiteId, N'; User=', SYSTEM_USER);
                    DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

                    INSERT INTO dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
                    VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

                    THROW;
                END CATCH
            END");

        // Table: Intelligence\Sql\Tables\PredictiveSafety.sql (read-only)
        await conn.ExecuteAsync(@"
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PredictiveSafety' AND schema_id = SCHEMA_ID('Intelligence'))
            BEGIN
                CREATE TABLE [Intelligence].[PredictiveSafety] (
                    PredictionId       UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                    TenantId           UNIQUEIDENTIFIER NOT NULL,
                    SiteId             UNIQUEIDENTIFIER NULL,
                    RiskCategory       NVARCHAR(100)    NOT NULL,
                    PredictedRiskScore FLOAT            NOT NULL,
                    ModelVersion       NVARCHAR(50)     NULL,
                    Status             NVARCHAR(50)     NOT NULL,
                    PredictedAtUtc     DATETIME2(3)     NOT NULL,
                    GeneratedBy        UNIQUEIDENTIFIER NOT NULL,
                    IsDeleted          BIT              NOT NULL CONSTRAINT DF_PredictiveSafety_IsDeleted DEFAULT (0),
                    CreatedAtUtc       DATETIME2(3)     NOT NULL CONSTRAINT DF_PredictiveSafety_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
                    CreatedBy          UNIQUEIDENTIFIER NULL,
                    ModifiedAtUtc      DATETIME2(3)     NULL,
                    ModifiedBy         UNIQUEIDENTIFIER NULL
                );

                CREATE NONCLUSTERED INDEX [IX_PredictiveSafety_TenantId_SiteId] ON [Intelligence].[PredictiveSafety] (TenantId, SiteId);
                CREATE NONCLUSTERED INDEX [IX_PredictiveSafety_PredictedAtUtc] ON [Intelligence].[PredictiveSafety] (PredictedAtUtc);
            END");

        // Procedure: usp_PredictiveSafety_GetPredictions.sql
        await conn.ExecuteAsync(@"
            CREATE OR ALTER PROCEDURE [Intelligence].[usp_PredictiveSafety_GetPredictions]
                @TenantId UNIQUEIDENTIFIER,
                @SiteId   UNIQUEIDENTIFIER = NULL,
                @FromUtc  DATETIME2(3),
                @ToUtc    DATETIME2(3)
            AS
            SET NOCOUNT ON;
            BEGIN
                BEGIN TRY
                    SELECT
                        PredictionId,
                        TenantId,
                        SiteId,
                        RiskCategory,
                        PredictedRiskScore,
                        ModelVersion,
                        Status,
                        PredictedAtUtc,
                        GeneratedBy
                    FROM [Intelligence].[PredictiveSafety]
                    WHERE TenantId = @TenantId
                      AND IsDeleted = 0
                      AND (@SiteId IS NULL OR SiteId = @SiteId)
                      AND PredictedAtUtc >= @FromUtc
                      AND PredictedAtUtc <= @ToUtc
                    ORDER BY PredictedAtUtc DESC;
                END TRY
                BEGIN CATCH
                    DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'TenantId=', @TenantId, N'; SiteId=', @SiteId, N'; User=', SYSTEM_USER);
                    DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

                    INSERT INTO dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
                    VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

                    THROW;
                END CATCH
            END");
    }

    /// <summary>
    /// Creates the <c>[Integrations]</c> schema's 14 tables + 14 stored procedures, mirroring
    /// <c>PlantSafety.Infrastructure\Integrations\Sql\Tables\*.sql</c> and
    /// <c>PlantSafety.Infrastructure\Integrations\Sql\Procedures\*.sql</c> byte-for-byte
    /// (module-relative -- no <c>USE [PlantSafety.CoreDB]</c> / cross-database
    /// AuditLog/ErrorLog prefixes, matching every other Apply*SchemaAsync method in this class).
    /// 13 of the 14 tables/procedures (Analytics, Analyzers, Auditing, Documents, Email, Export,
    /// Import, IoT, Notifications.Email, Notifications.Internal, Notifications.Texting,
    /// Reporting, Texting) are genuinely read-only -- their Get/List procedures are wrapped in
    /// BEGIN TRY/BEGIN CATCH with a canonical ErrorLog insert + THROW (the same deliberate,
    /// explicit exception to the normal read-proc convention applied to Intelligence's four
    /// read-only procedures above), but deliberately carry no AuditLog and no ROWVERSION/ETag.
    /// The 14th, [Integrations].[InboundWebhookEvents] / usp_InboundWebhookEvents_RecordEvent, is
    /// the module's one genuine write path (idempotent check-and-insert keyed by
    /// (TenantId, EventId)) and is modernized to the full write-procedure canonical template:
    /// RowVersion/ETag, TRY/CATCH, best-effort AuditLog on success, and ErrorLog + THROW on
    /// failure.
    /// </summary>
    public static async Task ApplyIntegrationsSchemaAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken);

        // Schema
        await conn.ExecuteAsync(@"
            IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Integrations')
                EXEC('CREATE SCHEMA [Integrations]');");

        // Table: Integrations\Sql\Tables\AnalyticsItems.sql (read-only)
        await conn.ExecuteAsync(@"
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AnalyticsItems' AND schema_id = SCHEMA_ID('Integrations'))
            BEGIN
                CREATE TABLE [Integrations].[AnalyticsItems] (
                    RecordId      UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                    TenantId      UNIQUEIDENTIFIER NOT NULL,
                    SiteId        UNIQUEIDENTIFIER NULL,
                    SourceSystem  NVARCHAR(200)    NOT NULL,
                    MetricName    NVARCHAR(200)    NOT NULL,
                    MetricValue   DECIMAL(18,4)    NOT NULL,
                    Status        NVARCHAR(50)     NOT NULL,
                    IngestedAtUtc DATETIME2(3)     NOT NULL,
                    IngestedBy    UNIQUEIDENTIFIER NOT NULL,
                    IsDeleted     BIT              NOT NULL CONSTRAINT DF_AnalyticsItems_IsDeleted DEFAULT (0),
                    CreatedUtc    DATETIME2(3)     NOT NULL CONSTRAINT DF_AnalyticsItems_CreatedUtc DEFAULT (SYSUTCDATETIME()),
                    CreatedBy     UNIQUEIDENTIFIER NULL,
                    UpdatedUtc    DATETIME2(3)     NULL,
                    UpdatedBy     UNIQUEIDENTIFIER NULL
                );

                CREATE NONCLUSTERED INDEX [IX_AnalyticsItems_TenantId_SiteId] ON [Integrations].[AnalyticsItems] (TenantId, SiteId);
                CREATE NONCLUSTERED INDEX [IX_AnalyticsItems_IngestedAtUtc] ON [Integrations].[AnalyticsItems] (IngestedAtUtc);
            END");

        // Procedure: usp_Analytics_GetRecords.sql
        await conn.ExecuteAsync(@"
            CREATE OR ALTER PROCEDURE [Integrations].[usp_Analytics_GetRecords]
                @TenantId UNIQUEIDENTIFIER,
                @SiteId   UNIQUEIDENTIFIER = NULL,
                @FromUtc  DATETIME2(3),
                @ToUtc    DATETIME2(3)
            AS
            SET NOCOUNT ON;
            BEGIN
                BEGIN TRY
                    SELECT
                        RecordId,
                        TenantId,
                        SiteId,
                        SourceSystem,
                        MetricName,
                        MetricValue,
                        Status,
                        IngestedAtUtc,
                        IngestedBy
                    FROM [Integrations].[AnalyticsItems]
                    WHERE TenantId = @TenantId
                      AND (@SiteId IS NULL OR SiteId = @SiteId)
                      AND IsDeleted = 0
                      AND IngestedAtUtc >= @FromUtc
                      AND IngestedAtUtc <= @ToUtc
                    ORDER BY IngestedAtUtc DESC;
                END TRY
                BEGIN CATCH
                    DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'TenantId=', @TenantId, N'; SiteId=', @SiteId, N'; User=', SYSTEM_USER);
                    DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

                    INSERT INTO dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
                    VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

                    THROW;
                END CATCH
            END");

        // Table: Integrations\Sql\Tables\AnalyzersItems.sql (read-only)
        await conn.ExecuteAsync(@"
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AnalyzersItems' AND schema_id = SCHEMA_ID('Integrations'))
            BEGIN
                CREATE TABLE [Integrations].[AnalyzersItems] (
                    RunId          UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                    TenantId       UNIQUEIDENTIFIER NOT NULL,
                    SiteId         UNIQUEIDENTIFIER NULL,
                    AnalyzerName   NVARCHAR(200)    NOT NULL,
                    TargetSystem   NVARCHAR(200)    NOT NULL,
                    FindingsCount  INT              NOT NULL,
                    Status         NVARCHAR(50)     NOT NULL,
                    RunAtUtc       DATETIME2(3)     NOT NULL,
                    TriggeredBy    UNIQUEIDENTIFIER NOT NULL,
                    IsDeleted      BIT              NOT NULL CONSTRAINT DF_AnalyzersItems_IsDeleted DEFAULT (0),
                    CreatedUtc     DATETIME2(3)     NOT NULL CONSTRAINT DF_AnalyzersItems_CreatedUtc DEFAULT (SYSUTCDATETIME()),
                    CreatedBy      UNIQUEIDENTIFIER NULL,
                    UpdatedUtc     DATETIME2(3)     NULL,
                    UpdatedBy      UNIQUEIDENTIFIER NULL
                );

                CREATE NONCLUSTERED INDEX [IX_AnalyzersItems_TenantId_SiteId] ON [Integrations].[AnalyzersItems] (TenantId, SiteId);
                CREATE NONCLUSTERED INDEX [IX_AnalyzersItems_RunAtUtc] ON [Integrations].[AnalyzersItems] (RunAtUtc);
            END");

        // Procedure: usp_Analyzers_GetRuns.sql
        await conn.ExecuteAsync(@"
            CREATE OR ALTER PROCEDURE [Integrations].[usp_Analyzers_GetRuns]
                @TenantId UNIQUEIDENTIFIER,
                @SiteId   UNIQUEIDENTIFIER = NULL,
                @FromUtc  DATETIME2(3),
                @ToUtc    DATETIME2(3)
            AS
            SET NOCOUNT ON;
            BEGIN
                BEGIN TRY
                    SELECT
                        RunId,
                        TenantId,
                        SiteId,
                        AnalyzerName,
                        TargetSystem,
                        FindingsCount,
                        Status,
                        RunAtUtc,
                        TriggeredBy
                    FROM [Integrations].[AnalyzersItems]
                    WHERE TenantId = @TenantId
                      AND (@SiteId IS NULL OR SiteId = @SiteId)
                      AND IsDeleted = 0
                      AND RunAtUtc >= @FromUtc
                      AND RunAtUtc <= @ToUtc
                    ORDER BY RunAtUtc DESC;
                END TRY
                BEGIN CATCH
                    DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'TenantId=', @TenantId, N'; SiteId=', @SiteId, N'; User=', SYSTEM_USER);
                    DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

                    INSERT INTO dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
                    VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

                    THROW;
                END CATCH
            END");

        // Table: Integrations\Sql\Tables\AuditingItems.sql (read-only)
        await conn.ExecuteAsync(@"
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AuditingItems' AND schema_id = SCHEMA_ID('Integrations'))
            BEGIN
                CREATE TABLE [Integrations].[AuditingItems] (
                    LogId           UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                    TenantId        UNIQUEIDENTIFIER NOT NULL,
                    SiteId          UNIQUEIDENTIFIER NULL,
                    ExternalSystem  NVARCHAR(200)    NOT NULL,
                    Direction       NVARCHAR(20)     NOT NULL,
                    Endpoint        NVARCHAR(400)    NOT NULL,
                    Status          NVARCHAR(50)     NOT NULL,
                    OccurredAtUtc   DATETIME2(3)     NOT NULL,
                    InitiatedBy     UNIQUEIDENTIFIER NOT NULL,
                    IsDeleted       BIT              NOT NULL CONSTRAINT DF_AuditingItems_IsDeleted DEFAULT (0),
                    CreatedUtc      DATETIME2(3)     NOT NULL CONSTRAINT DF_AuditingItems_CreatedUtc DEFAULT (SYSUTCDATETIME()),
                    CreatedBy       UNIQUEIDENTIFIER NULL,
                    UpdatedUtc      DATETIME2(3)     NULL,
                    UpdatedBy       UNIQUEIDENTIFIER NULL
                );

                CREATE NONCLUSTERED INDEX [IX_AuditingItems_TenantId_SiteId] ON [Integrations].[AuditingItems] (TenantId, SiteId);
                CREATE NONCLUSTERED INDEX [IX_AuditingItems_OccurredAtUtc] ON [Integrations].[AuditingItems] (OccurredAtUtc);
            END");

        // Procedure: usp_Auditing_GetLogs.sql
        await conn.ExecuteAsync(@"
            CREATE OR ALTER PROCEDURE [Integrations].[usp_Auditing_GetLogs]
                @TenantId UNIQUEIDENTIFIER,
                @SiteId   UNIQUEIDENTIFIER = NULL,
                @FromUtc  DATETIME2(3),
                @ToUtc    DATETIME2(3)
            AS
            SET NOCOUNT ON;
            BEGIN
                BEGIN TRY
                    SELECT
                        LogId,
                        TenantId,
                        SiteId,
                        ExternalSystem,
                        Direction,
                        Endpoint,
                        Status,
                        OccurredAtUtc,
                        InitiatedBy
                    FROM [Integrations].[AuditingItems]
                    WHERE TenantId = @TenantId
                      AND (@SiteId IS NULL OR SiteId = @SiteId)
                      AND IsDeleted = 0
                      AND OccurredAtUtc >= @FromUtc
                      AND OccurredAtUtc <= @ToUtc
                    ORDER BY OccurredAtUtc DESC;
                END TRY
                BEGIN CATCH
                    DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'TenantId=', @TenantId, N'; SiteId=', @SiteId, N'; User=', SYSTEM_USER);
                    DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

                    INSERT INTO dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
                    VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

                    THROW;
                END CATCH
            END");

        // Table: Integrations\Sql\Tables\DocumentsItems.sql (read-only)
        await conn.ExecuteAsync(@"
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'DocumentsItems' AND schema_id = SCHEMA_ID('Integrations'))
            BEGIN
                CREATE TABLE [Integrations].[DocumentsItems] (
                    DocumentId    UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                    TenantId      UNIQUEIDENTIFIER NOT NULL,
                    SiteId        UNIQUEIDENTIFIER NULL,
                    SourceSystem  NVARCHAR(200)    NOT NULL,
                    FileName      NVARCHAR(400)    NOT NULL,
                    DocumentType  NVARCHAR(100)    NOT NULL,
                    Status        NVARCHAR(50)     NOT NULL,
                    SyncedAtUtc   DATETIME2(3)     NOT NULL,
                    SyncedBy      UNIQUEIDENTIFIER NOT NULL,
                    IsDeleted     BIT              NOT NULL CONSTRAINT DF_DocumentsItems_IsDeleted DEFAULT (0),
                    CreatedUtc    DATETIME2(3)     NOT NULL CONSTRAINT DF_DocumentsItems_CreatedUtc DEFAULT (SYSUTCDATETIME()),
                    CreatedBy     UNIQUEIDENTIFIER NULL,
                    UpdatedUtc    DATETIME2(3)     NULL,
                    UpdatedBy     UNIQUEIDENTIFIER NULL
                );

                CREATE NONCLUSTERED INDEX [IX_DocumentsItems_TenantId_SiteId] ON [Integrations].[DocumentsItems] (TenantId, SiteId);
                CREATE NONCLUSTERED INDEX [IX_DocumentsItems_SyncedAtUtc] ON [Integrations].[DocumentsItems] (SyncedAtUtc);
            END");

        // Procedure: usp_Documents_GetDocuments.sql
        await conn.ExecuteAsync(@"
            CREATE OR ALTER PROCEDURE [Integrations].[usp_Documents_GetDocuments]
                @TenantId UNIQUEIDENTIFIER,
                @SiteId   UNIQUEIDENTIFIER = NULL,
                @FromUtc  DATETIME2(3),
                @ToUtc    DATETIME2(3)
            AS
            SET NOCOUNT ON;
            BEGIN
                BEGIN TRY
                    SELECT
                        DocumentId,
                        TenantId,
                        SiteId,
                        SourceSystem,
                        FileName,
                        DocumentType,
                        Status,
                        SyncedAtUtc,
                        SyncedBy
                    FROM [Integrations].[DocumentsItems]
                    WHERE TenantId = @TenantId
                      AND (@SiteId IS NULL OR SiteId = @SiteId)
                      AND IsDeleted = 0
                      AND SyncedAtUtc >= @FromUtc
                      AND SyncedAtUtc <= @ToUtc
                    ORDER BY SyncedAtUtc DESC;
                END TRY
                BEGIN CATCH
                    DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'TenantId=', @TenantId, N'; SiteId=', @SiteId, N'; User=', SYSTEM_USER);
                    DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

                    INSERT INTO dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
                    VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

                    THROW;
                END CATCH
            END");

        // Table: Integrations\Sql\Tables\EmailItems.sql (read-only, root-level Integrations.Email)
        await conn.ExecuteAsync(@"
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'EmailItems' AND schema_id = SCHEMA_ID('Integrations'))
            BEGIN
                CREATE TABLE [Integrations].[EmailItems] (
                    EmailId       UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                    TenantId      UNIQUEIDENTIFIER NOT NULL,
                    SiteId        UNIQUEIDENTIFIER NULL,
                    Recipient     NVARCHAR(320)    NOT NULL,
                    Subject       NVARCHAR(400)    NOT NULL,
                    Provider      NVARCHAR(100)    NOT NULL,
                    Status        NVARCHAR(50)     NOT NULL,
                    SentAtUtc     DATETIME2(3)     NOT NULL,
                    SentBy        UNIQUEIDENTIFIER NOT NULL,
                    IsDeleted     BIT              NOT NULL CONSTRAINT DF_EmailItems_IsDeleted DEFAULT (0),
                    CreatedUtc    DATETIME2(3)     NOT NULL CONSTRAINT DF_EmailItems_CreatedUtc DEFAULT (SYSUTCDATETIME()),
                    CreatedBy     UNIQUEIDENTIFIER NULL,
                    UpdatedUtc    DATETIME2(3)     NULL,
                    UpdatedBy     UNIQUEIDENTIFIER NULL
                );

                CREATE NONCLUSTERED INDEX [IX_EmailItems_TenantId_SiteId] ON [Integrations].[EmailItems] (TenantId, SiteId);
                CREATE NONCLUSTERED INDEX [IX_EmailItems_SentAtUtc] ON [Integrations].[EmailItems] (SentAtUtc);
            END");

        // Procedure: usp_Email_GetEmails.sql
        await conn.ExecuteAsync(@"
            CREATE OR ALTER PROCEDURE [Integrations].[usp_Email_GetEmails]
                @TenantId UNIQUEIDENTIFIER,
                @SiteId   UNIQUEIDENTIFIER = NULL,
                @FromUtc  DATETIME2(3),
                @ToUtc    DATETIME2(3)
            AS
            SET NOCOUNT ON;
            BEGIN
                BEGIN TRY
                    SELECT
                        EmailId,
                        TenantId,
                        SiteId,
                        Recipient,
                        Subject,
                        Provider,
                        Status,
                        SentAtUtc,
                        SentBy
                    FROM [Integrations].[EmailItems]
                    WHERE TenantId = @TenantId
                      AND (@SiteId IS NULL OR SiteId = @SiteId)
                      AND IsDeleted = 0
                      AND SentAtUtc >= @FromUtc
                      AND SentAtUtc <= @ToUtc
                    ORDER BY SentAtUtc DESC;
                END TRY
                BEGIN CATCH
                    DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'TenantId=', @TenantId, N'; SiteId=', @SiteId, N'; User=', SYSTEM_USER);
                    DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

                    INSERT INTO dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
                    VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

                    THROW;
                END CATCH
            END");

        // Table: Integrations\Sql\Tables\ExportItems.sql (read-only)
        await conn.ExecuteAsync(@"
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ExportItems' AND schema_id = SCHEMA_ID('Integrations'))
            BEGIN
                CREATE TABLE [Integrations].[ExportItems] (
                    ExportRunId    UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                    TenantId       UNIQUEIDENTIFIER NOT NULL,
                    SiteId         UNIQUEIDENTIFIER NULL,
                    TargetSystem   NVARCHAR(200)    NOT NULL,
                    ExportType     NVARCHAR(100)    NOT NULL,
                    RecordCount    INT              NOT NULL,
                    Status         NVARCHAR(50)     NOT NULL,
                    CompletedAtUtc DATETIME2(3)     NOT NULL,
                    InitiatedBy    UNIQUEIDENTIFIER NOT NULL,
                    IsDeleted      BIT              NOT NULL CONSTRAINT DF_ExportItems_IsDeleted DEFAULT (0),
                    CreatedUtc     DATETIME2(3)     NOT NULL CONSTRAINT DF_ExportItems_CreatedUtc DEFAULT (SYSUTCDATETIME()),
                    CreatedBy      UNIQUEIDENTIFIER NULL,
                    UpdatedUtc     DATETIME2(3)     NULL,
                    UpdatedBy      UNIQUEIDENTIFIER NULL
                );

                CREATE NONCLUSTERED INDEX [IX_ExportItems_TenantId_SiteId] ON [Integrations].[ExportItems] (TenantId, SiteId);
                CREATE NONCLUSTERED INDEX [IX_ExportItems_CompletedAtUtc] ON [Integrations].[ExportItems] (CompletedAtUtc);
            END");

        // Procedure: usp_Export_GetExportRuns.sql
        await conn.ExecuteAsync(@"
            CREATE OR ALTER PROCEDURE [Integrations].[usp_Export_GetExportRuns]
                @TenantId UNIQUEIDENTIFIER,
                @SiteId   UNIQUEIDENTIFIER = NULL,
                @FromUtc  DATETIME2(3),
                @ToUtc    DATETIME2(3)
            AS
            SET NOCOUNT ON;
            BEGIN
                BEGIN TRY
                    SELECT
                        ExportRunId,
                        TenantId,
                        SiteId,
                        TargetSystem,
                        ExportType,
                        RecordCount,
                        Status,
                        CompletedAtUtc,
                        InitiatedBy
                    FROM [Integrations].[ExportItems]
                    WHERE TenantId = @TenantId
                      AND (@SiteId IS NULL OR SiteId = @SiteId)
                      AND IsDeleted = 0
                      AND CompletedAtUtc >= @FromUtc
                      AND CompletedAtUtc <= @ToUtc
                    ORDER BY CompletedAtUtc DESC;
                END TRY
                BEGIN CATCH
                    DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'TenantId=', @TenantId, N'; SiteId=', @SiteId, N'; User=', SYSTEM_USER);
                    DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

                    INSERT INTO dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
                    VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

                    THROW;
                END CATCH
            END");

        // Table: Integrations\Sql\Tables\ImportItems.sql (read-only)
        await conn.ExecuteAsync(@"
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ImportItems' AND schema_id = SCHEMA_ID('Integrations'))
            BEGIN
                CREATE TABLE [Integrations].[ImportItems] (
                    ImportRunId      UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                    TenantId         UNIQUEIDENTIFIER NOT NULL,
                    SiteId           UNIQUEIDENTIFIER NULL,
                    SourceSystem     NVARCHAR(200)    NOT NULL,
                    ImportType       NVARCHAR(100)    NOT NULL,
                    RecordsProcessed INT              NOT NULL,
                    RecordsFailed    INT              NOT NULL,
                    Status           NVARCHAR(50)     NOT NULL,
                    CompletedAtUtc   DATETIME2(3)     NOT NULL,
                    InitiatedBy      UNIQUEIDENTIFIER NOT NULL,
                    IsDeleted        BIT              NOT NULL CONSTRAINT DF_ImportItems_IsDeleted DEFAULT (0),
                    CreatedUtc       DATETIME2(3)     NOT NULL CONSTRAINT DF_ImportItems_CreatedUtc DEFAULT (SYSUTCDATETIME()),
                    CreatedBy        UNIQUEIDENTIFIER NULL,
                    UpdatedUtc       DATETIME2(3)     NULL,
                    UpdatedBy        UNIQUEIDENTIFIER NULL
                );

                CREATE NONCLUSTERED INDEX [IX_ImportItems_TenantId_SiteId] ON [Integrations].[ImportItems] (TenantId, SiteId);
                CREATE NONCLUSTERED INDEX [IX_ImportItems_CompletedAtUtc] ON [Integrations].[ImportItems] (CompletedAtUtc);
            END");

        // Procedure: usp_Import_GetImportRuns.sql
        await conn.ExecuteAsync(@"
            CREATE OR ALTER PROCEDURE [Integrations].[usp_Import_GetImportRuns]
                @TenantId UNIQUEIDENTIFIER,
                @SiteId   UNIQUEIDENTIFIER = NULL,
                @FromUtc  DATETIME2(3),
                @ToUtc    DATETIME2(3)
            AS
            SET NOCOUNT ON;
            BEGIN
                BEGIN TRY
                    SELECT
                        ImportRunId,
                        TenantId,
                        SiteId,
                        SourceSystem,
                        ImportType,
                        RecordsProcessed,
                        RecordsFailed,
                        Status,
                        CompletedAtUtc,
                        InitiatedBy
                    FROM [Integrations].[ImportItems]
                    WHERE TenantId = @TenantId
                      AND (@SiteId IS NULL OR SiteId = @SiteId)
                      AND IsDeleted = 0
                      AND CompletedAtUtc >= @FromUtc
                      AND CompletedAtUtc <= @ToUtc
                    ORDER BY CompletedAtUtc DESC;
                END TRY
                BEGIN CATCH
                    DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'TenantId=', @TenantId, N'; SiteId=', @SiteId, N'; User=', SYSTEM_USER);
                    DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

                    INSERT INTO dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
                    VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

                    THROW;
                END CATCH
            END");

        // Table: Integrations\Sql\Tables\IoTItems.sql (read-only)
        await conn.ExecuteAsync(@"
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'IoTItems' AND schema_id = SCHEMA_ID('Integrations'))
            BEGIN
                CREATE TABLE [Integrations].[IoTItems] (
                    ReadingId      UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                    TenantId       UNIQUEIDENTIFIER NOT NULL,
                    SiteId         UNIQUEIDENTIFIER NULL,
                    DeviceId       UNIQUEIDENTIFIER NOT NULL,
                    SensorType     NVARCHAR(100)    NOT NULL,
                    ReadingValue   FLOAT            NOT NULL,
                    Status         NVARCHAR(50)     NOT NULL,
                    RecordedAtUtc  DATETIME2(3)     NOT NULL,
                    IngestedBy     UNIQUEIDENTIFIER NOT NULL,
                    IsDeleted      BIT              NOT NULL CONSTRAINT DF_IoTItems_IsDeleted DEFAULT (0),
                    CreatedUtc     DATETIME2(3)     NOT NULL CONSTRAINT DF_IoTItems_CreatedUtc DEFAULT (SYSUTCDATETIME()),
                    CreatedBy      UNIQUEIDENTIFIER NULL,
                    UpdatedUtc     DATETIME2(3)     NULL,
                    UpdatedBy      UNIQUEIDENTIFIER NULL
                );

                CREATE NONCLUSTERED INDEX [IX_IoTItems_TenantId_SiteId] ON [Integrations].[IoTItems] (TenantId, SiteId);
                CREATE NONCLUSTERED INDEX [IX_IoTItems_RecordedAtUtc] ON [Integrations].[IoTItems] (RecordedAtUtc);
                CREATE NONCLUSTERED INDEX [IX_IoTItems_DeviceId] ON [Integrations].[IoTItems] (DeviceId);
            END");

        // Procedure: usp_IoT_GetIoTReadings.sql
        await conn.ExecuteAsync(@"
            CREATE OR ALTER PROCEDURE [Integrations].[usp_IoT_GetIoTReadings]
                @TenantId UNIQUEIDENTIFIER,
                @SiteId   UNIQUEIDENTIFIER = NULL,
                @FromUtc  DATETIME2(3),
                @ToUtc    DATETIME2(3)
            AS
            SET NOCOUNT ON;
            BEGIN
                BEGIN TRY
                    SELECT
                        ReadingId,
                        TenantId,
                        SiteId,
                        DeviceId,
                        SensorType,
                        ReadingValue,
                        Status,
                        RecordedAtUtc,
                        IngestedBy
                    FROM [Integrations].[IoTItems]
                    WHERE TenantId = @TenantId
                      AND (@SiteId IS NULL OR SiteId = @SiteId)
                      AND IsDeleted = 0
                      AND RecordedAtUtc >= @FromUtc
                      AND RecordedAtUtc <= @ToUtc
                    ORDER BY RecordedAtUtc DESC;
                END TRY
                BEGIN CATCH
                    DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'TenantId=', @TenantId, N'; SiteId=', @SiteId, N'; User=', SYSTEM_USER);
                    DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

                    INSERT INTO dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
                    VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

                    THROW;
                END CATCH
            END");

        // Table: Integrations\Sql\Tables\NotificationsEmailItems.sql (read-only)
        await conn.ExecuteAsync(@"
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'NotificationsEmailItems' AND schema_id = SCHEMA_ID('Integrations'))
            BEGIN
                CREATE TABLE [Integrations].[NotificationsEmailItems] (
                    EmailDeliveryId   UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                    TenantId          UNIQUEIDENTIFIER NOT NULL,
                    SiteId            UNIQUEIDENTIFIER NULL,
                    RecipientAddress  NVARCHAR(320)    NOT NULL,
                    Subject           NVARCHAR(400)    NOT NULL,
                    TemplateName      NVARCHAR(200)    NULL,
                    Status            NVARCHAR(50)     NOT NULL,
                    SentAtUtc         DATETIME2(3)     NOT NULL,
                    SentBy            UNIQUEIDENTIFIER NOT NULL,
                    IsDeleted         BIT              NOT NULL CONSTRAINT DF_NotificationsEmailItems_IsDeleted DEFAULT (0),
                    CreatedUtc        DATETIME2(3)     NOT NULL CONSTRAINT DF_NotificationsEmailItems_CreatedUtc DEFAULT (SYSUTCDATETIME()),
                    CreatedBy         UNIQUEIDENTIFIER NULL,
                    UpdatedUtc        DATETIME2(3)     NULL,
                    UpdatedBy         UNIQUEIDENTIFIER NULL
                );

                CREATE NONCLUSTERED INDEX [IX_NotificationsEmailItems_TenantId_SiteId] ON [Integrations].[NotificationsEmailItems] (TenantId, SiteId);
                CREATE NONCLUSTERED INDEX [IX_NotificationsEmailItems_SentAtUtc] ON [Integrations].[NotificationsEmailItems] (SentAtUtc);
            END");

        // Procedure: usp_NotificationsEmail_GetEmailDeliveries.sql
        await conn.ExecuteAsync(@"
            CREATE OR ALTER PROCEDURE [Integrations].[usp_NotificationsEmail_GetEmailDeliveries]
                @TenantId UNIQUEIDENTIFIER,
                @SiteId   UNIQUEIDENTIFIER = NULL,
                @FromUtc  DATETIME2(3),
                @ToUtc    DATETIME2(3)
            AS
            SET NOCOUNT ON;
            BEGIN
                BEGIN TRY
                    SELECT
                        EmailDeliveryId,
                        TenantId,
                        SiteId,
                        RecipientAddress,
                        Subject,
                        TemplateName,
                        Status,
                        SentAtUtc,
                        SentBy
                    FROM [Integrations].[NotificationsEmailItems]
                    WHERE TenantId = @TenantId
                      AND (@SiteId IS NULL OR SiteId = @SiteId)
                      AND IsDeleted = 0
                      AND SentAtUtc >= @FromUtc
                      AND SentAtUtc <= @ToUtc
                    ORDER BY SentAtUtc DESC;
                END TRY
                BEGIN CATCH
                    DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'TenantId=', @TenantId, N'; SiteId=', @SiteId, N'; User=', SYSTEM_USER);
                    DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

                    INSERT INTO dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
                    VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

                    THROW;
                END CATCH
            END");

        // Table: Integrations\Sql\Tables\NotificationsInternalItems.sql (read-only)
        await conn.ExecuteAsync(@"
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'NotificationsInternalItems' AND schema_id = SCHEMA_ID('Integrations'))
            BEGIN
                CREATE TABLE [Integrations].[NotificationsInternalItems] (
                    NotificationId   UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                    TenantId         UNIQUEIDENTIFIER NOT NULL,
                    SiteId           UNIQUEIDENTIFIER NULL,
                    RecipientUserId  UNIQUEIDENTIFIER NOT NULL,
                    Message          NVARCHAR(MAX)    NOT NULL,
                    Category         NVARCHAR(100)    NULL,
                    Status           NVARCHAR(50)     NOT NULL,
                    DeliveredAtUtc   DATETIME2(3)     NOT NULL,
                    TriggeredBy      UNIQUEIDENTIFIER NOT NULL,
                    IsDeleted        BIT              NOT NULL CONSTRAINT DF_NotificationsInternalItems_IsDeleted DEFAULT (0),
                    CreatedUtc       DATETIME2(3)     NOT NULL CONSTRAINT DF_NotificationsInternalItems_CreatedUtc DEFAULT (SYSUTCDATETIME()),
                    CreatedBy        UNIQUEIDENTIFIER NULL,
                    UpdatedUtc       DATETIME2(3)     NULL,
                    UpdatedBy        UNIQUEIDENTIFIER NULL
                );

                CREATE NONCLUSTERED INDEX [IX_NotificationsInternalItems_TenantId_SiteId] ON [Integrations].[NotificationsInternalItems] (TenantId, SiteId);
                CREATE NONCLUSTERED INDEX [IX_NotificationsInternalItems_DeliveredAtUtc] ON [Integrations].[NotificationsInternalItems] (DeliveredAtUtc);
                CREATE NONCLUSTERED INDEX [IX_NotificationsInternalItems_RecipientUserId] ON [Integrations].[NotificationsInternalItems] (RecipientUserId);
            END");

        // Procedure: usp_NotificationsInternal_GetInternalNotifications.sql
        await conn.ExecuteAsync(@"
            CREATE OR ALTER PROCEDURE [Integrations].[usp_NotificationsInternal_GetInternalNotifications]
                @TenantId UNIQUEIDENTIFIER,
                @SiteId   UNIQUEIDENTIFIER = NULL,
                @FromUtc  DATETIME2(3),
                @ToUtc    DATETIME2(3)
            AS
            SET NOCOUNT ON;
            BEGIN
                BEGIN TRY
                    SELECT
                        NotificationId,
                        TenantId,
                        SiteId,
                        RecipientUserId,
                        Message,
                        Category,
                        Status,
                        DeliveredAtUtc,
                        TriggeredBy
                    FROM [Integrations].[NotificationsInternalItems]
                    WHERE TenantId = @TenantId
                      AND (@SiteId IS NULL OR SiteId = @SiteId)
                      AND IsDeleted = 0
                      AND DeliveredAtUtc >= @FromUtc
                      AND DeliveredAtUtc <= @ToUtc
                    ORDER BY DeliveredAtUtc DESC;
                END TRY
                BEGIN CATCH
                    DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'TenantId=', @TenantId, N'; SiteId=', @SiteId, N'; User=', SYSTEM_USER);
                    DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

                    INSERT INTO dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
                    VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

                    THROW;
                END CATCH
            END");

        // Table: Integrations\Sql\Tables\NotificationsTextingItems.sql (read-only)
        await conn.ExecuteAsync(@"
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'NotificationsTextingItems' AND schema_id = SCHEMA_ID('Integrations'))
            BEGIN
                CREATE TABLE [Integrations].[NotificationsTextingItems] (
                    SmsDeliveryId          UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                    TenantId               UNIQUEIDENTIFIER NOT NULL,
                    SiteId                 UNIQUEIDENTIFIER NULL,
                    RecipientPhoneNumber   NVARCHAR(30)     NOT NULL,
                    MessageBody            NVARCHAR(1000)   NOT NULL,
                    TriggerEvent           NVARCHAR(200)    NULL,
                    Status                 NVARCHAR(50)     NOT NULL,
                    SentAtUtc              DATETIME2(3)     NOT NULL,
                    SentBy                 UNIQUEIDENTIFIER NOT NULL,
                    IsDeleted              BIT              NOT NULL CONSTRAINT DF_NotificationsTextingItems_IsDeleted DEFAULT (0),
                    CreatedUtc             DATETIME2(3)     NOT NULL CONSTRAINT DF_NotificationsTextingItems_CreatedUtc DEFAULT (SYSUTCDATETIME()),
                    CreatedBy              UNIQUEIDENTIFIER NULL,
                    UpdatedUtc             DATETIME2(3)     NULL,
                    UpdatedBy              UNIQUEIDENTIFIER NULL
                );

                CREATE NONCLUSTERED INDEX [IX_NotificationsTextingItems_TenantId_SiteId] ON [Integrations].[NotificationsTextingItems] (TenantId, SiteId);
                CREATE NONCLUSTERED INDEX [IX_NotificationsTextingItems_SentAtUtc] ON [Integrations].[NotificationsTextingItems] (SentAtUtc);
            END");

        // Procedure: usp_NotificationsTexting_GetTextingDeliveries.sql
        await conn.ExecuteAsync(@"
            CREATE OR ALTER PROCEDURE [Integrations].[usp_NotificationsTexting_GetTextingDeliveries]
                @TenantId UNIQUEIDENTIFIER,
                @SiteId   UNIQUEIDENTIFIER = NULL,
                @FromUtc  DATETIME2(3),
                @ToUtc    DATETIME2(3)
            AS
            SET NOCOUNT ON;
            BEGIN
                BEGIN TRY
                    SELECT
                        SmsDeliveryId,
                        TenantId,
                        SiteId,
                        RecipientPhoneNumber,
                        MessageBody,
                        TriggerEvent,
                        Status,
                        SentAtUtc,
                        SentBy
                    FROM [Integrations].[NotificationsTextingItems]
                    WHERE TenantId = @TenantId
                      AND (@SiteId IS NULL OR SiteId = @SiteId)
                      AND IsDeleted = 0
                      AND SentAtUtc >= @FromUtc
                      AND SentAtUtc <= @ToUtc
                    ORDER BY SentAtUtc DESC;
                END TRY
                BEGIN CATCH
                    DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'TenantId=', @TenantId, N'; SiteId=', @SiteId, N'; User=', SYSTEM_USER);
                    DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

                    INSERT INTO dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
                    VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

                    THROW;
                END CATCH
            END");

        // Table: Integrations\Sql\Tables\ReportingItems.sql (read-only)
        await conn.ExecuteAsync(@"
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ReportingItems' AND schema_id = SCHEMA_ID('Integrations'))
            BEGIN
                CREATE TABLE [Integrations].[ReportingItems] (
                    DeliveryId       UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                    TenantId         UNIQUEIDENTIFIER NOT NULL,
                    SiteId           UNIQUEIDENTIFIER NULL,
                    ReportName       NVARCHAR(300)    NOT NULL,
                    RecipientSystem  NVARCHAR(200)    NOT NULL,
                    DeliveryChannel  NVARCHAR(50)     NOT NULL,
                    Status           NVARCHAR(50)     NOT NULL,
                    DeliveredAtUtc   DATETIME2(3)     NOT NULL,
                    InitiatedBy      UNIQUEIDENTIFIER NOT NULL,
                    IsDeleted        BIT              NOT NULL CONSTRAINT DF_ReportingItems_IsDeleted DEFAULT (0),
                    CreatedUtc       DATETIME2(3)     NOT NULL CONSTRAINT DF_ReportingItems_CreatedUtc DEFAULT (SYSUTCDATETIME()),
                    CreatedBy        UNIQUEIDENTIFIER NULL,
                    UpdatedUtc       DATETIME2(3)     NULL,
                    UpdatedBy        UNIQUEIDENTIFIER NULL
                );

                CREATE NONCLUSTERED INDEX [IX_ReportingItems_TenantId_SiteId] ON [Integrations].[ReportingItems] (TenantId, SiteId);
                CREATE NONCLUSTERED INDEX [IX_ReportingItems_DeliveredAtUtc] ON [Integrations].[ReportingItems] (DeliveredAtUtc);
            END");

        // Procedure: usp_Reporting_GetReportDeliveries.sql
        await conn.ExecuteAsync(@"
            CREATE OR ALTER PROCEDURE [Integrations].[usp_Reporting_GetReportDeliveries]
                @TenantId UNIQUEIDENTIFIER,
                @SiteId   UNIQUEIDENTIFIER = NULL,
                @FromUtc  DATETIME2(3),
                @ToUtc    DATETIME2(3)
            AS
            SET NOCOUNT ON;
            BEGIN
                BEGIN TRY
                    SELECT
                        DeliveryId,
                        TenantId,
                        SiteId,
                        ReportName,
                        RecipientSystem,
                        DeliveryChannel,
                        Status,
                        DeliveredAtUtc,
                        InitiatedBy
                    FROM [Integrations].[ReportingItems]
                    WHERE TenantId = @TenantId
                      AND (@SiteId IS NULL OR SiteId = @SiteId)
                      AND IsDeleted = 0
                      AND DeliveredAtUtc >= @FromUtc
                      AND DeliveredAtUtc <= @ToUtc
                    ORDER BY DeliveredAtUtc DESC;
                END TRY
                BEGIN CATCH
                    DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'TenantId=', @TenantId, N'; SiteId=', @SiteId, N'; User=', SYSTEM_USER);
                    DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

                    INSERT INTO dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
                    VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

                    THROW;
                END CATCH
            END");

        // Table: Integrations\Sql\Tables\TextingItems.sql (read-only, root-level Integrations.Texting)
        await conn.ExecuteAsync(@"
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'TextingItems' AND schema_id = SCHEMA_ID('Integrations'))
            BEGIN
                CREATE TABLE [Integrations].[TextingItems] (
                    MessageId             UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                    TenantId              UNIQUEIDENTIFIER NOT NULL,
                    SiteId                UNIQUEIDENTIFIER NULL,
                    RecipientPhoneNumber  NVARCHAR(30)     NOT NULL,
                    MessageTemplate       NVARCHAR(200)    NOT NULL,
                    GatewayProvider       NVARCHAR(100)    NOT NULL,
                    Status                NVARCHAR(50)     NOT NULL,
                    SentAtUtc             DATETIME2(3)     NOT NULL,
                    InitiatedBy           UNIQUEIDENTIFIER NOT NULL,
                    IsDeleted             BIT              NOT NULL CONSTRAINT DF_TextingItems_IsDeleted DEFAULT (0),
                    CreatedUtc            DATETIME2(3)     NOT NULL CONSTRAINT DF_TextingItems_CreatedUtc DEFAULT (SYSUTCDATETIME()),
                    CreatedBy             UNIQUEIDENTIFIER NULL,
                    UpdatedUtc            DATETIME2(3)     NULL,
                    UpdatedBy             UNIQUEIDENTIFIER NULL
                );

                CREATE NONCLUSTERED INDEX [IX_TextingItems_TenantId_SiteId] ON [Integrations].[TextingItems] (TenantId, SiteId);
                CREATE NONCLUSTERED INDEX [IX_TextingItems_SentAtUtc] ON [Integrations].[TextingItems] (SentAtUtc);
            END");

        // Procedure: usp_Texting_GetTextMessages.sql
        await conn.ExecuteAsync(@"
            CREATE OR ALTER PROCEDURE [Integrations].[usp_Texting_GetTextMessages]
                @TenantId UNIQUEIDENTIFIER,
                @SiteId   UNIQUEIDENTIFIER = NULL,
                @FromUtc  DATETIME2(3),
                @ToUtc    DATETIME2(3)
            AS
            SET NOCOUNT ON;
            BEGIN
                BEGIN TRY
                    SELECT
                        MessageId,
                        TenantId,
                        SiteId,
                        RecipientPhoneNumber,
                        MessageTemplate,
                        GatewayProvider,
                        Status,
                        SentAtUtc,
                        InitiatedBy
                    FROM [Integrations].[TextingItems]
                    WHERE TenantId = @TenantId
                      AND (@SiteId IS NULL OR SiteId = @SiteId)
                      AND IsDeleted = 0
                      AND SentAtUtc >= @FromUtc
                      AND SentAtUtc <= @ToUtc
                    ORDER BY SentAtUtc DESC;
                END TRY
                BEGIN CATCH
                    DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'TenantId=', @TenantId, N'; SiteId=', @SiteId, N'; User=', SYSTEM_USER);
                    DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

                    INSERT INTO dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
                    VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

                    THROW;
                END CATCH
            END");

        // Table: Integrations\Sql\Tables\InboundWebhookEvents.sql -- the ONLY real write path in
        // this module (idempotent check-and-insert keyed by (TenantId, EventId)). Includes RowVersion.
        await conn.ExecuteAsync(@"
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'InboundWebhookEvents' AND schema_id = SCHEMA_ID('Integrations'))
            BEGIN
                CREATE TABLE [Integrations].[InboundWebhookEvents] (
                    Id            UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_InboundWebhookEvents_Id DEFAULT (NEWID()) PRIMARY KEY,
                    TenantId      UNIQUEIDENTIFIER NOT NULL,
                    SiteId        UNIQUEIDENTIFIER NULL,
                    EventId       NVARCHAR(200)    NOT NULL,
                    EventType     NVARCHAR(200)    NOT NULL,
                    Source        NVARCHAR(200)    NOT NULL,
                    OccurredAtUtc DATETIMEOFFSET(3) NOT NULL,
                    RawPayload    NVARCHAR(MAX)    NOT NULL,
                    ReceivedAtUtc DATETIME2(3)     NOT NULL CONSTRAINT DF_InboundWebhookEvents_ReceivedAtUtc DEFAULT (SYSUTCDATETIME()),
                    IsDeleted     BIT              NOT NULL CONSTRAINT DF_InboundWebhookEvents_IsDeleted DEFAULT (0),
                    RowVersion    ROWVERSION       NOT NULL
                );

                CREATE UNIQUE NONCLUSTERED INDEX [UX_InboundWebhookEvents_TenantId_EventId]
                    ON [Integrations].[InboundWebhookEvents] (TenantId, EventId);

                CREATE NONCLUSTERED INDEX [IX_InboundWebhookEvents_ReceivedAtUtc] ON [Integrations].[InboundWebhookEvents] (ReceivedAtUtc);
            END");

        // Procedure: usp_InboundWebhookEvents_RecordEvent.sql (TRY/CATCH + AuditLog + ErrorLog,
        // ETag projected from RowVersion -- the module's one canonical write procedure)
        await conn.ExecuteAsync(@"
            CREATE OR ALTER PROCEDURE [Integrations].[usp_InboundWebhookEvents_RecordEvent]
                @TenantId      UNIQUEIDENTIFIER,
                @SiteId        UNIQUEIDENTIFIER = NULL,
                @EventId       NVARCHAR(200),
                @EventType     NVARCHAR(200),
                @Source        NVARCHAR(200),
                @OccurredAtUtc DATETIMEOFFSET(3),
                @RawPayload    NVARCHAR(MAX),
                @Accepted      BIT OUTPUT,
                @ETag          NVARCHAR(34) OUTPUT
            AS
            SET NOCOUNT ON;
            BEGIN TRY
                BEGIN TRANSACTION;

                    DECLARE @IsNewInsert BIT = 0;

                    IF EXISTS (
                        SELECT 1
                        FROM [Integrations].[InboundWebhookEvents] WITH (UPDLOCK, HOLDLOCK)
                        WHERE TenantId = @TenantId AND EventId = @EventId
                    )
                    BEGIN
                        SET @Accepted = 1;
                    END
                    ELSE
                    BEGIN
                        INSERT INTO [Integrations].[InboundWebhookEvents]
                            (TenantId, SiteId, EventId, EventType, Source, OccurredAtUtc, RawPayload, IsDeleted)
                        VALUES
                            (@TenantId, @SiteId, @EventId, @EventType, @Source, @OccurredAtUtc, @RawPayload, 0);

                        SET @Accepted = 1;
                        SET @IsNewInsert = 1;
                    END

                    SELECT @ETag = CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1)
                    FROM [Integrations].[InboundWebhookEvents]
                    WHERE TenantId = @TenantId AND EventId = @EventId;

                    BEGIN TRY
                        INSERT INTO dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
                        VALUES (
                            N'InboundWebhookEvents', DB_NAME(),
                            CASE WHEN @IsNewInsert = 1 THEN N'INSERT' ELSE N'IDEMPOTENT_ACCEPT' END,
                            SYSTEM_USER, SYSUTCDATETIME(), 0,
                            CONCAT(N'TenantId=', @TenantId, N'; EventId=', @EventId, N'; EventType=', @EventType),
                            0, OBJECT_NAME(@@PROCID));
                    END TRY
                    BEGIN CATCH
                    END CATCH;

                COMMIT TRANSACTION;
            END TRY
            BEGIN CATCH
                IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;

                DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'TenantId=', @TenantId, N'; EventId=', @EventId, N'; User=', SYSTEM_USER);
                DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

                BEGIN TRY
                    INSERT INTO dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
                    VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));
                END TRY
                BEGIN CATCH
                END CATCH;

                THROW;
            END CATCH");
    }

    /// <summary>
    /// Phase 7 §5.4 (Security &amp; Compliance Agent): creates the [Auditing].[PermissionUsageLog]
    /// table and its Insert/GetByTenant stored procedures (mirrors
    /// PlantSafety.Infrastructure\Auditing\ComplianceMonitoring\PermissionUsage\Sql\**\*.sql
    /// verbatim, minus the leading `USE` statement -- same convention as every other
    /// Apply*SchemaAsync method in this file). Backs AuthorizationBehavior's access-log capture
    /// and GetTenantAuditStreamQuery's tenant-scoped audit-stream read. Additive-only evolution.
    /// </summary>
    public static async Task ApplyComplianceMonitoringSchemaAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken);

        await conn.ExecuteAsync("""
            IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Auditing')
                EXEC('CREATE SCHEMA [Auditing]');
            """);

        await conn.ExecuteAsync("""
            IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES
                           WHERE TABLE_SCHEMA = 'Auditing' AND TABLE_NAME = 'PermissionUsageLog')
            BEGIN
                CREATE TABLE [Auditing].[PermissionUsageLog] (
                    [PermissionUsageLogId] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                    [TenantId]             UNIQUEIDENTIFIER NOT NULL,
                    [UserId]               UNIQUEIDENTIFIER NOT NULL,
                    [Permission]           NVARCHAR(256)    NOT NULL,
                    [RequestType]          NVARCHAR(256)    NOT NULL,
                    [Decision]             NVARCHAR(16)     NOT NULL,
                    [CorrelationId]        UNIQUEIDENTIFIER NOT NULL,
                    [OccurredAtUtc]        DATETIME2(3)     NOT NULL CONSTRAINT DF_PermissionUsageLog_OccurredAtUtc DEFAULT (SYSUTCDATETIME())
                );

                CREATE INDEX IX_PermissionUsageLog_TenantId_UserId_OccurredAtUtc
                    ON [Auditing].[PermissionUsageLog] ([TenantId], [UserId], [OccurredAtUtc]);
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [Auditing].[usp_PermissionUsageLog_Insert]
                @TenantId      UNIQUEIDENTIFIER,
                @UserId        UNIQUEIDENTIFIER,
                @Permission    NVARCHAR(256),
                @RequestType   NVARCHAR(256),
                @Decision      NVARCHAR(16),
                @CorrelationId UNIQUEIDENTIFIER,
                @OccurredAtUtc DATETIME2(3)
            AS
            BEGIN
                SET NOCOUNT ON;

                INSERT INTO [Auditing].[PermissionUsageLog]
                    ([TenantId], [UserId], [Permission], [RequestType], [Decision], [CorrelationId], [OccurredAtUtc])
                VALUES
                    (@TenantId, @UserId, @Permission, @RequestType, @Decision, @CorrelationId, @OccurredAtUtc);
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [Auditing].[usp_PermissionUsageLog_GetByTenant]
                @TenantId  UNIQUEIDENTIFIER,
                @FromUtc   DATETIME2(3),
                @ToUtc     DATETIME2(3)
            AS
            BEGIN
                SET NOCOUNT ON;

                SELECT
                    [TenantId],
                    [UserId],
                    [Permission],
                    [RequestType],
                    [Decision],
                    [CorrelationId],
                    [OccurredAtUtc]
                FROM [Auditing].[PermissionUsageLog]
                WHERE [TenantId] = @TenantId
                  AND [OccurredAtUtc] BETWEEN @FromUtc AND @ToUtc
                ORDER BY [OccurredAtUtc] DESC;
            END
            """);
    }

    /// <summary>
    /// Creates the <c>[HazardousWasteManagement]</c> schema's seven tables (WasteStreams,
    /// WasteContainers, AccumulationAreas, Manifests, Transporters, TsdfFacilities,
    /// DisposalCertificates) and their stored procedures against the Testcontainers database,
    /// mirroring <c>PlantSafety.Infrastructure\HazardousWasteManagement\Sql\Tables\*.sql</c> and
    /// <c>...\Sql\Procedures\usp_*.sql</c> verbatim. Backs the Dapper repositories under
    /// <c>PlantSafety.Infrastructure.HazardousWasteManagement.*.Persistence</c>.
    /// </summary>
    public static async Task ApplyHazardousWasteManagementSchemaAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken);

        // Schema
        await conn.ExecuteAsync("""
            IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'HazardousWasteManagement')
                EXEC('CREATE SCHEMA [HazardousWasteManagement]');
            """);

        // Table: WasteStreams.sql
        await conn.ExecuteAsync("""
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'WasteStreams' AND schema_id = SCHEMA_ID('HazardousWasteManagement'))
            BEGIN
                CREATE TABLE [HazardousWasteManagement].[WasteStreams] (
                    [WasteStreamId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                    [TenantId] UNIQUEIDENTIFIER NOT NULL,
                    [SiteId] UNIQUEIDENTIFIER NULL,
                    [Status] NVARCHAR(100) NOT NULL,
                    [StreamName] NVARCHAR(400) NOT NULL,
                    [EpaWasteCode] NVARCHAR(20) NOT NULL,
                    [HazardClass] NVARCHAR(100) NOT NULL,
                    [GeneratorEpaId] NVARCHAR(50) NOT NULL,
                    [CharacterizationVersion] INT NOT NULL CONSTRAINT DF_WasteStreams_CharacterizationVersion DEFAULT (1),
                    [CreatedAtUtc] DATETIME2(3) NOT NULL,
                    [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
                    [IsDeleted] BIT NOT NULL CONSTRAINT DF_WasteStreams_IsDeleted DEFAULT (0),
                    [ModifiedAtUtc] DATETIME2(3) NULL,
                    [ModifiedBy] UNIQUEIDENTIFIER NULL,
                    [CorrelationId] UNIQUEIDENTIFIER NULL,
                    [ActorId] UNIQUEIDENTIFIER NULL
                );

                CREATE NONCLUSTERED INDEX [IX_WasteStreams_TenantId_SiteId]
                    ON [HazardousWasteManagement].[WasteStreams] ([TenantId], [SiteId])
                    INCLUDE ([Status], [StreamName]);
            END
            """);

        // Table: WasteContainers.sql
        await conn.ExecuteAsync("""
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'WasteContainers' AND schema_id = SCHEMA_ID('HazardousWasteManagement'))
            BEGIN
                CREATE TABLE [HazardousWasteManagement].[WasteContainers] (
                    [WasteContainerId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                    [TenantId] UNIQUEIDENTIFIER NOT NULL,
                    [SiteId] UNIQUEIDENTIFIER NULL,
                    [Status] NVARCHAR(100) NOT NULL,
                    [AccumulationAreaId] UNIQUEIDENTIFIER NOT NULL,
                    [WasteStreamId] UNIQUEIDENTIFIER NOT NULL,
                    [ContainerLabel] NVARCHAR(200) NOT NULL,
                    [CapacityGallons] DECIMAL(12,2) NOT NULL,
                    [RegulatoryTimeLimitDays] INT NOT NULL CONSTRAINT DF_WasteContainers_RegulatoryTimeLimitDays DEFAULT (90),
                    [OpenedAtUtc] DATETIME2(3) NOT NULL,
                    [ClosedAtUtc] DATETIME2(3) NULL,
                    [CreatedAtUtc] DATETIME2(3) NOT NULL,
                    [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
                    [IsDeleted] BIT NOT NULL CONSTRAINT DF_WasteContainers_IsDeleted DEFAULT (0),
                    [ModifiedAtUtc] DATETIME2(3) NULL,
                    [ModifiedBy] UNIQUEIDENTIFIER NULL,
                    [CorrelationId] UNIQUEIDENTIFIER NULL,
                    [ActorId] UNIQUEIDENTIFIER NULL
                );

                CREATE NONCLUSTERED INDEX [IX_WasteContainers_AccumulationAreaId]
                    ON [HazardousWasteManagement].[WasteContainers] ([AccumulationAreaId])
                    INCLUDE ([Status], [OpenedAtUtc], [RegulatoryTimeLimitDays]);
            END
            """);

        // Table: AccumulationAreas.sql
        await conn.ExecuteAsync("""
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AccumulationAreas' AND schema_id = SCHEMA_ID('HazardousWasteManagement'))
            BEGIN
                CREATE TABLE [HazardousWasteManagement].[AccumulationAreas] (
                    [AccumulationAreaId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                    [TenantId] UNIQUEIDENTIFIER NOT NULL,
                    [SiteId] UNIQUEIDENTIFIER NULL,
                    [Status] NVARCHAR(100) NOT NULL,
                    [AreaName] NVARCHAR(400) NOT NULL,
                    [LocationDescription] NVARCHAR(1000) NOT NULL,
                    [TimeLimitDays] INT NOT NULL CONSTRAINT DF_AccumulationAreas_TimeLimitDays DEFAULT (90),
                    [CreatedAtUtc] DATETIME2(3) NOT NULL,
                    [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
                    [IsDeleted] BIT NOT NULL CONSTRAINT DF_AccumulationAreas_IsDeleted DEFAULT (0),
                    [ModifiedAtUtc] DATETIME2(3) NULL,
                    [ModifiedBy] UNIQUEIDENTIFIER NULL,
                    [CorrelationId] UNIQUEIDENTIFIER NULL,
                    [ActorId] UNIQUEIDENTIFIER NULL
                );

                CREATE NONCLUSTERED INDEX [IX_AccumulationAreas_TenantId_SiteId]
                    ON [HazardousWasteManagement].[AccumulationAreas] ([TenantId], [SiteId])
                    INCLUDE ([Status]);
            END
            """);

        // Table: Manifests.sql
        await conn.ExecuteAsync("""
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Manifests' AND schema_id = SCHEMA_ID('HazardousWasteManagement'))
            BEGIN
                CREATE TABLE [HazardousWasteManagement].[Manifests] (
                    [HazardousWasteManifestId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                    [TenantId] UNIQUEIDENTIFIER NOT NULL,
                    [SiteId] UNIQUEIDENTIFIER NULL,
                    [Status] NVARCHAR(100) NOT NULL,
                    [ManifestNumber] NVARCHAR(50) NOT NULL,
                    [GeneratorEpaId] NVARCHAR(50) NOT NULL,
                    [WasteStreamId] UNIQUEIDENTIFIER NOT NULL,
                    [TransporterId] UNIQUEIDENTIFIER NULL,
                    [TsdfFacilityId] UNIQUEIDENTIFIER NULL,
                    [GeneratedAtUtc] DATETIME2(3) NOT NULL,
                    [TransporterSignedAtUtc] DATETIME2(3) NULL,
                    [TsdfReceivedAtUtc] DATETIME2(3) NULL,
                    [ClosedAtUtc] DATETIME2(3) NULL,
                    [IsImmutable] BIT NOT NULL CONSTRAINT DF_Manifests_IsImmutable DEFAULT (0),
                    [DiscrepancyReason] NVARCHAR(2000) NULL,
                    [RetentionExpiresAtUtc] DATETIME2(3) NOT NULL,
                    [CreatedAtUtc] DATETIME2(3) NOT NULL,
                    [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
                    [IsDeleted] BIT NOT NULL CONSTRAINT DF_Manifests_IsDeleted DEFAULT (0),
                    [ModifiedAtUtc] DATETIME2(3) NULL,
                    [ModifiedBy] UNIQUEIDENTIFIER NULL,
                    [CorrelationId] UNIQUEIDENTIFIER NULL,
                    [ActorId] UNIQUEIDENTIFIER NULL
                );

                CREATE UNIQUE NONCLUSTERED INDEX [UX_Manifests_ManifestNumber]
                    ON [HazardousWasteManagement].[Manifests] ([TenantId], [ManifestNumber]);

                CREATE NONCLUSTERED INDEX [IX_Manifests_TenantId_Status]
                    ON [HazardousWasteManagement].[Manifests] ([TenantId], [Status])
                    INCLUDE ([SiteId], [GeneratedAtUtc]);
            END
            """);

        // Table: Transporters.sql
        await conn.ExecuteAsync("""
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Transporters' AND schema_id = SCHEMA_ID('HazardousWasteManagement'))
            BEGIN
                CREATE TABLE [HazardousWasteManagement].[Transporters] (
                    [TransporterId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                    [TenantId] UNIQUEIDENTIFIER NOT NULL,
                    [SiteId] UNIQUEIDENTIFIER NULL,
                    [Status] NVARCHAR(100) NOT NULL,
                    [CompanyName] NVARCHAR(400) NOT NULL,
                    [EpaTransporterId] NVARCHAR(50) NOT NULL,
                    [LicenseExpiresAtUtc] DATETIME2(3) NULL,
                    [CreatedAtUtc] DATETIME2(3) NOT NULL,
                    [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
                    [IsDeleted] BIT NOT NULL CONSTRAINT DF_Transporters_IsDeleted DEFAULT (0),
                    [ModifiedAtUtc] DATETIME2(3) NULL,
                    [ModifiedBy] UNIQUEIDENTIFIER NULL,
                    [CorrelationId] UNIQUEIDENTIFIER NULL,
                    [ActorId] UNIQUEIDENTIFIER NULL
                );

                CREATE NONCLUSTERED INDEX [IX_Transporters_TenantId]
                    ON [HazardousWasteManagement].[Transporters] ([TenantId])
                    INCLUDE ([Status]);
            END
            """);

        // Table: TsdfFacilities.sql
        await conn.ExecuteAsync("""
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'TsdfFacilities' AND schema_id = SCHEMA_ID('HazardousWasteManagement'))
            BEGIN
                CREATE TABLE [HazardousWasteManagement].[TsdfFacilities] (
                    [TsdfFacilityId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                    [TenantId] UNIQUEIDENTIFIER NOT NULL,
                    [SiteId] UNIQUEIDENTIFIER NULL,
                    [Status] NVARCHAR(100) NOT NULL,
                    [FacilityName] NVARCHAR(400) NOT NULL,
                    [EpaFacilityId] NVARCHAR(50) NOT NULL,
                    [PermitExpiresAtUtc] DATETIME2(3) NULL,
                    [CreatedAtUtc] DATETIME2(3) NOT NULL,
                    [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
                    [IsDeleted] BIT NOT NULL CONSTRAINT DF_TsdfFacilities_IsDeleted DEFAULT (0),
                    [ModifiedAtUtc] DATETIME2(3) NULL,
                    [ModifiedBy] UNIQUEIDENTIFIER NULL,
                    [CorrelationId] UNIQUEIDENTIFIER NULL,
                    [ActorId] UNIQUEIDENTIFIER NULL
                );

                CREATE NONCLUSTERED INDEX [IX_TsdfFacilities_TenantId]
                    ON [HazardousWasteManagement].[TsdfFacilities] ([TenantId])
                    INCLUDE ([Status]);
            END
            """);

        // Table: DisposalCertificates.sql
        await conn.ExecuteAsync("""
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'DisposalCertificates' AND schema_id = SCHEMA_ID('HazardousWasteManagement'))
            BEGIN
                CREATE TABLE [HazardousWasteManagement].[DisposalCertificates] (
                    [DisposalCertificateId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                    [TenantId] UNIQUEIDENTIFIER NOT NULL,
                    [SiteId] UNIQUEIDENTIFIER NULL,
                    [Status] NVARCHAR(100) NOT NULL,
                    [ManifestId] UNIQUEIDENTIFIER NOT NULL,
                    [TsdfFacilityId] UNIQUEIDENTIFIER NOT NULL,
                    [DisposalMethod] NVARCHAR(100) NOT NULL,
                    [DisposalDateUtc] DATETIME2(3) NOT NULL,
                    [CertificateReceivedAtUtc] DATETIME2(3) NOT NULL,
                    [CreatedAtUtc] DATETIME2(3) NOT NULL,
                    [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
                    [IsDeleted] BIT NOT NULL CONSTRAINT DF_DisposalCertificates_IsDeleted DEFAULT (0),
                    [ModifiedAtUtc] DATETIME2(3) NULL,
                    [ModifiedBy] UNIQUEIDENTIFIER NULL,
                    [CorrelationId] UNIQUEIDENTIFIER NULL,
                    [ActorId] UNIQUEIDENTIFIER NULL
                );

                CREATE NONCLUSTERED INDEX [IX_DisposalCertificates_ManifestId]
                    ON [HazardousWasteManagement].[DisposalCertificates] ([ManifestId]);
            END
            """);

        // Procedure: usp_WasteStreams_Create.sql
        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [HazardousWasteManagement].[usp_WasteStreams_Create]
                @CorrelationId UNIQUEIDENTIFIER = NULL,
                @ActorId       UNIQUEIDENTIFIER = NULL,
                @WasteStreamId UNIQUEIDENTIFIER,
                @TenantId UNIQUEIDENTIFIER,
                @SiteId UNIQUEIDENTIFIER = NULL,
                @Status NVARCHAR(100),
                @StreamName NVARCHAR(400),
                @EpaWasteCode NVARCHAR(20),
                @HazardClass NVARCHAR(100),
                @GeneratorEpaId NVARCHAR(50),
                @CharacterizationVersion INT,
                @CreatedAtUtc DATETIME2(3),
                @CreatedBy UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN TRY
                BEGIN TRANSACTION;
                    INSERT INTO [HazardousWasteManagement].[WasteStreams]
                        ([WasteStreamId], [TenantId], [SiteId], [Status], [StreamName], [EpaWasteCode], [HazardClass], [GeneratorEpaId], [CharacterizationVersion], [CreatedAtUtc], [CreatedBy], [CorrelationId], [ActorId])
                    VALUES
                        (@WasteStreamId, @TenantId, @SiteId, @Status, @StreamName, @EpaWasteCode, @HazardClass, @GeneratorEpaId, @CharacterizationVersion, @CreatedAtUtc, @CreatedBy, @CorrelationId, @ActorId);
                COMMIT TRANSACTION;
            END TRY
            BEGIN CATCH
                IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
                THROW;
            END CATCH;
            """);

        // Procedure: usp_WasteStreams_GetAll.sql
        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [HazardousWasteManagement].[usp_WasteStreams_GetAll]
            AS
            BEGIN
                SET NOCOUNT ON;

                SELECT [WasteStreamId], [TenantId], [SiteId], [Status], [StreamName], [EpaWasteCode], [HazardClass], [GeneratorEpaId], [CharacterizationVersion], [CreatedAtUtc], [CreatedBy]
                FROM [HazardousWasteManagement].[WasteStreams]
                WHERE ISNULL([IsDeleted], 0) = 0;
            END;
            """);

        // Procedure: usp_WasteStreams_GetById.sql
        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [HazardousWasteManagement].[usp_WasteStreams_GetById]
            (
                @WasteStreamId UNIQUEIDENTIFIER
            )
            AS
            BEGIN
                SET NOCOUNT ON;

                SELECT [WasteStreamId], [TenantId], [SiteId], [Status], [StreamName], [EpaWasteCode], [HazardClass], [GeneratorEpaId], [CharacterizationVersion], [CreatedAtUtc], [CreatedBy]
                FROM [HazardousWasteManagement].[WasteStreams]
                WHERE [WasteStreamId] = @WasteStreamId
                  AND ISNULL([IsDeleted], 0) = 0;
            END;
            """);

        // Procedure: usp_WasteStreams_Update.sql
        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [HazardousWasteManagement].[usp_WasteStreams_Update]
                @CorrelationId UNIQUEIDENTIFIER = NULL,
                @ActorId       UNIQUEIDENTIFIER = NULL,
                @NowUtc        DATETIME2(3),
                @WasteStreamId UNIQUEIDENTIFIER,
                @TenantId UNIQUEIDENTIFIER,
                @Status NVARCHAR(100),
                @EpaWasteCode NVARCHAR(20),
                @HazardClass NVARCHAR(100),
                @CharacterizationVersion INT
            AS
            SET NOCOUNT ON;
            BEGIN TRY
                BEGIN TRANSACTION;
                    UPDATE [HazardousWasteManagement].[WasteStreams]
                    SET
                        [Status] = @Status,
                        [EpaWasteCode] = @EpaWasteCode,
                        [HazardClass] = @HazardClass,
                        [CharacterizationVersion] = @CharacterizationVersion,
                        [ModifiedAtUtc] = @NowUtc,
                        [ModifiedBy] = @ActorId,
                        [CorrelationId] = @CorrelationId
                    WHERE [WasteStreamId] = @WasteStreamId
                      AND [TenantId] = @TenantId
                      AND ISNULL([IsDeleted], 0) = 0;
                COMMIT TRANSACTION;
            END TRY
            BEGIN CATCH
                IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
                THROW;
            END CATCH;
            """);

        // Procedure: usp_WasteContainers_Create.sql
        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [HazardousWasteManagement].[usp_WasteContainers_Create]
                @CorrelationId UNIQUEIDENTIFIER = NULL,
                @ActorId       UNIQUEIDENTIFIER = NULL,
                @WasteContainerId UNIQUEIDENTIFIER,
                @TenantId UNIQUEIDENTIFIER,
                @SiteId UNIQUEIDENTIFIER = NULL,
                @Status NVARCHAR(100),
                @AccumulationAreaId UNIQUEIDENTIFIER,
                @WasteStreamId UNIQUEIDENTIFIER,
                @ContainerLabel NVARCHAR(200),
                @CapacityGallons DECIMAL(12,2),
                @RegulatoryTimeLimitDays INT,
                @OpenedAtUtc DATETIME2(3),
                @CreatedAtUtc DATETIME2(3),
                @CreatedBy UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN TRY
                BEGIN TRANSACTION;
                    INSERT INTO [HazardousWasteManagement].[WasteContainers]
                        ([WasteContainerId], [TenantId], [SiteId], [Status], [AccumulationAreaId], [WasteStreamId], [ContainerLabel], [CapacityGallons], [RegulatoryTimeLimitDays], [OpenedAtUtc], [CreatedAtUtc], [CreatedBy], [CorrelationId], [ActorId])
                    VALUES
                        (@WasteContainerId, @TenantId, @SiteId, @Status, @AccumulationAreaId, @WasteStreamId, @ContainerLabel, @CapacityGallons, @RegulatoryTimeLimitDays, @OpenedAtUtc, @CreatedAtUtc, @CreatedBy, @CorrelationId, @ActorId);
                COMMIT TRANSACTION;
            END TRY
            BEGIN CATCH
                IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
                THROW;
            END CATCH;
            """);

        // Procedure: usp_WasteContainers_GetAll.sql
        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [HazardousWasteManagement].[usp_WasteContainers_GetAll]
            AS
            BEGIN
                SET NOCOUNT ON;

                SELECT [WasteContainerId], [TenantId], [SiteId], [Status], [AccumulationAreaId], [WasteStreamId], [ContainerLabel], [CapacityGallons], [RegulatoryTimeLimitDays], [OpenedAtUtc], [ClosedAtUtc], [CreatedAtUtc], [CreatedBy]
                FROM [HazardousWasteManagement].[WasteContainers]
                WHERE ISNULL([IsDeleted], 0) = 0;
            END;
            """);

        // Procedure: usp_WasteContainers_GetById.sql
        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [HazardousWasteManagement].[usp_WasteContainers_GetById]
            (
                @WasteContainerId UNIQUEIDENTIFIER
            )
            AS
            BEGIN
                SET NOCOUNT ON;

                SELECT [WasteContainerId], [TenantId], [SiteId], [Status], [AccumulationAreaId], [WasteStreamId], [ContainerLabel], [CapacityGallons], [RegulatoryTimeLimitDays], [OpenedAtUtc], [ClosedAtUtc], [CreatedAtUtc], [CreatedBy]
                FROM [HazardousWasteManagement].[WasteContainers]
                WHERE [WasteContainerId] = @WasteContainerId
                  AND ISNULL([IsDeleted], 0) = 0;
            END;
            """);

        // Procedure: usp_WasteContainers_ListByArea.sql
        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [HazardousWasteManagement].[usp_WasteContainers_ListByArea]
                @TenantId UNIQUEIDENTIFIER,
                @AccumulationAreaId UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT
                    [WasteContainerId], [TenantId], [SiteId], [AccumulationAreaId], [WasteStreamId],
                    [ContainerLabel], [CapacityGallons], [RegulatoryTimeLimitDays], [OpenedAtUtc], [ClosedAtUtc], [Status]
                FROM [HazardousWasteManagement].[WasteContainers]
                WHERE TenantId = @TenantId
                  AND AccumulationAreaId = @AccumulationAreaId
                  AND ISNULL([IsDeleted], 0) = 0
                ORDER BY OpenedAtUtc DESC;
            END
            """);

        // Procedure: usp_WasteContainers_Update.sql
        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [HazardousWasteManagement].[usp_WasteContainers_Update]
                @CorrelationId UNIQUEIDENTIFIER = NULL,
                @ActorId       UNIQUEIDENTIFIER = NULL,
                @NowUtc        DATETIME2(3),
                @WasteContainerId UNIQUEIDENTIFIER,
                @TenantId UNIQUEIDENTIFIER,
                @Status NVARCHAR(100),
                @ClosedAtUtc DATETIME2(3) = NULL
            AS
            SET NOCOUNT ON;
            BEGIN TRY
                BEGIN TRANSACTION;
                    UPDATE [HazardousWasteManagement].[WasteContainers]
                    SET
                        [Status] = @Status,
                        [ClosedAtUtc] = @ClosedAtUtc,
                        [ModifiedAtUtc] = @NowUtc,
                        [ModifiedBy] = @ActorId,
                        [CorrelationId] = @CorrelationId
                    WHERE [WasteContainerId] = @WasteContainerId
                      AND [TenantId] = @TenantId
                      AND ISNULL([IsDeleted], 0) = 0;
                COMMIT TRANSACTION;
            END TRY
            BEGIN CATCH
                IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
                THROW;
            END CATCH;
            """);

        // Procedure: usp_AccumulationAreas_Create.sql
        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [HazardousWasteManagement].[usp_AccumulationAreas_Create]
                @CorrelationId UNIQUEIDENTIFIER = NULL,
                @ActorId       UNIQUEIDENTIFIER = NULL,
                @AccumulationAreaId UNIQUEIDENTIFIER,
                @TenantId UNIQUEIDENTIFIER,
                @SiteId UNIQUEIDENTIFIER = NULL,
                @Status NVARCHAR(100),
                @AreaName NVARCHAR(400),
                @LocationDescription NVARCHAR(1000),
                @TimeLimitDays INT,
                @CreatedAtUtc DATETIME2(3),
                @CreatedBy UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN TRY
                BEGIN TRANSACTION;
                    INSERT INTO [HazardousWasteManagement].[AccumulationAreas]
                        ([AccumulationAreaId], [TenantId], [SiteId], [Status], [AreaName], [LocationDescription], [TimeLimitDays], [CreatedAtUtc], [CreatedBy], [CorrelationId], [ActorId])
                    VALUES
                        (@AccumulationAreaId, @TenantId, @SiteId, @Status, @AreaName, @LocationDescription, @TimeLimitDays, @CreatedAtUtc, @CreatedBy, @CorrelationId, @ActorId);
                COMMIT TRANSACTION;
            END TRY
            BEGIN CATCH
                IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
                THROW;
            END CATCH;
            """);

        // Procedure: usp_AccumulationAreas_GetAll.sql
        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [HazardousWasteManagement].[usp_AccumulationAreas_GetAll]
            AS
            BEGIN
                SET NOCOUNT ON;

                SELECT [AccumulationAreaId], [TenantId], [SiteId], [Status], [AreaName], [LocationDescription], [TimeLimitDays], [CreatedAtUtc], [CreatedBy]
                FROM [HazardousWasteManagement].[AccumulationAreas]
                WHERE ISNULL([IsDeleted], 0) = 0;
            END;
            """);

        // Procedure: usp_AccumulationAreas_GetById.sql
        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [HazardousWasteManagement].[usp_AccumulationAreas_GetById]
            (
                @AccumulationAreaId UNIQUEIDENTIFIER
            )
            AS
            BEGIN
                SET NOCOUNT ON;

                SELECT [AccumulationAreaId], [TenantId], [SiteId], [Status], [AreaName], [LocationDescription], [TimeLimitDays], [CreatedAtUtc], [CreatedBy]
                FROM [HazardousWasteManagement].[AccumulationAreas]
                WHERE [AccumulationAreaId] = @AccumulationAreaId
                  AND ISNULL([IsDeleted], 0) = 0;
            END;
            """);

        // Procedure: usp_AccumulationAreas_ListNearingTimeLimit.sql
        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [HazardousWasteManagement].[usp_AccumulationAreas_ListNearingTimeLimit]
                @TenantId UNIQUEIDENTIFIER,
                @SiteId   UNIQUEIDENTIFIER = NULL
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT
                    a.[AccumulationAreaId], a.[TenantId], a.[SiteId], a.[AreaName], a.[LocationDescription], a.[TimeLimitDays], a.[Status],
                    MIN(DATEADD(DAY, c.[RegulatoryTimeLimitDays], c.[OpenedAtUtc])) AS [EarliestContainerDeadlineUtc]
                FROM [HazardousWasteManagement].[AccumulationAreas] a
                INNER JOIN [HazardousWasteManagement].[WasteContainers] c
                    ON c.[AccumulationAreaId] = a.[AccumulationAreaId]
                   AND c.[Status] = 'Open'
                   AND ISNULL(c.[IsDeleted], 0) = 0
                WHERE a.TenantId = @TenantId
                  AND (@SiteId IS NULL OR a.SiteId = @SiteId)
                  AND ISNULL(a.[IsDeleted], 0) = 0
                  AND DATEADD(DAY, c.[RegulatoryTimeLimitDays], c.[OpenedAtUtc]) <= DATEADD(DAY, 10, SYSUTCDATETIME())
                GROUP BY a.[AccumulationAreaId], a.[TenantId], a.[SiteId], a.[AreaName], a.[LocationDescription], a.[TimeLimitDays], a.[Status]
                ORDER BY [EarliestContainerDeadlineUtc] ASC;
            END
            """);

        // Procedure: usp_AccumulationAreas_Update.sql
        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [HazardousWasteManagement].[usp_AccumulationAreas_Update]
                @CorrelationId UNIQUEIDENTIFIER = NULL,
                @ActorId       UNIQUEIDENTIFIER = NULL,
                @NowUtc        DATETIME2(3),
                @AccumulationAreaId UNIQUEIDENTIFIER,
                @TenantId UNIQUEIDENTIFIER,
                @Status NVARCHAR(100)
            AS
            SET NOCOUNT ON;
            BEGIN TRY
                BEGIN TRANSACTION;
                    UPDATE [HazardousWasteManagement].[AccumulationAreas]
                    SET
                        [Status] = @Status,
                        [ModifiedAtUtc] = @NowUtc,
                        [ModifiedBy] = @ActorId,
                        [CorrelationId] = @CorrelationId
                    WHERE [AccumulationAreaId] = @AccumulationAreaId
                      AND [TenantId] = @TenantId
                      AND ISNULL([IsDeleted], 0) = 0;
                COMMIT TRANSACTION;
            END TRY
            BEGIN CATCH
                IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
                THROW;
            END CATCH;
            """);

        // Procedure: usp_Manifests_Create.sql
        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [HazardousWasteManagement].[usp_Manifests_Create]
                @CorrelationId UNIQUEIDENTIFIER = NULL,
                @ActorId       UNIQUEIDENTIFIER = NULL,
                @HazardousWasteManifestId UNIQUEIDENTIFIER,
                @TenantId UNIQUEIDENTIFIER,
                @SiteId UNIQUEIDENTIFIER = NULL,
                @Status NVARCHAR(100),
                @ManifestNumber NVARCHAR(50),
                @GeneratorEpaId NVARCHAR(50),
                @WasteStreamId UNIQUEIDENTIFIER,
                @GeneratedAtUtc DATETIME2(3),
                @IsImmutable BIT,
                @RetentionExpiresAtUtc DATETIME2(3),
                @CreatedAtUtc DATETIME2(3),
                @CreatedBy UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN TRY
                BEGIN TRANSACTION;
                    INSERT INTO [HazardousWasteManagement].[Manifests]
                        ([HazardousWasteManifestId], [TenantId], [SiteId], [Status], [ManifestNumber], [GeneratorEpaId], [WasteStreamId], [GeneratedAtUtc], [IsImmutable], [RetentionExpiresAtUtc], [CreatedAtUtc], [CreatedBy], [CorrelationId], [ActorId])
                    VALUES
                        (@HazardousWasteManifestId, @TenantId, @SiteId, @Status, @ManifestNumber, @GeneratorEpaId, @WasteStreamId, @GeneratedAtUtc, @IsImmutable, @RetentionExpiresAtUtc, @CreatedAtUtc, @CreatedBy, @CorrelationId, @ActorId);
                COMMIT TRANSACTION;
            END TRY
            BEGIN CATCH
                IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
                THROW;
            END CATCH;
            """);

        // Procedure: usp_Manifests_GetAll.sql
        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [HazardousWasteManagement].[usp_Manifests_GetAll]
            AS
            BEGIN
                SET NOCOUNT ON;

                SELECT [HazardousWasteManifestId], [TenantId], [SiteId], [Status], [ManifestNumber], [GeneratorEpaId], [WasteStreamId], [TransporterId], [TsdfFacilityId],
                       [GeneratedAtUtc], [TransporterSignedAtUtc], [TsdfReceivedAtUtc], [ClosedAtUtc], [IsImmutable], [DiscrepancyReason], [RetentionExpiresAtUtc], [CreatedAtUtc], [CreatedBy]
                FROM [HazardousWasteManagement].[Manifests]
                WHERE ISNULL([IsDeleted], 0) = 0;
            END;
            """);

        // Procedure: usp_Manifests_GetById.sql
        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [HazardousWasteManagement].[usp_Manifests_GetById]
            (
                @HazardousWasteManifestId UNIQUEIDENTIFIER
            )
            AS
            BEGIN
                SET NOCOUNT ON;

                SELECT [HazardousWasteManifestId], [TenantId], [SiteId], [Status], [ManifestNumber], [GeneratorEpaId], [WasteStreamId], [TransporterId], [TsdfFacilityId],
                       [GeneratedAtUtc], [TransporterSignedAtUtc], [TsdfReceivedAtUtc], [ClosedAtUtc], [IsImmutable], [DiscrepancyReason], [RetentionExpiresAtUtc], [CreatedAtUtc], [CreatedBy]
                FROM [HazardousWasteManagement].[Manifests]
                WHERE [HazardousWasteManifestId] = @HazardousWasteManifestId
                  AND ISNULL([IsDeleted], 0) = 0;
            END;
            """);

        // Procedure: usp_Manifests_ListOpen.sql
        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [HazardousWasteManagement].[usp_Manifests_ListOpen]
                @TenantId UNIQUEIDENTIFIER,
                @SiteId   UNIQUEIDENTIFIER = NULL
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT [HazardousWasteManifestId], [TenantId], [SiteId], [Status], [ManifestNumber], [GeneratorEpaId], [WasteStreamId], [TransporterId], [TsdfFacilityId],
                       [GeneratedAtUtc], [TransporterSignedAtUtc], [TsdfReceivedAtUtc], [ClosedAtUtc], [IsImmutable], [DiscrepancyReason], [RetentionExpiresAtUtc]
                FROM [HazardousWasteManagement].[Manifests]
                WHERE TenantId = @TenantId
                  AND (@SiteId IS NULL OR SiteId = @SiteId)
                  AND [Status] <> 'Closed'
                  AND ISNULL([IsDeleted], 0) = 0
                ORDER BY [GeneratedAtUtc] ASC;
            END
            """);

        // Procedure: usp_Manifests_Update.sql
        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [HazardousWasteManagement].[usp_Manifests_Update]
                @CorrelationId UNIQUEIDENTIFIER = NULL,
                @ActorId       UNIQUEIDENTIFIER = NULL,
                @NowUtc        DATETIME2(3),
                @HazardousWasteManifestId UNIQUEIDENTIFIER,
                @TenantId UNIQUEIDENTIFIER,
                @Status NVARCHAR(100),
                @TransporterId UNIQUEIDENTIFIER = NULL,
                @TsdfFacilityId UNIQUEIDENTIFIER = NULL,
                @TransporterSignedAtUtc DATETIME2(3) = NULL,
                @TsdfReceivedAtUtc DATETIME2(3) = NULL,
                @ClosedAtUtc DATETIME2(3) = NULL,
                @IsImmutable BIT,
                @DiscrepancyReason NVARCHAR(2000) = NULL
            AS
            SET NOCOUNT ON;
            BEGIN TRY
                BEGIN TRANSACTION;
                    UPDATE [HazardousWasteManagement].[Manifests]
                    SET
                        [Status] = @Status,
                        [TransporterId] = @TransporterId,
                        [TsdfFacilityId] = @TsdfFacilityId,
                        [TransporterSignedAtUtc] = @TransporterSignedAtUtc,
                        [TsdfReceivedAtUtc] = @TsdfReceivedAtUtc,
                        [ClosedAtUtc] = @ClosedAtUtc,
                        [IsImmutable] = @IsImmutable,
                        [DiscrepancyReason] = @DiscrepancyReason,
                        [ModifiedAtUtc] = @NowUtc,
                        [ModifiedBy] = @ActorId,
                        [CorrelationId] = @CorrelationId
                    WHERE [HazardousWasteManifestId] = @HazardousWasteManifestId
                      AND [TenantId] = @TenantId
                      AND ISNULL([IsDeleted], 0) = 0;
                COMMIT TRANSACTION;
            END TRY
            BEGIN CATCH
                IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
                THROW;
            END CATCH;
            """);

        // Procedure: usp_Transporters_Create.sql
        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [HazardousWasteManagement].[usp_Transporters_Create]
                @CorrelationId UNIQUEIDENTIFIER = NULL,
                @ActorId       UNIQUEIDENTIFIER = NULL,
                @TransporterId UNIQUEIDENTIFIER,
                @TenantId UNIQUEIDENTIFIER,
                @SiteId UNIQUEIDENTIFIER = NULL,
                @Status NVARCHAR(100),
                @CompanyName NVARCHAR(400),
                @EpaTransporterId NVARCHAR(50),
                @LicenseExpiresAtUtc DATETIME2(3) = NULL,
                @CreatedAtUtc DATETIME2(3),
                @CreatedBy UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN TRY
                BEGIN TRANSACTION;
                    INSERT INTO [HazardousWasteManagement].[Transporters]
                        ([TransporterId], [TenantId], [SiteId], [Status], [CompanyName], [EpaTransporterId], [LicenseExpiresAtUtc], [CreatedAtUtc], [CreatedBy], [CorrelationId], [ActorId])
                    VALUES
                        (@TransporterId, @TenantId, @SiteId, @Status, @CompanyName, @EpaTransporterId, @LicenseExpiresAtUtc, @CreatedAtUtc, @CreatedBy, @CorrelationId, @ActorId);
                COMMIT TRANSACTION;
            END TRY
            BEGIN CATCH
                IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
                THROW;
            END CATCH;
            """);

        // Procedure: usp_Transporters_GetAll.sql
        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [HazardousWasteManagement].[usp_Transporters_GetAll]
            AS
            BEGIN
                SET NOCOUNT ON;

                SELECT [TransporterId], [TenantId], [SiteId], [Status], [CompanyName], [EpaTransporterId], [LicenseExpiresAtUtc], [CreatedAtUtc], [CreatedBy]
                FROM [HazardousWasteManagement].[Transporters]
                WHERE ISNULL([IsDeleted], 0) = 0;
            END;
            """);

        // Procedure: usp_Transporters_GetById.sql
        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [HazardousWasteManagement].[usp_Transporters_GetById]
            (
                @TransporterId UNIQUEIDENTIFIER
            )
            AS
            BEGIN
                SET NOCOUNT ON;

                SELECT [TransporterId], [TenantId], [SiteId], [Status], [CompanyName], [EpaTransporterId], [LicenseExpiresAtUtc], [CreatedAtUtc], [CreatedBy]
                FROM [HazardousWasteManagement].[Transporters]
                WHERE [TransporterId] = @TransporterId
                  AND ISNULL([IsDeleted], 0) = 0;
            END;
            """);

        // Procedure: usp_Transporters_Update.sql
        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [HazardousWasteManagement].[usp_Transporters_Update]
                @CorrelationId UNIQUEIDENTIFIER = NULL,
                @ActorId       UNIQUEIDENTIFIER = NULL,
                @NowUtc        DATETIME2(3),
                @TransporterId UNIQUEIDENTIFIER,
                @TenantId UNIQUEIDENTIFIER,
                @Status NVARCHAR(100)
            AS
            SET NOCOUNT ON;
            BEGIN TRY
                BEGIN TRANSACTION;
                    UPDATE [HazardousWasteManagement].[Transporters]
                    SET
                        [Status] = @Status,
                        [ModifiedAtUtc] = @NowUtc,
                        [ModifiedBy] = @ActorId,
                        [CorrelationId] = @CorrelationId
                    WHERE [TransporterId] = @TransporterId
                      AND [TenantId] = @TenantId
                      AND ISNULL([IsDeleted], 0) = 0;
                COMMIT TRANSACTION;
            END TRY
            BEGIN CATCH
                IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
                THROW;
            END CATCH;
            """);

        // Procedure: usp_TsdfFacilities_Create.sql
        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [HazardousWasteManagement].[usp_TsdfFacilities_Create]
                @CorrelationId UNIQUEIDENTIFIER = NULL,
                @ActorId       UNIQUEIDENTIFIER = NULL,
                @TsdfFacilityId UNIQUEIDENTIFIER,
                @TenantId UNIQUEIDENTIFIER,
                @SiteId UNIQUEIDENTIFIER = NULL,
                @Status NVARCHAR(100),
                @FacilityName NVARCHAR(400),
                @EpaFacilityId NVARCHAR(50),
                @PermitExpiresAtUtc DATETIME2(3) = NULL,
                @CreatedAtUtc DATETIME2(3),
                @CreatedBy UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN TRY
                BEGIN TRANSACTION;
                    INSERT INTO [HazardousWasteManagement].[TsdfFacilities]
                        ([TsdfFacilityId], [TenantId], [SiteId], [Status], [FacilityName], [EpaFacilityId], [PermitExpiresAtUtc], [CreatedAtUtc], [CreatedBy], [CorrelationId], [ActorId])
                    VALUES
                        (@TsdfFacilityId, @TenantId, @SiteId, @Status, @FacilityName, @EpaFacilityId, @PermitExpiresAtUtc, @CreatedAtUtc, @CreatedBy, @CorrelationId, @ActorId);
                COMMIT TRANSACTION;
            END TRY
            BEGIN CATCH
                IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
                THROW;
            END CATCH;
            """);

        // Procedure: usp_TsdfFacilities_GetAll.sql
        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [HazardousWasteManagement].[usp_TsdfFacilities_GetAll]
            AS
            BEGIN
                SET NOCOUNT ON;

                SELECT [TsdfFacilityId], [TenantId], [SiteId], [Status], [FacilityName], [EpaFacilityId], [PermitExpiresAtUtc], [CreatedAtUtc], [CreatedBy]
                FROM [HazardousWasteManagement].[TsdfFacilities]
                WHERE ISNULL([IsDeleted], 0) = 0;
            END;
            """);

        // Procedure: usp_TsdfFacilities_GetById.sql
        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [HazardousWasteManagement].[usp_TsdfFacilities_GetById]
            (
                @TsdfFacilityId UNIQUEIDENTIFIER
            )
            AS
            BEGIN
                SET NOCOUNT ON;

                SELECT [TsdfFacilityId], [TenantId], [SiteId], [Status], [FacilityName], [EpaFacilityId], [PermitExpiresAtUtc], [CreatedAtUtc], [CreatedBy]
                FROM [HazardousWasteManagement].[TsdfFacilities]
                WHERE [TsdfFacilityId] = @TsdfFacilityId
                  AND ISNULL([IsDeleted], 0) = 0;
            END;
            """);

        // Procedure: usp_TsdfFacilities_Update.sql
        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [HazardousWasteManagement].[usp_TsdfFacilities_Update]
                @CorrelationId UNIQUEIDENTIFIER = NULL,
                @ActorId       UNIQUEIDENTIFIER = NULL,
                @NowUtc        DATETIME2(3),
                @TsdfFacilityId UNIQUEIDENTIFIER,
                @TenantId UNIQUEIDENTIFIER,
                @Status NVARCHAR(100)
            AS
            SET NOCOUNT ON;
            BEGIN TRY
                BEGIN TRANSACTION;
                    UPDATE [HazardousWasteManagement].[TsdfFacilities]
                    SET
                        [Status] = @Status,
                        [ModifiedAtUtc] = @NowUtc,
                        [ModifiedBy] = @ActorId,
                        [CorrelationId] = @CorrelationId
                    WHERE [TsdfFacilityId] = @TsdfFacilityId
                      AND [TenantId] = @TenantId
                      AND ISNULL([IsDeleted], 0) = 0;
                COMMIT TRANSACTION;
            END TRY
            BEGIN CATCH
                IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
                THROW;
            END CATCH;
            """);

        // Procedure: usp_DisposalCertificates_Create.sql
        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [HazardousWasteManagement].[usp_DisposalCertificates_Create]
                @CorrelationId UNIQUEIDENTIFIER = NULL,
                @ActorId       UNIQUEIDENTIFIER = NULL,
                @DisposalCertificateId UNIQUEIDENTIFIER,
                @TenantId UNIQUEIDENTIFIER,
                @SiteId UNIQUEIDENTIFIER = NULL,
                @Status NVARCHAR(100),
                @ManifestId UNIQUEIDENTIFIER,
                @TsdfFacilityId UNIQUEIDENTIFIER,
                @DisposalMethod NVARCHAR(100),
                @DisposalDateUtc DATETIME2(3),
                @CertificateReceivedAtUtc DATETIME2(3),
                @CreatedAtUtc DATETIME2(3),
                @CreatedBy UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN TRY
                BEGIN TRANSACTION;
                    INSERT INTO [HazardousWasteManagement].[DisposalCertificates]
                        ([DisposalCertificateId], [TenantId], [SiteId], [Status], [ManifestId], [TsdfFacilityId], [DisposalMethod], [DisposalDateUtc], [CertificateReceivedAtUtc], [CreatedAtUtc], [CreatedBy], [CorrelationId], [ActorId])
                    VALUES
                        (@DisposalCertificateId, @TenantId, @SiteId, @Status, @ManifestId, @TsdfFacilityId, @DisposalMethod, @DisposalDateUtc, @CertificateReceivedAtUtc, @CreatedAtUtc, @CreatedBy, @CorrelationId, @ActorId);
                COMMIT TRANSACTION;
            END TRY
            BEGIN CATCH
                IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
                THROW;
            END CATCH;
            """);

        // Procedure: usp_DisposalCertificates_GetAll.sql
        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [HazardousWasteManagement].[usp_DisposalCertificates_GetAll]
            AS
            BEGIN
                SET NOCOUNT ON;

                SELECT [DisposalCertificateId], [TenantId], [SiteId], [Status], [ManifestId], [TsdfFacilityId], [DisposalMethod], [DisposalDateUtc], [CertificateReceivedAtUtc], [CreatedAtUtc], [CreatedBy]
                FROM [HazardousWasteManagement].[DisposalCertificates]
                WHERE ISNULL([IsDeleted], 0) = 0;
            END;
            """);

        // Procedure: usp_DisposalCertificates_GetById.sql
        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [HazardousWasteManagement].[usp_DisposalCertificates_GetById]
            (
                @DisposalCertificateId UNIQUEIDENTIFIER
            )
            AS
            BEGIN
                SET NOCOUNT ON;

                SELECT [DisposalCertificateId], [TenantId], [SiteId], [Status], [ManifestId], [TsdfFacilityId], [DisposalMethod], [DisposalDateUtc], [CertificateReceivedAtUtc], [CreatedAtUtc], [CreatedBy]
                FROM [HazardousWasteManagement].[DisposalCertificates]
                WHERE [DisposalCertificateId] = @DisposalCertificateId
                  AND ISNULL([IsDeleted], 0) = 0;
            END;
            """);

        // Procedure: usp_DisposalCertificates_GetHistoryByStream.sql
        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [HazardousWasteManagement].[usp_DisposalCertificates_GetHistoryByStream]
                @TenantId UNIQUEIDENTIFIER,
                @WasteStreamId UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT
                    dc.[DisposalCertificateId], dc.[TenantId], dc.[SiteId], m.[WasteStreamId], dc.[ManifestId], dc.[TsdfFacilityId],
                    dc.[DisposalMethod], dc.[DisposalDateUtc], dc.[CertificateReceivedAtUtc], dc.[Status]
                FROM [HazardousWasteManagement].[DisposalCertificates] dc
                INNER JOIN [HazardousWasteManagement].[Manifests] m ON m.[HazardousWasteManifestId] = dc.[ManifestId]
                WHERE dc.TenantId = @TenantId
                  AND m.[WasteStreamId] = @WasteStreamId
                  AND ISNULL(dc.[IsDeleted], 0) = 0
                ORDER BY dc.[CertificateReceivedAtUtc] DESC;
            END
            """);

        // Procedure: usp_DisposalCertificates_Update.sql
        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [HazardousWasteManagement].[usp_DisposalCertificates_Update]
                @CorrelationId UNIQUEIDENTIFIER = NULL,
                @ActorId       UNIQUEIDENTIFIER = NULL,
                @NowUtc        DATETIME2(3),
                @DisposalCertificateId UNIQUEIDENTIFIER,
                @TenantId UNIQUEIDENTIFIER,
                @Status NVARCHAR(100)
            AS
            SET NOCOUNT ON;
            BEGIN TRY
                BEGIN TRANSACTION;
                    UPDATE [HazardousWasteManagement].[DisposalCertificates]
                    SET
                        [Status] = @Status,
                        [ModifiedAtUtc] = @NowUtc,
                        [ModifiedBy] = @ActorId,
                        [CorrelationId] = @CorrelationId
                    WHERE [DisposalCertificateId] = @DisposalCertificateId
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

    /// <summary>
    /// Creates the <c>[AutonomousVehicleSafety]</c> schema's five tables (AutonomousVehicleAsset,
    /// AutonomousVehicleIncident, HumanRobotInteractionEvent, OperatingGeofence,
    /// SafetyCaseValidation) and their Create/GetById/GetAll/Update/read-model stored procedures
    /// against the Testcontainers database, mirroring
    /// <c>PlantSafety.Infrastructure\AutonomousVehicleSafety\Sql\Tables\*.sql</c> and
    /// <c>\Procedures\usp_*.sql</c> verbatim.
    /// </summary>
    public static async Task ApplyAutonomousVehicleSafetySchemaAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken);

        await conn.ExecuteAsync("""
            IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'AutonomousVehicleSafety')
                EXEC('CREATE SCHEMA [AutonomousVehicleSafety]');
            """);

        // ---- Table: AutonomousVehicleAsset ---------------------------------------
        await conn.ExecuteAsync("""
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AutonomousVehicleAsset' AND schema_id = SCHEMA_ID('AutonomousVehicleSafety'))
            BEGIN
                CREATE TABLE [AutonomousVehicleSafety].[AutonomousVehicleAsset] (
                    [AutonomousVehicleAssetId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                    [TenantId] UNIQUEIDENTIFIER NOT NULL,
                    [SiteId] UNIQUEIDENTIFIER NULL,
                    [Status] NVARCHAR(100) NOT NULL,
                    [AssetTag] NVARCHAR(200) NOT NULL,
                    [VehicleType] NVARCHAR(100) NOT NULL,
                    [AutonomyLevel] NVARCHAR(100) NOT NULL,
                    [CurrentAutonomyMode] NVARCHAR(100) NOT NULL,
                    [SafetyCaseValidationId] UNIQUEIDENTIFIER NULL,
                    [ModeTransitionCount] INT NOT NULL CONSTRAINT DF_AutonomousVehicleAsset_ModeTransitionCount DEFAULT (0),
                    [SuspensionReason] NVARCHAR(1000) NULL,
                    [SuspendedAtUtc] DATETIME2(3) NULL,
                    [CreatedAtUtc] DATETIME2(3) NOT NULL,
                    [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
                    [IsDeleted] BIT NOT NULL CONSTRAINT DF_AutonomousVehicleAsset_IsDeleted DEFAULT (0),
                    [ModifiedAtUtc] DATETIME2(3) NULL,
                    [ModifiedBy] UNIQUEIDENTIFIER NULL,
                    [CorrelationId] UNIQUEIDENTIFIER NULL,
                    [ActorId] UNIQUEIDENTIFIER NULL
                );

                CREATE NONCLUSTERED INDEX [IX_AutonomousVehicleAsset_TenantId_SiteId]
                    ON [AutonomousVehicleSafety].[AutonomousVehicleAsset] ([TenantId], [SiteId])
                    INCLUDE ([Status], [AssetTag]);
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [AutonomousVehicleSafety].[usp_AutonomousVehicleAsset_Create]
                @CorrelationId UNIQUEIDENTIFIER = NULL,
                @ActorId       UNIQUEIDENTIFIER = NULL,
                @AutonomousVehicleAssetId UNIQUEIDENTIFIER,
                @TenantId UNIQUEIDENTIFIER,
                @SiteId UNIQUEIDENTIFIER = NULL,
                @Status NVARCHAR(100),
                @AssetTag NVARCHAR(200),
                @VehicleType NVARCHAR(100),
                @AutonomyLevel NVARCHAR(100),
                @CurrentAutonomyMode NVARCHAR(100),
                @SafetyCaseValidationId UNIQUEIDENTIFIER = NULL,
                @ModeTransitionCount INT,
                @SuspensionReason NVARCHAR(1000) = NULL,
                @SuspendedAtUtc DATETIME2(3) = NULL,
                @CreatedAtUtc DATETIME2(3),
                @CreatedBy UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN TRY
                BEGIN TRANSACTION;
                    INSERT INTO [AutonomousVehicleSafety].[AutonomousVehicleAsset]
                        ([AutonomousVehicleAssetId], [TenantId], [SiteId], [Status], [AssetTag], [VehicleType], [AutonomyLevel], [CurrentAutonomyMode], [SafetyCaseValidationId], [ModeTransitionCount], [SuspensionReason], [SuspendedAtUtc], [CreatedAtUtc], [CreatedBy], [CorrelationId], [ActorId])
                    VALUES
                        (@AutonomousVehicleAssetId, @TenantId, @SiteId, @Status, @AssetTag, @VehicleType, @AutonomyLevel, @CurrentAutonomyMode, @SafetyCaseValidationId, @ModeTransitionCount, @SuspensionReason, @SuspendedAtUtc, @CreatedAtUtc, @CreatedBy, @CorrelationId, @ActorId);
                COMMIT TRANSACTION;
            END TRY
            BEGIN CATCH
                IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
                THROW;
            END CATCH;
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [AutonomousVehicleSafety].[usp_AutonomousVehicleAsset_GetById]
            (
                @AutonomousVehicleAssetId UNIQUEIDENTIFIER
            )
            AS
            BEGIN
                SET NOCOUNT ON;

                SELECT [AutonomousVehicleAssetId], [TenantId], [SiteId], [Status], [AssetTag], [VehicleType], [AutonomyLevel], [CurrentAutonomyMode], [SafetyCaseValidationId], [ModeTransitionCount], [SuspensionReason], [SuspendedAtUtc], [CreatedAtUtc], [CreatedBy]
                FROM [AutonomousVehicleSafety].[AutonomousVehicleAsset]
                WHERE [AutonomousVehicleAssetId] = @AutonomousVehicleAssetId
                  AND ISNULL([IsDeleted], 0) = 0;
            END;
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [AutonomousVehicleSafety].[usp_AutonomousVehicleAsset_GetItemById]
                @TenantId UNIQUEIDENTIFIER,
                @AutonomousVehicleAssetId UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT
                    [AutonomousVehicleAssetId],
                    [TenantId],
                    [SiteId],
                    [AssetTag],
                    [VehicleType],
                    [AutonomyLevel],
                    [CurrentAutonomyMode],
                    [ModeTransitionCount],
                    [Status] AS [Status],
                    [SuspensionReason],
                    [CreatedAtUtc],
                    [CreatedBy]
                FROM [AutonomousVehicleSafety].[AutonomousVehicleAsset]
                WHERE [TenantId] = @TenantId
                  AND [AutonomousVehicleAssetId] = @AutonomousVehicleAssetId
                  AND ISNULL([IsDeleted], 0) = 0;
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [AutonomousVehicleSafety].[usp_AutonomousVehicleAsset_Update]
                @CorrelationId UNIQUEIDENTIFIER = NULL,
                @ActorId       UNIQUEIDENTIFIER = NULL,
                @NowUtc        DATETIME2(3),
                @AutonomousVehicleAssetId UNIQUEIDENTIFIER,
                @TenantId UNIQUEIDENTIFIER,
                @SiteId UNIQUEIDENTIFIER = NULL,
                @Status NVARCHAR(100),
                @AssetTag NVARCHAR(200),
                @VehicleType NVARCHAR(100),
                @AutonomyLevel NVARCHAR(100),
                @CurrentAutonomyMode NVARCHAR(100),
                @SafetyCaseValidationId UNIQUEIDENTIFIER = NULL,
                @ModeTransitionCount INT,
                @SuspensionReason NVARCHAR(1000) = NULL,
                @SuspendedAtUtc DATETIME2(3) = NULL
            AS
            SET NOCOUNT ON;
            BEGIN TRY
                BEGIN TRANSACTION;
                    UPDATE [AutonomousVehicleSafety].[AutonomousVehicleAsset]
                    SET
                    [SiteId] = @SiteId,
                    [Status] = @Status,
                    [AssetTag] = @AssetTag,
                    [VehicleType] = @VehicleType,
                    [AutonomyLevel] = @AutonomyLevel,
                    [CurrentAutonomyMode] = @CurrentAutonomyMode,
                    [SafetyCaseValidationId] = @SafetyCaseValidationId,
                    [ModeTransitionCount] = @ModeTransitionCount,
                    [SuspensionReason] = @SuspensionReason,
                    [SuspendedAtUtc] = @SuspendedAtUtc,
                    [ModifiedAtUtc] = @NowUtc,
                    [ModifiedBy] = @ActorId,
                    [CorrelationId] = @CorrelationId
                    WHERE [AutonomousVehicleAssetId] = @AutonomousVehicleAssetId
                      AND [TenantId] = @TenantId
                      AND ISNULL([IsDeleted], 0) = 0;
                COMMIT TRANSACTION;
            END TRY
            BEGIN CATCH
                IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
                THROW;
            END CATCH;
            """);

        // ---- Table: AutonomousVehicleIncident -------------------------------------
        await conn.ExecuteAsync("""
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
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [AutonomousVehicleSafety].[usp_AutonomousVehicleIncident_Create]
                @CorrelationId UNIQUEIDENTIFIER = NULL,
                @ActorId       UNIQUEIDENTIFIER = NULL,
                @AutonomousVehicleIncidentId UNIQUEIDENTIFIER,
                @TenantId UNIQUEIDENTIFIER,
                @SiteId UNIQUEIDENTIFIER = NULL,
                @Status NVARCHAR(100),
                @AutonomousVehicleAssetId UNIQUEIDENTIFIER,
                @IncidentType NVARCHAR(100),
                @SeverityLevel NVARCHAR(100),
                @OccurredAtUtc DATETIME2(3),
                @DescriptionSummary NVARCHAR(2000),
                @InvestigationLeadUserId UNIQUEIDENTIFIER = NULL,
                @SuspensionTriggered BIT,
                @ClosedAtUtc DATETIME2(3) = NULL,
                @CreatedAtUtc DATETIME2(3),
                @CreatedBy UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN TRY
                BEGIN TRANSACTION;
                    INSERT INTO [AutonomousVehicleSafety].[AutonomousVehicleIncident]
                        ([AutonomousVehicleIncidentId], [TenantId], [SiteId], [Status], [AutonomousVehicleAssetId], [IncidentType], [SeverityLevel], [OccurredAtUtc], [DescriptionSummary], [InvestigationLeadUserId], [SuspensionTriggered], [ClosedAtUtc], [CreatedAtUtc], [CreatedBy], [CorrelationId], [ActorId])
                    VALUES
                        (@AutonomousVehicleIncidentId, @TenantId, @SiteId, @Status, @AutonomousVehicleAssetId, @IncidentType, @SeverityLevel, @OccurredAtUtc, @DescriptionSummary, @InvestigationLeadUserId, @SuspensionTriggered, @ClosedAtUtc, @CreatedAtUtc, @CreatedBy, @CorrelationId, @ActorId);
                COMMIT TRANSACTION;
            END TRY
            BEGIN CATCH
                IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
                THROW;
            END CATCH;
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [AutonomousVehicleSafety].[usp_AutonomousVehicleIncident_GetById]
            (
                @AutonomousVehicleIncidentId UNIQUEIDENTIFIER
            )
            AS
            BEGIN
                SET NOCOUNT ON;

                SELECT [AutonomousVehicleIncidentId], [TenantId], [SiteId], [Status], [AutonomousVehicleAssetId], [IncidentType], [SeverityLevel], [OccurredAtUtc], [DescriptionSummary], [InvestigationLeadUserId], [SuspensionTriggered], [ClosedAtUtc], [CreatedAtUtc], [CreatedBy]
                FROM [AutonomousVehicleSafety].[AutonomousVehicleIncident]
                WHERE [AutonomousVehicleIncidentId] = @AutonomousVehicleIncidentId
                  AND ISNULL([IsDeleted], 0) = 0;
            END;
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [AutonomousVehicleSafety].[usp_AutonomousVehicleIncident_GetHistoryByAssetId]
                @TenantId UNIQUEIDENTIFIER,
                @AutonomousVehicleAssetId UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT
                    [AutonomousVehicleIncidentId],
                    [TenantId],
                    [SiteId],
                    [AutonomousVehicleAssetId],
                    [IncidentType],
                    [SeverityLevel],
                    [Status] AS [Status],
                    [SuspensionTriggered],
                    [OccurredAtUtc],
                    [CreatedAtUtc],
                    [CreatedBy]
                FROM [AutonomousVehicleSafety].[AutonomousVehicleIncident]
                WHERE [TenantId] = @TenantId
                  AND [AutonomousVehicleAssetId] = @AutonomousVehicleAssetId
                  AND ISNULL([IsDeleted], 0) = 0
                ORDER BY [OccurredAtUtc] DESC;
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [AutonomousVehicleSafety].[usp_AutonomousVehicleIncident_Update]
                @CorrelationId UNIQUEIDENTIFIER = NULL,
                @ActorId       UNIQUEIDENTIFIER = NULL,
                @NowUtc        DATETIME2(3),
                @AutonomousVehicleIncidentId UNIQUEIDENTIFIER,
                @TenantId UNIQUEIDENTIFIER,
                @SiteId UNIQUEIDENTIFIER = NULL,
                @Status NVARCHAR(100),
                @AutonomousVehicleAssetId UNIQUEIDENTIFIER,
                @IncidentType NVARCHAR(100),
                @SeverityLevel NVARCHAR(100),
                @OccurredAtUtc DATETIME2(3),
                @DescriptionSummary NVARCHAR(2000),
                @InvestigationLeadUserId UNIQUEIDENTIFIER = NULL,
                @SuspensionTriggered BIT,
                @ClosedAtUtc DATETIME2(3) = NULL
            AS
            SET NOCOUNT ON;
            BEGIN TRY
                BEGIN TRANSACTION;
                    UPDATE [AutonomousVehicleSafety].[AutonomousVehicleIncident]
                    SET
                    [SiteId] = @SiteId,
                    [Status] = @Status,
                    [AutonomousVehicleAssetId] = @AutonomousVehicleAssetId,
                    [IncidentType] = @IncidentType,
                    [SeverityLevel] = @SeverityLevel,
                    [OccurredAtUtc] = @OccurredAtUtc,
                    [DescriptionSummary] = @DescriptionSummary,
                    [InvestigationLeadUserId] = @InvestigationLeadUserId,
                    [SuspensionTriggered] = @SuspensionTriggered,
                    [ClosedAtUtc] = @ClosedAtUtc,
                    [ModifiedAtUtc] = @NowUtc,
                    [ModifiedBy] = @ActorId,
                    [CorrelationId] = @CorrelationId
                    WHERE [AutonomousVehicleIncidentId] = @AutonomousVehicleIncidentId
                      AND [TenantId] = @TenantId
                      AND ISNULL([IsDeleted], 0) = 0;
                COMMIT TRANSACTION;
            END TRY
            BEGIN CATCH
                IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
                THROW;
            END CATCH;
            """);

        // ---- Table: HumanRobotInteractionEvent ------------------------------------
        await conn.ExecuteAsync("""
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
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [AutonomousVehicleSafety].[usp_HumanRobotInteractionEvent_Create]
                @CorrelationId UNIQUEIDENTIFIER = NULL,
                @ActorId       UNIQUEIDENTIFIER = NULL,
                @HumanRobotInteractionEventId UNIQUEIDENTIFIER,
                @TenantId UNIQUEIDENTIFIER,
                @SiteId UNIQUEIDENTIFIER = NULL,
                @Status NVARCHAR(100),
                @AutonomousVehicleAssetId UNIQUEIDENTIFIER,
                @InteractionType NVARCHAR(100),
                @SeverityLevel NVARCHAR(100),
                @DistanceMeters DECIMAL(9,3) = NULL,
                @LocationDescription NVARCHAR(400),
                @DetectedAtUtc DATETIME2(3),
                @ReviewedByUserId UNIQUEIDENTIFIER = NULL,
                @ReviewedAtUtc DATETIME2(3) = NULL,
                @CreatedAtUtc DATETIME2(3),
                @CreatedBy UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN TRY
                BEGIN TRANSACTION;
                    INSERT INTO [AutonomousVehicleSafety].[HumanRobotInteractionEvent]
                        ([HumanRobotInteractionEventId], [TenantId], [SiteId], [Status], [AutonomousVehicleAssetId], [InteractionType], [SeverityLevel], [DistanceMeters], [LocationDescription], [DetectedAtUtc], [ReviewedByUserId], [ReviewedAtUtc], [CreatedAtUtc], [CreatedBy], [CorrelationId], [ActorId])
                    VALUES
                        (@HumanRobotInteractionEventId, @TenantId, @SiteId, @Status, @AutonomousVehicleAssetId, @InteractionType, @SeverityLevel, @DistanceMeters, @LocationDescription, @DetectedAtUtc, @ReviewedByUserId, @ReviewedAtUtc, @CreatedAtUtc, @CreatedBy, @CorrelationId, @ActorId);
                COMMIT TRANSACTION;
            END TRY
            BEGIN CATCH
                IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
                THROW;
            END CATCH;
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [AutonomousVehicleSafety].[usp_HumanRobotInteractionEvent_GetItems]
                @TenantId UNIQUEIDENTIFIER,
                @SiteId   UNIQUEIDENTIFIER = NULL,
                @FromUtc  DATETIME2(3),
                @ToUtc    DATETIME2(3)
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT
                    [HumanRobotInteractionEventId],
                    [TenantId],
                    [SiteId],
                    [AutonomousVehicleAssetId],
                    [InteractionType],
                    [SeverityLevel],
                    [DistanceMeters],
                    [Status] AS [Status],
                    [DetectedAtUtc],
                    [CreatedAtUtc],
                    [CreatedBy]
                FROM [AutonomousVehicleSafety].[HumanRobotInteractionEvent]
                WHERE [TenantId] = @TenantId
                  AND (@SiteId IS NULL OR [SiteId] = @SiteId)
                  AND ISNULL([IsDeleted], 0) = 0
                  AND [DetectedAtUtc] >= @FromUtc
                  AND [DetectedAtUtc] <= @ToUtc
                ORDER BY [DetectedAtUtc] DESC;
            END
            """);

        // ---- Table: OperatingGeofence ----------------------------------------------
        await conn.ExecuteAsync("""
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'OperatingGeofence' AND schema_id = SCHEMA_ID('AutonomousVehicleSafety'))
            BEGIN
                CREATE TABLE [AutonomousVehicleSafety].[OperatingGeofence] (
                    [OperatingGeofenceId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                    [TenantId] UNIQUEIDENTIFIER NOT NULL,
                    [SiteId] UNIQUEIDENTIFIER NULL,
                    [Status] NVARCHAR(100) NOT NULL,
                    [GeofenceName] NVARCHAR(200) NOT NULL,
                    [ZoneType] NVARCHAR(100) NOT NULL,
                    [BoundaryDefinition] NVARCHAR(MAX) NOT NULL,
                    [MaxSpeedKph] DECIMAL(9,2) NULL,
                    [ViolationCount] INT NOT NULL CONSTRAINT DF_OperatingGeofence_ViolationCount DEFAULT (0),
                    [LastAuditedAtUtc] DATETIME2(3) NULL,
                    [NextAuditDueAtUtc] DATETIME2(3) NULL,
                    [CreatedAtUtc] DATETIME2(3) NOT NULL,
                    [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
                    [IsDeleted] BIT NOT NULL CONSTRAINT DF_OperatingGeofence_IsDeleted DEFAULT (0),
                    [ModifiedAtUtc] DATETIME2(3) NULL,
                    [ModifiedBy] UNIQUEIDENTIFIER NULL,
                    [CorrelationId] UNIQUEIDENTIFIER NULL,
                    [ActorId] UNIQUEIDENTIFIER NULL
                );

                CREATE NONCLUSTERED INDEX [IX_OperatingGeofence_TenantId_SiteId]
                    ON [AutonomousVehicleSafety].[OperatingGeofence] ([TenantId], [SiteId])
                    INCLUDE ([Status], [GeofenceName], [ViolationCount]);
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [AutonomousVehicleSafety].[usp_OperatingGeofence_Create]
                @CorrelationId UNIQUEIDENTIFIER = NULL,
                @ActorId       UNIQUEIDENTIFIER = NULL,
                @OperatingGeofenceId UNIQUEIDENTIFIER,
                @TenantId UNIQUEIDENTIFIER,
                @SiteId UNIQUEIDENTIFIER = NULL,
                @Status NVARCHAR(100),
                @GeofenceName NVARCHAR(200),
                @ZoneType NVARCHAR(100),
                @BoundaryDefinition NVARCHAR(MAX),
                @MaxSpeedKph DECIMAL(9,2) = NULL,
                @ViolationCount INT,
                @LastAuditedAtUtc DATETIME2(3) = NULL,
                @NextAuditDueAtUtc DATETIME2(3) = NULL,
                @CreatedAtUtc DATETIME2(3),
                @CreatedBy UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN TRY
                BEGIN TRANSACTION;
                    INSERT INTO [AutonomousVehicleSafety].[OperatingGeofence]
                        ([OperatingGeofenceId], [TenantId], [SiteId], [Status], [GeofenceName], [ZoneType], [BoundaryDefinition], [MaxSpeedKph], [ViolationCount], [LastAuditedAtUtc], [NextAuditDueAtUtc], [CreatedAtUtc], [CreatedBy], [CorrelationId], [ActorId])
                    VALUES
                        (@OperatingGeofenceId, @TenantId, @SiteId, @Status, @GeofenceName, @ZoneType, @BoundaryDefinition, @MaxSpeedKph, @ViolationCount, @LastAuditedAtUtc, @NextAuditDueAtUtc, @CreatedAtUtc, @CreatedBy, @CorrelationId, @ActorId);
                COMMIT TRANSACTION;
            END TRY
            BEGIN CATCH
                IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
                THROW;
            END CATCH;
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [AutonomousVehicleSafety].[usp_OperatingGeofence_GetActiveViolations]
                @TenantId UNIQUEIDENTIFIER,
                @SiteId   UNIQUEIDENTIFIER = NULL,
                @FromUtc  DATETIME2(3),
                @ToUtc    DATETIME2(3)
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT
                    [OperatingGeofenceId],
                    [TenantId],
                    [SiteId],
                    [GeofenceName],
                    [ZoneType],
                    [ViolationCount],
                    [Status] AS [Status],
                    [LastAuditedAtUtc],
                    [CreatedAtUtc],
                    [CreatedBy]
                FROM [AutonomousVehicleSafety].[OperatingGeofence]
                WHERE [TenantId] = @TenantId
                  AND (@SiteId IS NULL OR [SiteId] = @SiteId)
                  AND [Status] = 'Active'
                  AND [ViolationCount] > 0
                  AND ISNULL([IsDeleted], 0) = 0
                  AND [CreatedAtUtc] >= @FromUtc
                  AND [CreatedAtUtc] <= @ToUtc
                ORDER BY [ViolationCount] DESC, [CreatedAtUtc] DESC;
            END
            """);

        // ---- Table: SafetyCaseValidation --------------------------------------------
        await conn.ExecuteAsync("""
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'SafetyCaseValidation' AND schema_id = SCHEMA_ID('AutonomousVehicleSafety'))
            BEGIN
                CREATE TABLE [AutonomousVehicleSafety].[SafetyCaseValidation] (
                    [SafetyCaseValidationId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                    [TenantId] UNIQUEIDENTIFIER NOT NULL,
                    [SiteId] UNIQUEIDENTIFIER NULL,
                    [Status] NVARCHAR(100) NOT NULL,
                    [AutonomousVehicleAssetId] UNIQUEIDENTIFIER NOT NULL,
                    [StandardReference] NVARCHAR(100) NOT NULL,
                    [DocumentVersion] NVARCHAR(50) NOT NULL,
                    [RevalidationCadenceDays] INT NOT NULL,
                    [ValidatedByUserId] UNIQUEIDENTIFIER NULL,
                    [ValidatedAtUtc] DATETIME2(3) NULL,
                    [ExpiresAtUtc] DATETIME2(3) NULL,
                    [RejectionReason] NVARCHAR(1000) NULL,
                    [CreatedAtUtc] DATETIME2(3) NOT NULL,
                    [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
                    [IsDeleted] BIT NOT NULL CONSTRAINT DF_SafetyCaseValidation_IsDeleted DEFAULT (0),
                    [ModifiedAtUtc] DATETIME2(3) NULL,
                    [ModifiedBy] UNIQUEIDENTIFIER NULL,
                    [CorrelationId] UNIQUEIDENTIFIER NULL,
                    [ActorId] UNIQUEIDENTIFIER NULL
                );

                CREATE NONCLUSTERED INDEX [IX_SafetyCaseValidation_TenantId_AssetId]
                    ON [AutonomousVehicleSafety].[SafetyCaseValidation] ([TenantId], [AutonomousVehicleAssetId])
                    INCLUDE ([Status], [ExpiresAtUtc], [CreatedAtUtc]);
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [AutonomousVehicleSafety].[usp_SafetyCaseValidation_Create]
                @CorrelationId UNIQUEIDENTIFIER = NULL,
                @ActorId       UNIQUEIDENTIFIER = NULL,
                @SafetyCaseValidationId UNIQUEIDENTIFIER,
                @TenantId UNIQUEIDENTIFIER,
                @SiteId UNIQUEIDENTIFIER = NULL,
                @Status NVARCHAR(100),
                @AutonomousVehicleAssetId UNIQUEIDENTIFIER,
                @StandardReference NVARCHAR(100),
                @DocumentVersion NVARCHAR(50),
                @RevalidationCadenceDays INT,
                @ValidatedByUserId UNIQUEIDENTIFIER = NULL,
                @ValidatedAtUtc DATETIME2(3) = NULL,
                @ExpiresAtUtc DATETIME2(3) = NULL,
                @RejectionReason NVARCHAR(1000) = NULL,
                @CreatedAtUtc DATETIME2(3),
                @CreatedBy UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN TRY
                BEGIN TRANSACTION;
                    INSERT INTO [AutonomousVehicleSafety].[SafetyCaseValidation]
                        ([SafetyCaseValidationId], [TenantId], [SiteId], [Status], [AutonomousVehicleAssetId], [StandardReference], [DocumentVersion], [RevalidationCadenceDays], [ValidatedByUserId], [ValidatedAtUtc], [ExpiresAtUtc], [RejectionReason], [CreatedAtUtc], [CreatedBy], [CorrelationId], [ActorId])
                    VALUES
                        (@SafetyCaseValidationId, @TenantId, @SiteId, @Status, @AutonomousVehicleAssetId, @StandardReference, @DocumentVersion, @RevalidationCadenceDays, @ValidatedByUserId, @ValidatedAtUtc, @ExpiresAtUtc, @RejectionReason, @CreatedAtUtc, @CreatedBy, @CorrelationId, @ActorId);
                COMMIT TRANSACTION;
            END TRY
            BEGIN CATCH
                IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
                THROW;
            END CATCH;
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [AutonomousVehicleSafety].[usp_SafetyCaseValidation_GetStatusByAssetId]
                @TenantId UNIQUEIDENTIFIER,
                @AutonomousVehicleAssetId UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT TOP (1)
                    [SafetyCaseValidationId],
                    [TenantId],
                    [SiteId],
                    [AutonomousVehicleAssetId],
                    [StandardReference],
                    [DocumentVersion],
                    [Status] AS [Status],
                    [ValidatedAtUtc],
                    [ExpiresAtUtc],
                    [CreatedAtUtc],
                    [CreatedBy]
                FROM [AutonomousVehicleSafety].[SafetyCaseValidation]
                WHERE [TenantId] = @TenantId
                  AND [AutonomousVehicleAssetId] = @AutonomousVehicleAssetId
                  AND ISNULL([IsDeleted], 0) = 0
                ORDER BY [CreatedAtUtc] DESC;
            END
            """);
    }

    /// <summary>
    /// Creates the <c>[CyberPhysicalSecurity]</c> schema's five tables (<c>OtAsset</c>,
    /// <c>ControlChangeEvent</c>, <c>SensorIntegrityCheck</c>, <c>SabotageIndicator</c>,
    /// <c>CyberPhysicalIncident</c>) and their stored procedures against the Testcontainers
    /// database, mirroring <c>PlantSafety.Infrastructure\CyberPhysicalSecurity\Sql\Tables\*.sql</c>
    /// and <c>\Procedures\usp_*.sql</c> verbatim. <c>ControlChangeEvent</c> and
    /// <c>SensorIntegrityCheck</c> both FK-reference <c>OtAsset</c>, so table creation order matters.
    /// <c>ControlChangeEvent</c> is append-only/tamper-evident: <c>usp_ControlChangeEvent_Insert</c>
    /// never mutates <c>PriorRecordHash</c>/<c>RecordHash</c>, only
    /// <c>usp_ControlChangeEvent_ReclassifyAuthorization</c> may update the mutable
    /// <c>AuthorizationStatus</c> column, matching <c>ControlChangeEvent.ComputeRecordHash</c>'s
    /// SHA-256 hash-chain in the Domain layer.
    /// </summary>
    public static async Task ApplyCyberPhysicalSecuritySchemaAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken);

        await conn.ExecuteAsync("""
            IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'CyberPhysicalSecurity')
                EXEC('CREATE SCHEMA [CyberPhysicalSecurity]');
            """);

        // ---- Table: OtAsset --------------------------------------------------------
        await conn.ExecuteAsync("""
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'OtAsset' AND schema_id = SCHEMA_ID('CyberPhysicalSecurity'))
            BEGIN
                CREATE TABLE [CyberPhysicalSecurity].[OtAsset]
                (
                    OtAssetId            UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_OtAsset PRIMARY KEY,
                    TenantId             UNIQUEIDENTIFIER NOT NULL,
                    SiteId               UNIQUEIDENTIFIER NOT NULL,
                    AssetTag             NVARCHAR(128)    NOT NULL,
                    AssetName            NVARCHAR(256)    NOT NULL,
                    AssetType            NVARCHAR(30)     NOT NULL,
                    Vendor               NVARCHAR(128)    NOT NULL CONSTRAINT DF_OtAsset_Vendor DEFAULT (N''),
                    Model                NVARCHAR(128)    NOT NULL CONSTRAINT DF_OtAsset_Model DEFAULT (N''),
                    FirmwareVersion      NVARCHAR(64)     NOT NULL CONSTRAINT DF_OtAsset_Firmware DEFAULT (N''),
                    NetworkZone          NVARCHAR(64)     NOT NULL CONSTRAINT DF_OtAsset_Zone DEFAULT (N''),
                    Criticality          NVARCHAR(20)     NOT NULL,
                    Status               NVARCHAR(20)     NOT NULL,
                    LastReconciledAtUtc  DATETIME2(3)     NULL,
                    CreatedAtUtc         DATETIME2(3)     NOT NULL,
                    CreatedBy            UNIQUEIDENTIFIER NOT NULL,
                    RowVersion           ROWVERSION       NOT NULL
                );

                CREATE UNIQUE NONCLUSTERED INDEX UX_OtAsset_Tenant_AssetTag ON [CyberPhysicalSecurity].[OtAsset] (TenantId, AssetTag);
                CREATE NONCLUSTERED INDEX IX_OtAsset_Tenant_Status ON [CyberPhysicalSecurity].[OtAsset] (TenantId, Status);
                CREATE NONCLUSTERED INDEX IX_OtAsset_Tenant_Site ON [CyberPhysicalSecurity].[OtAsset] (TenantId, SiteId);
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [CyberPhysicalSecurity].[usp_OtAsset_Insert]
                @OtAssetId           UNIQUEIDENTIFIER,
                @TenantId            UNIQUEIDENTIFIER,
                @SiteId              UNIQUEIDENTIFIER,
                @AssetTag            NVARCHAR(128),
                @AssetName           NVARCHAR(256),
                @AssetType           NVARCHAR(30),
                @Vendor              NVARCHAR(128),
                @Model               NVARCHAR(128),
                @FirmwareVersion     NVARCHAR(64),
                @NetworkZone         NVARCHAR(64),
                @Criticality         NVARCHAR(20),
                @Status              NVARCHAR(20),
                @LastReconciledAtUtc DATETIME2(3) = NULL,
                @CreatedAtUtc        DATETIME2(3),
                @CreatedBy           UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN
                BEGIN TRY
                    INSERT INTO [CyberPhysicalSecurity].[OtAsset]
                    (OtAssetId, TenantId, SiteId, AssetTag, AssetName, AssetType, Vendor, Model, FirmwareVersion, NetworkZone, Criticality, Status, LastReconciledAtUtc, CreatedAtUtc, CreatedBy)
                    VALUES
                    (@OtAssetId, @TenantId, @SiteId, @AssetTag, @AssetName, @AssetType, @Vendor, @Model, @FirmwareVersion, @NetworkZone, @Criticality, @Status, @LastReconciledAtUtc, @CreatedAtUtc, @CreatedBy);
                END TRY
                BEGIN CATCH
                    THROW;
                END CATCH
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [CyberPhysicalSecurity].[usp_OtAsset_Update]
                @OtAssetId           UNIQUEIDENTIFIER,
                @TenantId            UNIQUEIDENTIFIER,
                @Status              NVARCHAR(20),
                @LastReconciledAtUtc DATETIME2(3) = NULL
            AS
            SET NOCOUNT ON;
            BEGIN
                BEGIN TRY
                    UPDATE [CyberPhysicalSecurity].[OtAsset]
                    SET Status = @Status,
                        LastReconciledAtUtc = @LastReconciledAtUtc
                    WHERE OtAssetId = @OtAssetId AND TenantId = @TenantId;
                END TRY
                BEGIN CATCH
                    THROW;
                END CATCH
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [CyberPhysicalSecurity].[usp_OtAsset_GetById]
                @OtAssetId UNIQUEIDENTIFIER,
                @TenantId  UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT OtAssetId, TenantId, SiteId, AssetTag, AssetName, AssetType, Vendor, Model, FirmwareVersion,
                       NetworkZone, Criticality, Status, LastReconciledAtUtc, CreatedAtUtc, CreatedBy
                FROM [CyberPhysicalSecurity].[OtAsset]
                WHERE OtAssetId = @OtAssetId AND TenantId = @TenantId;
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [CyberPhysicalSecurity].[usp_OtAsset_GetAllActive]
                @TenantId UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT OtAssetId, TenantId, SiteId, AssetTag, AssetName, AssetType, Vendor, Model, FirmwareVersion,
                       NetworkZone, Criticality, Status, LastReconciledAtUtc, CreatedAtUtc, CreatedBy
                FROM [CyberPhysicalSecurity].[OtAsset]
                WHERE TenantId = @TenantId
                ORDER BY AssetTag;
            END
            """);

        // ---- Table: ControlChangeEvent (FK -> OtAsset) -----------------------------
        await conn.ExecuteAsync("""
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ControlChangeEvent' AND schema_id = SCHEMA_ID('CyberPhysicalSecurity'))
            BEGIN
                CREATE TABLE [CyberPhysicalSecurity].[ControlChangeEvent]
                (
                    ControlChangeEventId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_ControlChangeEvent PRIMARY KEY,
                    TenantId              UNIQUEIDENTIFIER NOT NULL,
                    SiteId                UNIQUEIDENTIFIER NOT NULL,
                    OtAssetId             UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_ControlChangeEvent_OtAsset REFERENCES [CyberPhysicalSecurity].[OtAsset] (OtAssetId),
                    ChangeDescription     NVARCHAR(2000)   NOT NULL CONSTRAINT DF_ControlChangeEvent_Desc DEFAULT (N''),
                    ChangedParameter      NVARCHAR(256)    NOT NULL,
                    OldValue              NVARCHAR(512)    NOT NULL CONSTRAINT DF_ControlChangeEvent_Old DEFAULT (N''),
                    NewValue              NVARCHAR(512)    NOT NULL CONSTRAINT DF_ControlChangeEvent_New DEFAULT (N''),
                    ChangeSourceUserId    UNIQUEIDENTIFIER NULL,
                    AuthorizationStatus   NVARCHAR(20)     NOT NULL,
                    DetectedAtUtc         DATETIME2(3)     NOT NULL,
                    PriorRecordHash       CHAR(64)         NULL,
                    RecordHash            CHAR(64)         NOT NULL,
                    CreatedAtUtc          DATETIME2(3)     NOT NULL,
                    CreatedBy             UNIQUEIDENTIFIER NOT NULL,
                    RowVersion            ROWVERSION       NOT NULL
                );

                CREATE NONCLUSTERED INDEX IX_ControlChangeEvent_Tenant_Asset_DetectedAt ON [CyberPhysicalSecurity].[ControlChangeEvent] (TenantId, OtAssetId, DetectedAtUtc DESC);
                CREATE NONCLUSTERED INDEX IX_ControlChangeEvent_Tenant_AuthStatus ON [CyberPhysicalSecurity].[ControlChangeEvent] (TenantId, AuthorizationStatus);
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [CyberPhysicalSecurity].[usp_ControlChangeEvent_Insert]
                @ControlChangeEventId UNIQUEIDENTIFIER,
                @TenantId             UNIQUEIDENTIFIER,
                @SiteId               UNIQUEIDENTIFIER,
                @OtAssetId            UNIQUEIDENTIFIER,
                @ChangeDescription    NVARCHAR(2000),
                @ChangedParameter     NVARCHAR(256),
                @OldValue             NVARCHAR(512),
                @NewValue             NVARCHAR(512),
                @ChangeSourceUserId   UNIQUEIDENTIFIER = NULL,
                @AuthorizationStatus  NVARCHAR(20),
                @DetectedAtUtc        DATETIME2(3),
                @PriorRecordHash      CHAR(64) = NULL,
                @RecordHash           CHAR(64),
                @CreatedAtUtc         DATETIME2(3),
                @CreatedBy            UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN
                BEGIN TRY
                    INSERT INTO [CyberPhysicalSecurity].[ControlChangeEvent]
                    (ControlChangeEventId, TenantId, SiteId, OtAssetId, ChangeDescription, ChangedParameter, OldValue, NewValue,
                     ChangeSourceUserId, AuthorizationStatus, DetectedAtUtc, PriorRecordHash, RecordHash, CreatedAtUtc, CreatedBy)
                    VALUES
                    (@ControlChangeEventId, @TenantId, @SiteId, @OtAssetId, @ChangeDescription, @ChangedParameter, @OldValue, @NewValue,
                     @ChangeSourceUserId, @AuthorizationStatus, @DetectedAtUtc, @PriorRecordHash, @RecordHash, @CreatedAtUtc, @CreatedBy);
                END TRY
                BEGIN CATCH
                    THROW;
                END CATCH
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [CyberPhysicalSecurity].[usp_ControlChangeEvent_ReclassifyAuthorization]
                @ControlChangeEventId UNIQUEIDENTIFIER,
                @TenantId             UNIQUEIDENTIFIER,
                @AuthorizationStatus  NVARCHAR(20)
            AS
            SET NOCOUNT ON;
            BEGIN
                BEGIN TRY
                    UPDATE [CyberPhysicalSecurity].[ControlChangeEvent]
                    SET AuthorizationStatus = @AuthorizationStatus
                    WHERE ControlChangeEventId = @ControlChangeEventId AND TenantId = @TenantId;
                END TRY
                BEGIN CATCH
                    THROW;
                END CATCH
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [CyberPhysicalSecurity].[usp_ControlChangeEvent_GetById]
                @ControlChangeEventId UNIQUEIDENTIFIER,
                @TenantId             UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT ControlChangeEventId, TenantId, SiteId, OtAssetId, ChangeDescription, ChangedParameter, OldValue, NewValue,
                       ChangeSourceUserId, AuthorizationStatus, DetectedAtUtc, PriorRecordHash, RecordHash, CreatedAtUtc, CreatedBy
                FROM [CyberPhysicalSecurity].[ControlChangeEvent]
                WHERE ControlChangeEventId = @ControlChangeEventId AND TenantId = @TenantId;
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [CyberPhysicalSecurity].[usp_ControlChangeEvent_GetLatestForAsset]
                @OtAssetId UNIQUEIDENTIFIER,
                @TenantId  UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT TOP (1) ControlChangeEventId, TenantId, SiteId, OtAssetId, ChangeDescription, ChangedParameter, OldValue, NewValue,
                       ChangeSourceUserId, AuthorizationStatus, DetectedAtUtc, PriorRecordHash, RecordHash, CreatedAtUtc, CreatedBy
                FROM [CyberPhysicalSecurity].[ControlChangeEvent]
                WHERE OtAssetId = @OtAssetId AND TenantId = @TenantId
                ORDER BY DetectedAtUtc DESC, CreatedAtUtc DESC;
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [CyberPhysicalSecurity].[usp_ControlChangeEvent_GetByAuthorizationStatus]
                @TenantId            UNIQUEIDENTIFIER,
                @AuthorizationStatus NVARCHAR(20)
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT ControlChangeEventId, TenantId, SiteId, OtAssetId, ChangeDescription, ChangedParameter, OldValue, NewValue,
                       ChangeSourceUserId, AuthorizationStatus, DetectedAtUtc, PriorRecordHash, RecordHash, CreatedAtUtc, CreatedBy
                FROM [CyberPhysicalSecurity].[ControlChangeEvent]
                WHERE TenantId = @TenantId AND AuthorizationStatus = @AuthorizationStatus
                ORDER BY DetectedAtUtc DESC;
            END
            """);

        // ---- Table: SensorIntegrityCheck (FK -> OtAsset) ---------------------------
        await conn.ExecuteAsync("""
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'SensorIntegrityCheck' AND schema_id = SCHEMA_ID('CyberPhysicalSecurity'))
            BEGIN
                CREATE TABLE [CyberPhysicalSecurity].[SensorIntegrityCheck]
                (
                    SensorIntegrityCheckId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_SensorIntegrityCheck PRIMARY KEY,
                    TenantId                UNIQUEIDENTIFIER NOT NULL,
                    SiteId                  UNIQUEIDENTIFIER NOT NULL,
                    OtAssetId               UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_SensorIntegrityCheck_OtAsset REFERENCES [CyberPhysicalSecurity].[OtAsset] (OtAssetId),
                    CheckType               NVARCHAR(20)     NOT NULL,
                    Result                  NVARCHAR(20)     NOT NULL,
                    Details                 NVARCHAR(2000)   NOT NULL CONSTRAINT DF_SensorIntegrityCheck_Details DEFAULT (N''),
                    CheckedAtUtc            DATETIME2(3)     NOT NULL,
                    NextCheckDueUtc         DATETIME2(3)     NULL,
                    CreatedAtUtc            DATETIME2(3)     NOT NULL,
                    CreatedBy               UNIQUEIDENTIFIER NOT NULL
                );

                CREATE NONCLUSTERED INDEX IX_SensorIntegrityCheck_Tenant_Asset_CheckedAt ON [CyberPhysicalSecurity].[SensorIntegrityCheck] (TenantId, OtAssetId, CheckedAtUtc DESC);
                CREATE NONCLUSTERED INDEX IX_SensorIntegrityCheck_Tenant_NextDue ON [CyberPhysicalSecurity].[SensorIntegrityCheck] (TenantId, NextCheckDueUtc);
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [CyberPhysicalSecurity].[usp_SensorIntegrityCheck_Insert]
                @SensorIntegrityCheckId UNIQUEIDENTIFIER,
                @TenantId               UNIQUEIDENTIFIER,
                @SiteId                 UNIQUEIDENTIFIER,
                @OtAssetId              UNIQUEIDENTIFIER,
                @CheckType              NVARCHAR(20),
                @Result                 NVARCHAR(20),
                @Details                NVARCHAR(2000),
                @CheckedAtUtc           DATETIME2(3),
                @NextCheckDueUtc        DATETIME2(3) = NULL,
                @CreatedAtUtc           DATETIME2(3),
                @CreatedBy              UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN
                BEGIN TRY
                    INSERT INTO [CyberPhysicalSecurity].[SensorIntegrityCheck]
                    (SensorIntegrityCheckId, TenantId, SiteId, OtAssetId, CheckType, Result, Details, CheckedAtUtc, NextCheckDueUtc, CreatedAtUtc, CreatedBy)
                    VALUES
                    (@SensorIntegrityCheckId, @TenantId, @SiteId, @OtAssetId, @CheckType, @Result, @Details, @CheckedAtUtc, @NextCheckDueUtc, @CreatedAtUtc, @CreatedBy);
                END TRY
                BEGIN CATCH
                    THROW;
                END CATCH
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [CyberPhysicalSecurity].[usp_SensorIntegrityCheck_GetById]
                @SensorIntegrityCheckId UNIQUEIDENTIFIER,
                @TenantId               UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT SensorIntegrityCheckId, TenantId, SiteId, OtAssetId, CheckType, Result, Details, CheckedAtUtc, NextCheckDueUtc, CreatedAtUtc, CreatedBy
                FROM [CyberPhysicalSecurity].[SensorIntegrityCheck]
                WHERE SensorIntegrityCheckId = @SensorIntegrityCheckId AND TenantId = @TenantId;
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [CyberPhysicalSecurity].[usp_SensorIntegrityCheck_GetHistoryForAsset]
                @OtAssetId UNIQUEIDENTIFIER,
                @TenantId  UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT SensorIntegrityCheckId, TenantId, SiteId, OtAssetId, CheckType, Result, Details, CheckedAtUtc, NextCheckDueUtc, CreatedAtUtc, CreatedBy
                FROM [CyberPhysicalSecurity].[SensorIntegrityCheck]
                WHERE OtAssetId = @OtAssetId AND TenantId = @TenantId
                ORDER BY CheckedAtUtc DESC;
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [CyberPhysicalSecurity].[usp_SensorIntegrityCheck_GetAssetsWithChecksDue]
                @TenantId UNIQUEIDENTIFIER,
                @AsOfUtc  DATETIME2(3)
            AS
            SET NOCOUNT ON;
            BEGIN
                ;WITH LatestPerAsset AS
                (
                    SELECT OtAssetId, NextCheckDueUtc,
                           ROW_NUMBER() OVER (PARTITION BY OtAssetId ORDER BY CheckedAtUtc DESC) AS rn
                    FROM [CyberPhysicalSecurity].[SensorIntegrityCheck]
                    WHERE TenantId = @TenantId
                )
                SELECT OtAssetId
                FROM LatestPerAsset
                WHERE rn = 1 AND (NextCheckDueUtc IS NULL OR NextCheckDueUtc <= @AsOfUtc);
            END
            """);

        // ---- Table: SabotageIndicator -----------------------------------------------
        await conn.ExecuteAsync("""
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'SabotageIndicator' AND schema_id = SCHEMA_ID('CyberPhysicalSecurity'))
            BEGIN
                CREATE TABLE [CyberPhysicalSecurity].[SabotageIndicator]
                (
                    SabotageIndicatorId     UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_SabotageIndicator PRIMARY KEY,
                    TenantId                 UNIQUEIDENTIFIER NOT NULL,
                    SiteId                   UNIQUEIDENTIFIER NOT NULL,
                    Source                   NVARCHAR(30)     NOT NULL,
                    SourceReferenceId        UNIQUEIDENTIFIER NULL,
                    Description              NVARCHAR(2000)   NOT NULL CONSTRAINT DF_SabotageIndicator_Desc DEFAULT (N''),
                    Severity                 NVARCHAR(20)     NOT NULL,
                    Status                   NVARCHAR(30)     NOT NULL,
                    CorrelatedIndicatorCount INT              NOT NULL CONSTRAINT DF_SabotageIndicator_Correlated DEFAULT (1),
                    RaisedAtUtc              DATETIME2(3)     NOT NULL,
                    RiskManagementPriority   NVARCHAR(5)      NOT NULL,
                    CreatedAtUtc             DATETIME2(3)     NOT NULL,
                    CreatedBy                UNIQUEIDENTIFIER NOT NULL,
                    RowVersion               ROWVERSION       NOT NULL
                );

                CREATE NONCLUSTERED INDEX IX_SabotageIndicator_Tenant_Status ON [CyberPhysicalSecurity].[SabotageIndicator] (TenantId, Status);
                CREATE NONCLUSTERED INDEX IX_SabotageIndicator_Tenant_RaisedAt ON [CyberPhysicalSecurity].[SabotageIndicator] (TenantId, RaisedAtUtc DESC);
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [CyberPhysicalSecurity].[usp_SabotageIndicator_Insert]
                @SabotageIndicatorId      UNIQUEIDENTIFIER,
                @TenantId                 UNIQUEIDENTIFIER,
                @SiteId                   UNIQUEIDENTIFIER,
                @Source                   NVARCHAR(30),
                @SourceReferenceId        UNIQUEIDENTIFIER = NULL,
                @Description              NVARCHAR(2000),
                @Severity                 NVARCHAR(20),
                @Status                   NVARCHAR(30),
                @CorrelatedIndicatorCount INT,
                @RaisedAtUtc              DATETIME2(3),
                @RiskManagementPriority   NVARCHAR(5),
                @CreatedAtUtc             DATETIME2(3),
                @CreatedBy                UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN
                BEGIN TRY
                    INSERT INTO [CyberPhysicalSecurity].[SabotageIndicator]
                    (SabotageIndicatorId, TenantId, SiteId, Source, SourceReferenceId, Description, Severity, Status,
                     CorrelatedIndicatorCount, RaisedAtUtc, RiskManagementPriority, CreatedAtUtc, CreatedBy)
                    VALUES
                    (@SabotageIndicatorId, @TenantId, @SiteId, @Source, @SourceReferenceId, @Description, @Severity, @Status,
                     @CorrelatedIndicatorCount, @RaisedAtUtc, @RiskManagementPriority, @CreatedAtUtc, @CreatedBy);
                END TRY
                BEGIN CATCH
                    THROW;
                END CATCH
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [CyberPhysicalSecurity].[usp_SabotageIndicator_Update]
                @SabotageIndicatorId      UNIQUEIDENTIFIER,
                @TenantId                 UNIQUEIDENTIFIER,
                @SiteId                   UNIQUEIDENTIFIER,
                @Source                   NVARCHAR(30),
                @SourceReferenceId        UNIQUEIDENTIFIER = NULL,
                @Description              NVARCHAR(2000),
                @Severity                 NVARCHAR(20),
                @Status                   NVARCHAR(30),
                @CorrelatedIndicatorCount INT,
                @RaisedAtUtc              DATETIME2(3),
                @RiskManagementPriority   NVARCHAR(5),
                @CreatedAtUtc             DATETIME2(3),
                @CreatedBy                UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN
                BEGIN TRY
                    UPDATE [CyberPhysicalSecurity].[SabotageIndicator]
                    SET Severity = @Severity,
                        Status = @Status,
                        CorrelatedIndicatorCount = @CorrelatedIndicatorCount,
                        RiskManagementPriority = @RiskManagementPriority
                    WHERE SabotageIndicatorId = @SabotageIndicatorId AND TenantId = @TenantId;
                END TRY
                BEGIN CATCH
                    THROW;
                END CATCH
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [CyberPhysicalSecurity].[usp_SabotageIndicator_GetById]
                @SabotageIndicatorId UNIQUEIDENTIFIER,
                @TenantId            UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT SabotageIndicatorId, TenantId, SiteId, Source, SourceReferenceId, Description, Severity, Status,
                       CorrelatedIndicatorCount, RaisedAtUtc, RiskManagementPriority, CreatedAtUtc, CreatedBy
                FROM [CyberPhysicalSecurity].[SabotageIndicator]
                WHERE SabotageIndicatorId = @SabotageIndicatorId AND TenantId = @TenantId;
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [CyberPhysicalSecurity].[usp_SabotageIndicator_GetActive]
                @TenantId UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT SabotageIndicatorId, TenantId, SiteId, Source, SourceReferenceId, Description, Severity, Status,
                       CorrelatedIndicatorCount, RaisedAtUtc, RiskManagementPriority, CreatedAtUtc, CreatedBy
                FROM [CyberPhysicalSecurity].[SabotageIndicator]
                WHERE TenantId = @TenantId AND Status IN (N'Open', N'UnderInvestigation')
                ORDER BY RaisedAtUtc DESC;
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [CyberPhysicalSecurity].[usp_SabotageIndicator_GetForCorrelationWindow]
                @TenantId       UNIQUEIDENTIFIER,
                @WindowStartUtc DATETIME2(3),
                @WindowEndUtc   DATETIME2(3)
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT SabotageIndicatorId, TenantId, SiteId, Source, SourceReferenceId, Description, Severity, Status,
                       CorrelatedIndicatorCount, RaisedAtUtc, RiskManagementPriority, CreatedAtUtc, CreatedBy
                FROM [CyberPhysicalSecurity].[SabotageIndicator]
                WHERE TenantId = @TenantId
                  AND Status IN (N'Open', N'UnderInvestigation')
                  AND RaisedAtUtc BETWEEN @WindowStartUtc AND @WindowEndUtc
                ORDER BY RaisedAtUtc DESC;
            END
            """);

        // ---- Table: CyberPhysicalIncident --------------------------------------------
        await conn.ExecuteAsync("""
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CyberPhysicalIncident' AND schema_id = SCHEMA_ID('CyberPhysicalSecurity'))
            BEGIN
                CREATE TABLE [CyberPhysicalSecurity].[CyberPhysicalIncident]
                (
                    CyberPhysicalIncidentId    UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_CyberPhysicalIncident PRIMARY KEY,
                    TenantId                     UNIQUEIDENTIFIER NOT NULL,
                    SiteId                       UNIQUEIDENTIFIER NOT NULL,
                    Title                        NVARCHAR(256)    NOT NULL,
                    Description                  NVARCHAR(4000)   NOT NULL CONSTRAINT DF_CyberPhysicalIncident_Desc DEFAULT (N''),
                    Severity                     NVARCHAR(20)     NOT NULL,
                    Status                       NVARCHAR(20)     NOT NULL,
                    RelatedSabotageIndicatorIds  NVARCHAR(4000)   NULL,
                    OwnerId                      UNIQUEIDENTIFIER NOT NULL,
                    OpenedAtUtc                  DATETIME2(3)     NOT NULL,
                    ClosedAtUtc                  DATETIME2(3)     NULL,
                    ResponseTimeMinutes          INT              NULL,
                    CreatedAtUtc                 DATETIME2(3)     NOT NULL,
                    CreatedBy                    UNIQUEIDENTIFIER NOT NULL,
                    RowVersion                   ROWVERSION       NOT NULL
                );

                CREATE NONCLUSTERED INDEX IX_CyberPhysicalIncident_Tenant_Status ON [CyberPhysicalSecurity].[CyberPhysicalIncident] (TenantId, Status);
                CREATE NONCLUSTERED INDEX IX_CyberPhysicalIncident_Tenant_OpenedAt ON [CyberPhysicalSecurity].[CyberPhysicalIncident] (TenantId, OpenedAtUtc DESC);
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [CyberPhysicalSecurity].[usp_CyberPhysicalIncident_Insert]
                @CyberPhysicalIncidentId     UNIQUEIDENTIFIER,
                @TenantId                    UNIQUEIDENTIFIER,
                @SiteId                      UNIQUEIDENTIFIER,
                @Title                       NVARCHAR(256),
                @Description                 NVARCHAR(4000),
                @Severity                    NVARCHAR(20),
                @Status                      NVARCHAR(20),
                @RelatedSabotageIndicatorIds NVARCHAR(4000) = NULL,
                @OwnerId                     UNIQUEIDENTIFIER,
                @OpenedAtUtc                 DATETIME2(3),
                @ClosedAtUtc                 DATETIME2(3) = NULL,
                @ResponseTimeMinutes         INT = NULL,
                @CreatedAtUtc                DATETIME2(3),
                @CreatedBy                   UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN
                BEGIN TRY
                    INSERT INTO [CyberPhysicalSecurity].[CyberPhysicalIncident]
                    (CyberPhysicalIncidentId, TenantId, SiteId, Title, Description, Severity, Status, RelatedSabotageIndicatorIds,
                     OwnerId, OpenedAtUtc, ClosedAtUtc, ResponseTimeMinutes, CreatedAtUtc, CreatedBy)
                    VALUES
                    (@CyberPhysicalIncidentId, @TenantId, @SiteId, @Title, @Description, @Severity, @Status, @RelatedSabotageIndicatorIds,
                     @OwnerId, @OpenedAtUtc, @ClosedAtUtc, @ResponseTimeMinutes, @CreatedAtUtc, @CreatedBy);
                END TRY
                BEGIN CATCH
                    THROW;
                END CATCH
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [CyberPhysicalSecurity].[usp_CyberPhysicalIncident_Update]
                @CyberPhysicalIncidentId UNIQUEIDENTIFIER,
                @TenantId                UNIQUEIDENTIFIER,
                @Status                  NVARCHAR(20),
                @ClosedAtUtc             DATETIME2(3) = NULL,
                @ResponseTimeMinutes     INT = NULL
            AS
            SET NOCOUNT ON;
            BEGIN
                BEGIN TRY
                    UPDATE [CyberPhysicalSecurity].[CyberPhysicalIncident]
                    SET Status = @Status,
                        ClosedAtUtc = @ClosedAtUtc,
                        ResponseTimeMinutes = @ResponseTimeMinutes
                    WHERE CyberPhysicalIncidentId = @CyberPhysicalIncidentId AND TenantId = @TenantId;
                END TRY
                BEGIN CATCH
                    THROW;
                END CATCH
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [CyberPhysicalSecurity].[usp_CyberPhysicalIncident_GetById]
                @CyberPhysicalIncidentId UNIQUEIDENTIFIER,
                @TenantId                UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT CyberPhysicalIncidentId, TenantId, SiteId, Title, Description, Severity, Status, RelatedSabotageIndicatorIds,
                       OwnerId, OpenedAtUtc, ClosedAtUtc, ResponseTimeMinutes, CreatedAtUtc, CreatedBy
                FROM [CyberPhysicalSecurity].[CyberPhysicalIncident]
                WHERE CyberPhysicalIncidentId = @CyberPhysicalIncidentId AND TenantId = @TenantId;
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [CyberPhysicalSecurity].[usp_CyberPhysicalIncident_GetAll]
                @TenantId UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT CyberPhysicalIncidentId, TenantId, SiteId, Title, Description, Severity, Status, RelatedSabotageIndicatorIds,
                       OwnerId, OpenedAtUtc, ClosedAtUtc, ResponseTimeMinutes, CreatedAtUtc, CreatedBy
                FROM [CyberPhysicalSecurity].[CyberPhysicalIncident]
                WHERE TenantId = @TenantId
                ORDER BY OpenedAtUtc DESC;
            END
            """);
    }

    /// <summary>
    /// Creates EnvironmentalHealth.WaterDischargeCompliance schema objects (6 aggregates:
    /// DischargePermit, DischargePoint, EffluentSample, DischargeLimit, LimitExceedance,
    /// DischargeMonitoringReport) -- tables plus Create/GetById/Update stored procedures,
    /// mirrored verbatim from PlantSafety.Infrastructure\EnvironmentalHealth\WaterDischargeCompliance\*\Sql\
    /// (embeds schema rather than executing the on-disk Scripts folder, matching every other
    /// ApplyXSchemaAsync method above). GetAll/GetItems procedures are intentionally omitted --
    /// list query handlers are covered at the unit-test layer with fakes, not DB round-trips.
    /// </summary>
    public static async Task ApplyWaterDischargeComplianceSchemaAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken);

        // ---- DischargePermit ------------------------------------------------------------------
        await conn.ExecuteAsync("""
            IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'EnvironmentalHealth')
            BEGIN
                EXEC('CREATE SCHEMA [EnvironmentalHealth]');
            END
            
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'DischargePermit' AND schema_id = SCHEMA_ID('EnvironmentalHealth'))
            BEGIN
                CREATE TABLE [EnvironmentalHealth].[DischargePermit] (
                    [DischargePermitId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                    [TenantId] UNIQUEIDENTIFIER NOT NULL,
                    [SiteId] UNIQUEIDENTIFIER NULL,
                    [Status] NVARCHAR(50) NOT NULL,
                    [PermitNumber] NVARCHAR(100) NOT NULL,
                    [RegulatoryProgram] NVARCHAR(50) NOT NULL,
                    [IssuingAuthority] NVARCHAR(200) NOT NULL,
                    [EffectiveAtUtc] DATETIME2(3) NOT NULL,
                    [ExpiresAtUtc] DATETIME2(3) NOT NULL,
                    [RenewalCount] INT NOT NULL CONSTRAINT DF_DischargePermit_RenewalCount DEFAULT (0),
                    [PermitConditionSummary] NVARCHAR(1000) NOT NULL CONSTRAINT DF_DischargePermit_PermitConditionSummary DEFAULT (N''),
                    [LifecycleActionReason] NVARCHAR(500) NULL,
                    [CreatedAtUtc] DATETIME2(3) NOT NULL,
                    [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
                    [IsDeleted] BIT NOT NULL CONSTRAINT DF_DischargePermit_IsDeleted DEFAULT (0),
                    [ModifiedAtUtc] DATETIME2(3) NULL,
                    [ModifiedBy] UNIQUEIDENTIFIER NULL,
                    [CorrelationId] UNIQUEIDENTIFIER NULL,
                    [ActorId] UNIQUEIDENTIFIER NULL
                );
            
                CREATE NONCLUSTERED INDEX [IX_DischargePermit_TenantId_SiteId]
                    ON [EnvironmentalHealth].[DischargePermit] ([TenantId], [SiteId])
                    INCLUDE ([Status], [PermitNumber], [ExpiresAtUtc]);
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [EnvironmentalHealth].[usp_DischargePermit_Create]
                @CorrelationId UNIQUEIDENTIFIER = NULL,
                @ActorId       UNIQUEIDENTIFIER = NULL,
                @DischargePermitId UNIQUEIDENTIFIER,
                @TenantId UNIQUEIDENTIFIER,
                @SiteId UNIQUEIDENTIFIER = NULL,
                @Status NVARCHAR(50),
                @PermitNumber NVARCHAR(100),
                @RegulatoryProgram NVARCHAR(50),
                @IssuingAuthority NVARCHAR(200),
                @EffectiveAtUtc DATETIME2(3),
                @ExpiresAtUtc DATETIME2(3),
                @RenewalCount INT,
                @PermitConditionSummary NVARCHAR(1000),
                @LifecycleActionReason NVARCHAR(500) = NULL,
                @CreatedAtUtc DATETIME2(3),
                @CreatedBy UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN TRY
                BEGIN TRANSACTION;
                    INSERT INTO [EnvironmentalHealth].[DischargePermit]
                        ([DischargePermitId], [TenantId], [SiteId], [Status], [PermitNumber], [RegulatoryProgram], [IssuingAuthority],
                         [EffectiveAtUtc], [ExpiresAtUtc], [RenewalCount], [PermitConditionSummary], [LifecycleActionReason],
                         [CreatedAtUtc], [CreatedBy], [CorrelationId], [ActorId])
                    VALUES
                        (@DischargePermitId, @TenantId, @SiteId, @Status, @PermitNumber, @RegulatoryProgram, @IssuingAuthority,
                         @EffectiveAtUtc, @ExpiresAtUtc, @RenewalCount, @PermitConditionSummary, @LifecycleActionReason,
                         @CreatedAtUtc, @CreatedBy, @CorrelationId, @ActorId);
                COMMIT TRANSACTION;
            END TRY
            BEGIN CATCH
                IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
                THROW;
            END CATCH;
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [EnvironmentalHealth].[usp_DischargePermit_GetById]
            (
                @DischargePermitId UNIQUEIDENTIFIER
            )
            AS
            BEGIN
                SET NOCOUNT ON;
            
                SELECT [DischargePermitId], [TenantId], [SiteId], [Status], [PermitNumber], [RegulatoryProgram], [IssuingAuthority],
                       [EffectiveAtUtc], [ExpiresAtUtc], [RenewalCount], [PermitConditionSummary], [LifecycleActionReason],
                       [CreatedAtUtc], [CreatedBy]
                FROM [EnvironmentalHealth].[DischargePermit]
                WHERE [DischargePermitId] = @DischargePermitId
                  AND ISNULL([IsDeleted], 0) = 0;
            END;
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [EnvironmentalHealth].[usp_DischargePermit_Update]
                @CorrelationId UNIQUEIDENTIFIER = NULL,
                @ActorId       UNIQUEIDENTIFIER = NULL,
                @NowUtc        DATETIME2(3),
                @DischargePermitId UNIQUEIDENTIFIER,
                @TenantId UNIQUEIDENTIFIER,
                @SiteId UNIQUEIDENTIFIER = NULL,
                @Status NVARCHAR(50),
                @PermitNumber NVARCHAR(100),
                @RegulatoryProgram NVARCHAR(50),
                @IssuingAuthority NVARCHAR(200),
                @EffectiveAtUtc DATETIME2(3),
                @ExpiresAtUtc DATETIME2(3),
                @RenewalCount INT,
                @PermitConditionSummary NVARCHAR(1000),
                @LifecycleActionReason NVARCHAR(500) = NULL
            AS
            SET NOCOUNT ON;
            BEGIN TRY
                BEGIN TRANSACTION;
                    UPDATE [EnvironmentalHealth].[DischargePermit]
                    SET
                        [SiteId] = @SiteId,
                        [Status] = @Status,
                        [ExpiresAtUtc] = @ExpiresAtUtc,
                        [RenewalCount] = @RenewalCount,
                        [LifecycleActionReason] = @LifecycleActionReason,
                        [ModifiedAtUtc] = @NowUtc,
                        [ModifiedBy] = @ActorId,
                        [CorrelationId] = @CorrelationId
                    WHERE [DischargePermitId] = @DischargePermitId
                      AND [TenantId] = @TenantId
                      AND ISNULL([IsDeleted], 0) = 0;
                COMMIT TRANSACTION;
            END TRY
            BEGIN CATCH
                IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
                THROW;
            END CATCH;
            """);

        // ---- DischargePoint ------------------------------------------------------------------
        await conn.ExecuteAsync("""
            IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'EnvironmentalHealth')
            BEGIN
                EXEC('CREATE SCHEMA [EnvironmentalHealth]');
            END
            
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'DischargePoint' AND schema_id = SCHEMA_ID('EnvironmentalHealth'))
            BEGIN
                CREATE TABLE [EnvironmentalHealth].[DischargePoint] (
                    [DischargePointId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                    [TenantId] UNIQUEIDENTIFIER NOT NULL,
                    [SiteId] UNIQUEIDENTIFIER NULL,
                    [Status] NVARCHAR(50) NOT NULL,
                    [DischargePermitId] UNIQUEIDENTIFIER NOT NULL,
                    [PointCode] NVARCHAR(50) NOT NULL,
                    [PointName] NVARCHAR(200) NOT NULL,
                    [ReceivingWaterBody] NVARCHAR(200) NOT NULL,
                    [Outfall] NVARCHAR(50) NOT NULL,
                    [Latitude] DECIMAL(9,6) NULL,
                    [Longitude] DECIMAL(9,6) NULL,
                    [CreatedAtUtc] DATETIME2(3) NOT NULL,
                    [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
                    [IsDeleted] BIT NOT NULL CONSTRAINT DF_DischargePoint_IsDeleted DEFAULT (0),
                    [ModifiedAtUtc] DATETIME2(3) NULL,
                    [ModifiedBy] UNIQUEIDENTIFIER NULL,
                    [CorrelationId] UNIQUEIDENTIFIER NULL,
                    [ActorId] UNIQUEIDENTIFIER NULL
                );
            
                CREATE NONCLUSTERED INDEX [IX_DischargePoint_TenantId_SiteId]
                    ON [EnvironmentalHealth].[DischargePoint] ([TenantId], [SiteId])
                    INCLUDE ([Status], [PointCode]);
                CREATE NONCLUSTERED INDEX [IX_DischargePoint_DischargePermitId]
                    ON [EnvironmentalHealth].[DischargePoint] ([DischargePermitId]);
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [EnvironmentalHealth].[usp_DischargePoint_Create]
                @CorrelationId UNIQUEIDENTIFIER = NULL,
                @ActorId       UNIQUEIDENTIFIER = NULL,
                @DischargePointId UNIQUEIDENTIFIER,
                @TenantId UNIQUEIDENTIFIER,
                @SiteId UNIQUEIDENTIFIER = NULL,
                @Status NVARCHAR(50),
                @DischargePermitId UNIQUEIDENTIFIER,
                @PointCode NVARCHAR(50),
                @PointName NVARCHAR(200),
                @ReceivingWaterBody NVARCHAR(200),
                @Outfall NVARCHAR(50),
                @Latitude DECIMAL(9,6) = NULL,
                @Longitude DECIMAL(9,6) = NULL,
                @CreatedAtUtc DATETIME2(3),
                @CreatedBy UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN TRY
                BEGIN TRANSACTION;
                    INSERT INTO [EnvironmentalHealth].[DischargePoint]
                        ([DischargePointId], [TenantId], [SiteId], [Status], [DischargePermitId], [PointCode], [PointName],
                         [ReceivingWaterBody], [Outfall], [Latitude], [Longitude], [CreatedAtUtc], [CreatedBy], [CorrelationId], [ActorId])
                    VALUES
                        (@DischargePointId, @TenantId, @SiteId, @Status, @DischargePermitId, @PointCode, @PointName,
                         @ReceivingWaterBody, @Outfall, @Latitude, @Longitude, @CreatedAtUtc, @CreatedBy, @CorrelationId, @ActorId);
                COMMIT TRANSACTION;
            END TRY
            BEGIN CATCH
                IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
                THROW;
            END CATCH;
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [EnvironmentalHealth].[usp_DischargePoint_GetById]
            (
                @DischargePointId UNIQUEIDENTIFIER
            )
            AS
            BEGIN
                SET NOCOUNT ON;
            
                SELECT [DischargePointId], [TenantId], [SiteId], [Status], [DischargePermitId], [PointCode], [PointName],
                       [ReceivingWaterBody], [Outfall], [Latitude], [Longitude], [CreatedAtUtc], [CreatedBy]
                FROM [EnvironmentalHealth].[DischargePoint]
                WHERE [DischargePointId] = @DischargePointId
                  AND ISNULL([IsDeleted], 0) = 0;
            END;
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [EnvironmentalHealth].[usp_DischargePoint_Update]
                @CorrelationId UNIQUEIDENTIFIER = NULL,
                @ActorId       UNIQUEIDENTIFIER = NULL,
                @NowUtc        DATETIME2(3),
                @DischargePointId UNIQUEIDENTIFIER,
                @TenantId UNIQUEIDENTIFIER,
                @SiteId UNIQUEIDENTIFIER = NULL,
                @Status NVARCHAR(50),
                @PointName NVARCHAR(200),
                @ReceivingWaterBody NVARCHAR(200)
            AS
            SET NOCOUNT ON;
            BEGIN TRY
                BEGIN TRANSACTION;
                    UPDATE [EnvironmentalHealth].[DischargePoint]
                    SET
                        [SiteId] = @SiteId,
                        [Status] = @Status,
                        [PointName] = @PointName,
                        [ReceivingWaterBody] = @ReceivingWaterBody,
                        [ModifiedAtUtc] = @NowUtc,
                        [ModifiedBy] = @ActorId,
                        [CorrelationId] = @CorrelationId
                    WHERE [DischargePointId] = @DischargePointId
                      AND [TenantId] = @TenantId
                      AND ISNULL([IsDeleted], 0) = 0;
                COMMIT TRANSACTION;
            END TRY
            BEGIN CATCH
                IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
                THROW;
            END CATCH;
            """);

        // ---- EffluentSample ------------------------------------------------------------------
        await conn.ExecuteAsync("""
            IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'EnvironmentalHealth')
            BEGIN
                EXEC('CREATE SCHEMA [EnvironmentalHealth]');
            END
            
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'EffluentSample' AND schema_id = SCHEMA_ID('EnvironmentalHealth'))
            BEGIN
                CREATE TABLE [EnvironmentalHealth].[EffluentSample] (
                    [EffluentSampleId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                    [TenantId] UNIQUEIDENTIFIER NOT NULL,
                    [SiteId] UNIQUEIDENTIFIER NULL,
                    [Status] NVARCHAR(50) NOT NULL,
                    [DischargePointId] UNIQUEIDENTIFIER NOT NULL,
                    [Parameter] NVARCHAR(50) NOT NULL,
                    [MeasuredValue] DECIMAL(18,4) NOT NULL,
                    [UnitOfMeasure] NVARCHAR(50) NOT NULL,
                    [SampledAtUtc] DATETIME2(3) NOT NULL,
                    [CollectedByUserId] UNIQUEIDENTIFIER NOT NULL,
                    [LabSourceSystem] NVARCHAR(100) NOT NULL,
                    [ChainOfCustodyReference] NVARCHAR(200) NOT NULL,
                    [CorrectionOfSampleId] UNIQUEIDENTIFIER NULL,
                    [CorrectionReason] NVARCHAR(500) NULL,
                    [CreatedAtUtc] DATETIME2(3) NOT NULL,
                    [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
                    [IsDeleted] BIT NOT NULL CONSTRAINT DF_EffluentSample_IsDeleted DEFAULT (0),
                    [ModifiedAtUtc] DATETIME2(3) NULL,
                    [ModifiedBy] UNIQUEIDENTIFIER NULL,
                    [CorrelationId] UNIQUEIDENTIFIER NULL,
                    [ActorId] UNIQUEIDENTIFIER NULL
                );
            
                CREATE NONCLUSTERED INDEX [IX_EffluentSample_TenantId_DischargePointId]
                    ON [EnvironmentalHealth].[EffluentSample] ([TenantId], [DischargePointId], [SampledAtUtc]);
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [EnvironmentalHealth].[usp_EffluentSample_Create]
                @CorrelationId UNIQUEIDENTIFIER = NULL,
                @ActorId       UNIQUEIDENTIFIER = NULL,
                @EffluentSampleId UNIQUEIDENTIFIER,
                @TenantId UNIQUEIDENTIFIER,
                @SiteId UNIQUEIDENTIFIER = NULL,
                @Status NVARCHAR(50),
                @DischargePointId UNIQUEIDENTIFIER,
                @Parameter NVARCHAR(50),
                @MeasuredValue DECIMAL(18,4),
                @UnitOfMeasure NVARCHAR(50),
                @SampledAtUtc DATETIME2(3),
                @CollectedByUserId UNIQUEIDENTIFIER,
                @LabSourceSystem NVARCHAR(100),
                @ChainOfCustodyReference NVARCHAR(200),
                @CorrectionOfSampleId UNIQUEIDENTIFIER = NULL,
                @CorrectionReason NVARCHAR(500) = NULL,
                @CreatedAtUtc DATETIME2(3),
                @CreatedBy UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN TRY
                BEGIN TRANSACTION;
                    INSERT INTO [EnvironmentalHealth].[EffluentSample]
                        ([EffluentSampleId], [TenantId], [SiteId], [Status], [DischargePointId], [Parameter], [MeasuredValue],
                         [UnitOfMeasure], [SampledAtUtc], [CollectedByUserId], [LabSourceSystem], [ChainOfCustodyReference],
                         [CorrectionOfSampleId], [CorrectionReason], [CreatedAtUtc], [CreatedBy], [CorrelationId], [ActorId])
                    VALUES
                        (@EffluentSampleId, @TenantId, @SiteId, @Status, @DischargePointId, @Parameter, @MeasuredValue,
                         @UnitOfMeasure, @SampledAtUtc, @CollectedByUserId, @LabSourceSystem, @ChainOfCustodyReference,
                         @CorrectionOfSampleId, @CorrectionReason, @CreatedAtUtc, @CreatedBy, @CorrelationId, @ActorId);
                COMMIT TRANSACTION;
            END TRY
            BEGIN CATCH
                IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
                THROW;
            END CATCH;
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [EnvironmentalHealth].[usp_EffluentSample_GetById]
            (
                @EffluentSampleId UNIQUEIDENTIFIER
            )
            AS
            BEGIN
                SET NOCOUNT ON;
            
                SELECT [EffluentSampleId], [TenantId], [SiteId], [Status], [DischargePointId], [Parameter], [MeasuredValue],
                       [UnitOfMeasure], [SampledAtUtc], [CollectedByUserId], [LabSourceSystem], [ChainOfCustodyReference],
                       [CorrectionOfSampleId], [CorrectionReason], [CreatedAtUtc], [CreatedBy]
                FROM [EnvironmentalHealth].[EffluentSample]
                WHERE [EffluentSampleId] = @EffluentSampleId
                  AND ISNULL([IsDeleted], 0) = 0;
            END;
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [EnvironmentalHealth].[usp_EffluentSample_Update]
                @CorrelationId UNIQUEIDENTIFIER = NULL,
                @ActorId       UNIQUEIDENTIFIER = NULL,
                @NowUtc        DATETIME2(3),
                @EffluentSampleId UNIQUEIDENTIFIER,
                @TenantId UNIQUEIDENTIFIER,
                @Status NVARCHAR(50)
            AS
            SET NOCOUNT ON;
            BEGIN TRY
                BEGIN TRANSACTION;
                    UPDATE [EnvironmentalHealth].[EffluentSample]
                    SET
                        [Status] = @Status,
                        [ModifiedAtUtc] = @NowUtc,
                        [ModifiedBy] = @ActorId,
                        [CorrelationId] = @CorrelationId
                    WHERE [EffluentSampleId] = @EffluentSampleId
                      AND [TenantId] = @TenantId
                      AND ISNULL([IsDeleted], 0) = 0;
                COMMIT TRANSACTION;
            END TRY
            BEGIN CATCH
                IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
                THROW;
            END CATCH;
            """);

        // ---- DischargeLimit ------------------------------------------------------------------
        await conn.ExecuteAsync("""
            IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'EnvironmentalHealth')
            BEGIN
                EXEC('CREATE SCHEMA [EnvironmentalHealth]');
            END
            
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'DischargeLimit' AND schema_id = SCHEMA_ID('EnvironmentalHealth'))
            BEGIN
                CREATE TABLE [EnvironmentalHealth].[DischargeLimit] (
                    [DischargeLimitId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                    [TenantId] UNIQUEIDENTIFIER NOT NULL,
                    [SiteId] UNIQUEIDENTIFIER NULL,
                    [Status] NVARCHAR(50) NOT NULL,
                    [DischargePointId] UNIQUEIDENTIFIER NOT NULL,
                    [DischargePermitId] UNIQUEIDENTIFIER NOT NULL,
                    [Parameter] NVARCHAR(50) NOT NULL,
                    [LimitBasis] NVARCHAR(50) NOT NULL,
                    [LimitValue] DECIMAL(18,4) NOT NULL,
                    [UnitOfMeasure] NVARCHAR(50) NOT NULL,
                    [EffectiveAtUtc] DATETIME2(3) NOT NULL,
                    [PermitConditionReference] NVARCHAR(200) NOT NULL,
                    [RevisionCount] INT NOT NULL CONSTRAINT DF_DischargeLimit_RevisionCount DEFAULT (0),
                    [CreatedAtUtc] DATETIME2(3) NOT NULL,
                    [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
                    [IsDeleted] BIT NOT NULL CONSTRAINT DF_DischargeLimit_IsDeleted DEFAULT (0),
                    [ModifiedAtUtc] DATETIME2(3) NULL,
                    [ModifiedBy] UNIQUEIDENTIFIER NULL,
                    [CorrelationId] UNIQUEIDENTIFIER NULL,
                    [ActorId] UNIQUEIDENTIFIER NULL
                );
            
                CREATE NONCLUSTERED INDEX [IX_DischargeLimit_TenantId_DischargePointId]
                    ON [EnvironmentalHealth].[DischargeLimit] ([TenantId], [DischargePointId])
                    INCLUDE ([Status], [Parameter]);
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [EnvironmentalHealth].[usp_DischargeLimit_Create]
                @CorrelationId UNIQUEIDENTIFIER = NULL,
                @ActorId       UNIQUEIDENTIFIER = NULL,
                @DischargeLimitId UNIQUEIDENTIFIER,
                @TenantId UNIQUEIDENTIFIER,
                @SiteId UNIQUEIDENTIFIER = NULL,
                @Status NVARCHAR(50),
                @DischargePointId UNIQUEIDENTIFIER,
                @DischargePermitId UNIQUEIDENTIFIER,
                @Parameter NVARCHAR(50),
                @LimitBasis NVARCHAR(50),
                @LimitValue DECIMAL(18,4),
                @UnitOfMeasure NVARCHAR(50),
                @EffectiveAtUtc DATETIME2(3),
                @PermitConditionReference NVARCHAR(200),
                @RevisionCount INT,
                @CreatedAtUtc DATETIME2(3),
                @CreatedBy UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN TRY
                BEGIN TRANSACTION;
                    INSERT INTO [EnvironmentalHealth].[DischargeLimit]
                        ([DischargeLimitId], [TenantId], [SiteId], [Status], [DischargePointId], [DischargePermitId], [Parameter],
                         [LimitBasis], [LimitValue], [UnitOfMeasure], [EffectiveAtUtc], [PermitConditionReference], [RevisionCount],
                         [CreatedAtUtc], [CreatedBy], [CorrelationId], [ActorId])
                    VALUES
                        (@DischargeLimitId, @TenantId, @SiteId, @Status, @DischargePointId, @DischargePermitId, @Parameter,
                         @LimitBasis, @LimitValue, @UnitOfMeasure, @EffectiveAtUtc, @PermitConditionReference, @RevisionCount,
                         @CreatedAtUtc, @CreatedBy, @CorrelationId, @ActorId);
                COMMIT TRANSACTION;
            END TRY
            BEGIN CATCH
                IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
                THROW;
            END CATCH;
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [EnvironmentalHealth].[usp_DischargeLimit_GetById]
            (
                @DischargeLimitId UNIQUEIDENTIFIER
            )
            AS
            BEGIN
                SET NOCOUNT ON;
            
                SELECT [DischargeLimitId], [TenantId], [SiteId], [Status], [DischargePointId], [DischargePermitId], [Parameter],
                       [LimitBasis], [LimitValue], [UnitOfMeasure], [EffectiveAtUtc], [PermitConditionReference], [RevisionCount],
                       [CreatedAtUtc], [CreatedBy]
                FROM [EnvironmentalHealth].[DischargeLimit]
                WHERE [DischargeLimitId] = @DischargeLimitId
                  AND ISNULL([IsDeleted], 0) = 0;
            END;
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [EnvironmentalHealth].[usp_DischargeLimit_Update]
                @CorrelationId UNIQUEIDENTIFIER = NULL,
                @ActorId       UNIQUEIDENTIFIER = NULL,
                @NowUtc        DATETIME2(3),
                @DischargeLimitId UNIQUEIDENTIFIER,
                @TenantId UNIQUEIDENTIFIER,
                @Status NVARCHAR(50),
                @LimitValue DECIMAL(18,4),
                @RevisionCount INT
            AS
            SET NOCOUNT ON;
            BEGIN TRY
                BEGIN TRANSACTION;
                    UPDATE [EnvironmentalHealth].[DischargeLimit]
                    SET
                        [Status] = @Status,
                        [LimitValue] = @LimitValue,
                        [RevisionCount] = @RevisionCount,
                        [ModifiedAtUtc] = @NowUtc,
                        [ModifiedBy] = @ActorId,
                        [CorrelationId] = @CorrelationId
                    WHERE [DischargeLimitId] = @DischargeLimitId
                      AND [TenantId] = @TenantId
                      AND ISNULL([IsDeleted], 0) = 0;
                COMMIT TRANSACTION;
            END TRY
            BEGIN CATCH
                IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
                THROW;
            END CATCH;
            """);

        // ---- DischargeMonitoringReport ------------------------------------------------------------------
        await conn.ExecuteAsync("""
            IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'EnvironmentalHealth')
            BEGIN
                EXEC('CREATE SCHEMA [EnvironmentalHealth]');
            END
            
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'DischargeMonitoringReport' AND schema_id = SCHEMA_ID('EnvironmentalHealth'))
            BEGIN
                CREATE TABLE [EnvironmentalHealth].[DischargeMonitoringReport] (
                    [DischargeMonitoringReportId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                    [TenantId] UNIQUEIDENTIFIER NOT NULL,
                    [SiteId] UNIQUEIDENTIFIER NULL,
                    [Status] NVARCHAR(50) NOT NULL,
                    [DischargePermitId] UNIQUEIDENTIFIER NOT NULL,
                    [ReportingPeriodStartUtc] DATETIME2(3) NOT NULL,
                    [ReportingPeriodEndUtc] DATETIME2(3) NOT NULL,
                    [DueAtUtc] DATETIME2(3) NOT NULL,
                    [SubmittedAtUtc] DATETIME2(3) NULL,
                    [SubmittedByUserId] UNIQUEIDENTIFIER NULL,
                    [SamplesWithinLimitCount] INT NOT NULL CONSTRAINT DF_DischargeMonitoringReport_SamplesWithinLimitCount DEFAULT (0),
                    [TotalSamplesCount] INT NOT NULL CONSTRAINT DF_DischargeMonitoringReport_TotalSamplesCount DEFAULT (0),
                    [RetentionExpiresAtUtc] DATETIME2(3) NULL,
                    [RejectionReason] NVARCHAR(500) NULL,
                    [CreatedAtUtc] DATETIME2(3) NOT NULL,
                    [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
                    [IsDeleted] BIT NOT NULL CONSTRAINT DF_DischargeMonitoringReport_IsDeleted DEFAULT (0),
                    [ModifiedAtUtc] DATETIME2(3) NULL,
                    [ModifiedBy] UNIQUEIDENTIFIER NULL,
                    [CorrelationId] UNIQUEIDENTIFIER NULL,
                    [ActorId] UNIQUEIDENTIFIER NULL
                );
            
                CREATE NONCLUSTERED INDEX [IX_DischargeMonitoringReport_TenantId_Period]
                    ON [EnvironmentalHealth].[DischargeMonitoringReport] ([TenantId], [ReportingPeriodStartUtc], [ReportingPeriodEndUtc])
                    INCLUDE ([Status], [SiteId]);
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [EnvironmentalHealth].[usp_DischargeMonitoringReport_Create]
                @CorrelationId UNIQUEIDENTIFIER = NULL,
                @ActorId       UNIQUEIDENTIFIER = NULL,
                @DischargeMonitoringReportId UNIQUEIDENTIFIER,
                @TenantId UNIQUEIDENTIFIER,
                @SiteId UNIQUEIDENTIFIER = NULL,
                @Status NVARCHAR(50),
                @DischargePermitId UNIQUEIDENTIFIER,
                @ReportingPeriodStartUtc DATETIME2(3),
                @ReportingPeriodEndUtc DATETIME2(3),
                @DueAtUtc DATETIME2(3),
                @SamplesWithinLimitCount INT,
                @TotalSamplesCount INT,
                @CreatedAtUtc DATETIME2(3),
                @CreatedBy UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN TRY
                BEGIN TRANSACTION;
                    INSERT INTO [EnvironmentalHealth].[DischargeMonitoringReport]
                        ([DischargeMonitoringReportId], [TenantId], [SiteId], [Status], [DischargePermitId], [ReportingPeriodStartUtc],
                         [ReportingPeriodEndUtc], [DueAtUtc], [SamplesWithinLimitCount], [TotalSamplesCount],
                         [CreatedAtUtc], [CreatedBy], [CorrelationId], [ActorId])
                    VALUES
                        (@DischargeMonitoringReportId, @TenantId, @SiteId, @Status, @DischargePermitId, @ReportingPeriodStartUtc,
                         @ReportingPeriodEndUtc, @DueAtUtc, @SamplesWithinLimitCount, @TotalSamplesCount,
                         @CreatedAtUtc, @CreatedBy, @CorrelationId, @ActorId);
                COMMIT TRANSACTION;
            END TRY
            BEGIN CATCH
                IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
                THROW;
            END CATCH;
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [EnvironmentalHealth].[usp_DischargeMonitoringReport_GetById]
            (
                @DischargeMonitoringReportId UNIQUEIDENTIFIER
            )
            AS
            BEGIN
                SET NOCOUNT ON;
            
                SELECT [DischargeMonitoringReportId], [TenantId], [SiteId], [Status], [DischargePermitId], [ReportingPeriodStartUtc],
                       [ReportingPeriodEndUtc], [DueAtUtc], [SubmittedAtUtc], [SubmittedByUserId], [SamplesWithinLimitCount],
                       [TotalSamplesCount], [RetentionExpiresAtUtc], [RejectionReason], [CreatedAtUtc], [CreatedBy]
                FROM [EnvironmentalHealth].[DischargeMonitoringReport]
                WHERE [DischargeMonitoringReportId] = @DischargeMonitoringReportId
                  AND ISNULL([IsDeleted], 0) = 0;
            END;
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [EnvironmentalHealth].[usp_DischargeMonitoringReport_Update]
                @CorrelationId UNIQUEIDENTIFIER = NULL,
                @ActorId       UNIQUEIDENTIFIER = NULL,
                @NowUtc        DATETIME2(3),
                @DischargeMonitoringReportId UNIQUEIDENTIFIER,
                @TenantId UNIQUEIDENTIFIER,
                @Status NVARCHAR(50),
                @SubmittedAtUtc DATETIME2(3) = NULL,
                @SubmittedByUserId UNIQUEIDENTIFIER = NULL,
                @SamplesWithinLimitCount INT,
                @TotalSamplesCount INT,
                @RetentionExpiresAtUtc DATETIME2(3) = NULL,
                @RejectionReason NVARCHAR(500) = NULL
            AS
            SET NOCOUNT ON;
            BEGIN TRY
                BEGIN TRANSACTION;
                    UPDATE [EnvironmentalHealth].[DischargeMonitoringReport]
                    SET
                        [Status] = @Status,
                        [SubmittedAtUtc] = @SubmittedAtUtc,
                        [SubmittedByUserId] = @SubmittedByUserId,
                        [SamplesWithinLimitCount] = @SamplesWithinLimitCount,
                        [TotalSamplesCount] = @TotalSamplesCount,
                        [RetentionExpiresAtUtc] = @RetentionExpiresAtUtc,
                        [RejectionReason] = @RejectionReason,
                        [ModifiedAtUtc] = @NowUtc,
                        [ModifiedBy] = @ActorId,
                        [CorrelationId] = @CorrelationId
                    WHERE [DischargeMonitoringReportId] = @DischargeMonitoringReportId
                      AND [TenantId] = @TenantId
                      AND ISNULL([IsDeleted], 0) = 0;
                COMMIT TRANSACTION;
            END TRY
            BEGIN CATCH
                IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
                THROW;
            END CATCH;
            """);

        // ---- LimitExceedance ------------------------------------------------------------------
        await conn.ExecuteAsync("""
            IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'EnvironmentalHealth')
            BEGIN
                EXEC('CREATE SCHEMA [EnvironmentalHealth]');
            END
            
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'LimitExceedance' AND schema_id = SCHEMA_ID('EnvironmentalHealth'))
            BEGIN
                CREATE TABLE [EnvironmentalHealth].[LimitExceedance] (
                    [LimitExceedanceId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                    [TenantId] UNIQUEIDENTIFIER NOT NULL,
                    [SiteId] UNIQUEIDENTIFIER NULL,
                    [Status] NVARCHAR(50) NOT NULL,
                    [DischargePointId] UNIQUEIDENTIFIER NOT NULL,
                    [EffluentSampleId] UNIQUEIDENTIFIER NOT NULL,
                    [DischargeLimitId] UNIQUEIDENTIFIER NOT NULL,
                    [ParameterName] NVARCHAR(100) NOT NULL,
                    [MeasuredValue] DECIMAL(18,4) NOT NULL,
                    [LimitValue] DECIMAL(18,4) NOT NULL,
                    [ExceedancePercentage] DECIMAL(9,2) NOT NULL,
                    [Severity] NVARCHAR(50) NOT NULL,
                    [DetectedAtUtc] DATETIME2(3) NOT NULL,
                    [RepeatExceedanceCount] INT NOT NULL CONSTRAINT DF_LimitExceedance_RepeatExceedanceCount DEFAULT (0),
                    [ResponseInitiatedAtUtc] DATETIME2(3) NULL,
                    [ResponseInitiatedByUserId] UNIQUEIDENTIFIER NULL,
                    [ResponseNotes] NVARCHAR(1000) NULL,
                    [ClosedAtUtc] DATETIME2(3) NULL,
                    [ClosedByUserId] UNIQUEIDENTIFIER NULL,
                    [ResolutionNotes] NVARCHAR(1000) NULL,
                    [CreatedAtUtc] DATETIME2(3) NOT NULL,
                    [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
                    [IsDeleted] BIT NOT NULL CONSTRAINT DF_LimitExceedance_IsDeleted DEFAULT (0),
                    [ModifiedAtUtc] DATETIME2(3) NULL,
                    [ModifiedBy] UNIQUEIDENTIFIER NULL,
                    [CorrelationId] UNIQUEIDENTIFIER NULL,
                    [ActorId] UNIQUEIDENTIFIER NULL
                );
            
                CREATE NONCLUSTERED INDEX [IX_LimitExceedance_TenantId_SiteId_Status]
                    ON [EnvironmentalHealth].[LimitExceedance] ([TenantId], [SiteId], [Status]);
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [EnvironmentalHealth].[usp_LimitExceedance_Create]
                @CorrelationId UNIQUEIDENTIFIER = NULL,
                @ActorId       UNIQUEIDENTIFIER = NULL,
                @LimitExceedanceId UNIQUEIDENTIFIER,
                @TenantId UNIQUEIDENTIFIER,
                @SiteId UNIQUEIDENTIFIER = NULL,
                @Status NVARCHAR(50),
                @DischargePointId UNIQUEIDENTIFIER,
                @EffluentSampleId UNIQUEIDENTIFIER,
                @DischargeLimitId UNIQUEIDENTIFIER,
                @ParameterName NVARCHAR(100),
                @MeasuredValue DECIMAL(18,4),
                @LimitValue DECIMAL(18,4),
                @ExceedancePercentage DECIMAL(9,2),
                @Severity NVARCHAR(50),
                @DetectedAtUtc DATETIME2(3),
                @RepeatExceedanceCount INT,
                @CreatedAtUtc DATETIME2(3),
                @CreatedBy UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN TRY
                BEGIN TRANSACTION;
                    INSERT INTO [EnvironmentalHealth].[LimitExceedance]
                        ([LimitExceedanceId], [TenantId], [SiteId], [Status], [DischargePointId], [EffluentSampleId], [DischargeLimitId],
                         [ParameterName], [MeasuredValue], [LimitValue], [ExceedancePercentage], [Severity], [DetectedAtUtc],
                         [RepeatExceedanceCount], [CreatedAtUtc], [CreatedBy], [CorrelationId], [ActorId])
                    VALUES
                        (@LimitExceedanceId, @TenantId, @SiteId, @Status, @DischargePointId, @EffluentSampleId, @DischargeLimitId,
                         @ParameterName, @MeasuredValue, @LimitValue, @ExceedancePercentage, @Severity, @DetectedAtUtc,
                         @RepeatExceedanceCount, @CreatedAtUtc, @CreatedBy, @CorrelationId, @ActorId);
                COMMIT TRANSACTION;
            END TRY
            BEGIN CATCH
                IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
                THROW;
            END CATCH;
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [EnvironmentalHealth].[usp_LimitExceedance_GetById]
            (
                @LimitExceedanceId UNIQUEIDENTIFIER
            )
            AS
            BEGIN
                SET NOCOUNT ON;
            
                SELECT [LimitExceedanceId], [TenantId], [SiteId], [Status], [DischargePointId], [EffluentSampleId], [DischargeLimitId],
                       [ParameterName], [MeasuredValue], [LimitValue], [ExceedancePercentage], [Severity], [DetectedAtUtc],
                       [RepeatExceedanceCount], [ResponseInitiatedAtUtc], [ResponseInitiatedByUserId], [ResponseNotes],
                       [ClosedAtUtc], [ClosedByUserId], [ResolutionNotes], [CreatedAtUtc], [CreatedBy]
                FROM [EnvironmentalHealth].[LimitExceedance]
                WHERE [LimitExceedanceId] = @LimitExceedanceId
                  AND ISNULL([IsDeleted], 0) = 0;
            END;
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [EnvironmentalHealth].[usp_LimitExceedance_Update]
                @CorrelationId UNIQUEIDENTIFIER = NULL,
                @ActorId       UNIQUEIDENTIFIER = NULL,
                @NowUtc        DATETIME2(3),
                @LimitExceedanceId UNIQUEIDENTIFIER,
                @TenantId UNIQUEIDENTIFIER,
                @Status NVARCHAR(50),
                @ResponseInitiatedAtUtc DATETIME2(3) = NULL,
                @ResponseInitiatedByUserId UNIQUEIDENTIFIER = NULL,
                @ResponseNotes NVARCHAR(1000) = NULL,
                @ClosedAtUtc DATETIME2(3) = NULL,
                @ClosedByUserId UNIQUEIDENTIFIER = NULL,
                @ResolutionNotes NVARCHAR(1000) = NULL
            AS
            SET NOCOUNT ON;
            BEGIN TRY
                BEGIN TRANSACTION;
                    UPDATE [EnvironmentalHealth].[LimitExceedance]
                    SET
                        [Status] = @Status,
                        [ResponseInitiatedAtUtc] = @ResponseInitiatedAtUtc,
                        [ResponseInitiatedByUserId] = @ResponseInitiatedByUserId,
                        [ResponseNotes] = @ResponseNotes,
                        [ClosedAtUtc] = @ClosedAtUtc,
                        [ClosedByUserId] = @ClosedByUserId,
                        [ResolutionNotes] = @ResolutionNotes,
                        [ModifiedAtUtc] = @NowUtc,
                        [ModifiedBy] = @ActorId,
                        [CorrelationId] = @CorrelationId
                    WHERE [LimitExceedanceId] = @LimitExceedanceId
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

    /// <summary>
    /// Creates the <c>[CommunitySafety]</c> schema's five tables (CommunityContact,
    /// CommunityConcern, CommunityImpactAssessment, CommunityNotification,
    /// CommunityMeetingRecord) and their stored procedures against the Testcontainers database,
    /// mirroring <c>PlantSafety.Infrastructure\CommunitySafety\Sql\Tables\*.sql</c> and
    /// <c>...\Sql\Procedures\usp_*.sql</c> verbatim. Backs the Dapper repositories under
    /// <c>PlantSafety.Infrastructure.CommunitySafety.*.Persistence</c>.
    /// </summary>
    public static async Task ApplyCommunitySafetySchemaAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken);

        await conn.ExecuteAsync("""
            IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'CommunitySafety')
                EXEC('CREATE SCHEMA [CommunitySafety]');
            """);

        // ---- CommunityContact ------------------------------------------------------------

        await conn.ExecuteAsync("""
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CommunityContact' AND schema_id = SCHEMA_ID('CommunitySafety'))
            BEGIN
                CREATE TABLE [CommunitySafety].[CommunityContact] (
                    [CommunityContactId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                    [TenantId] UNIQUEIDENTIFIER NOT NULL,
                    [SiteId] UNIQUEIDENTIFIER NULL,
                    [Status] NVARCHAR(50) NOT NULL,
                    [DisplayName] NVARCHAR(200) NOT NULL,
                    [ContactType] NVARCHAR(50) NOT NULL,
                    [Organization] NVARCHAR(200) NULL,
                    [CommunityArea] NVARCHAR(200) NOT NULL,
                    [Email] NVARCHAR(320) NOT NULL,
                    [Phone] NVARCHAR(32) NULL,
                    [OptedInForNotifications] BIT NOT NULL CONSTRAINT DF_CommunityContact_OptedIn DEFAULT (1),
                    [ArchivedAtUtc] DATETIME2(3) NULL,
                    [ArchivalReason] NVARCHAR(1000) NULL,
                    [CreatedAtUtc] DATETIME2(3) NOT NULL,
                    [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
                    [IsDeleted] BIT NOT NULL CONSTRAINT DF_CommunityContact_IsDeleted DEFAULT (0),
                    [ModifiedAtUtc] DATETIME2(3) NULL,
                    [ModifiedBy] UNIQUEIDENTIFIER NULL,
                    [CorrelationId] UNIQUEIDENTIFIER NULL,
                    [ActorId] UNIQUEIDENTIFIER NULL
                );

                CREATE NONCLUSTERED INDEX [IX_CommunityContact_TenantId_SiteId]
                    ON [CommunitySafety].[CommunityContact] ([TenantId], [SiteId])
                    INCLUDE ([Status], [DisplayName], [ContactType]);
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [CommunitySafety].[usp_CommunityContact_Create]
                @CorrelationId UNIQUEIDENTIFIER = NULL,
                @ActorId       UNIQUEIDENTIFIER = NULL,
                @CommunityContactId UNIQUEIDENTIFIER,
                @TenantId UNIQUEIDENTIFIER,
                @SiteId UNIQUEIDENTIFIER = NULL,
                @Status NVARCHAR(50),
                @DisplayName NVARCHAR(200),
                @ContactType NVARCHAR(50),
                @Organization NVARCHAR(200) = NULL,
                @CommunityArea NVARCHAR(200),
                @Email NVARCHAR(320),
                @Phone NVARCHAR(32) = NULL,
                @OptedInForNotifications BIT,
                @CreatedAtUtc DATETIME2(3),
                @CreatedBy UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN TRY
                BEGIN TRANSACTION;
                    INSERT INTO [CommunitySafety].[CommunityContact]
                        ([CommunityContactId], [TenantId], [SiteId], [Status], [DisplayName], [ContactType], [Organization], [CommunityArea], [Email], [Phone], [OptedInForNotifications], [CreatedAtUtc], [CreatedBy], [CorrelationId], [ActorId])
                    VALUES
                        (@CommunityContactId, @TenantId, @SiteId, @Status, @DisplayName, @ContactType, @Organization, @CommunityArea, @Email, @Phone, @OptedInForNotifications, @CreatedAtUtc, @CreatedBy, @CorrelationId, @ActorId);
                COMMIT TRANSACTION;
            END TRY
            BEGIN CATCH
                IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
                THROW;
            END CATCH;
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [CommunitySafety].[usp_CommunityContact_GetById]
            (
                @CommunityContactId UNIQUEIDENTIFIER
            )
            AS
            BEGIN
                SET NOCOUNT ON;

                SELECT [CommunityContactId], [TenantId], [SiteId], [Status], [DisplayName], [ContactType], [Organization], [CommunityArea], [Email], [Phone], [OptedInForNotifications], [ArchivedAtUtc], [ArchivalReason], [CreatedAtUtc], [CreatedBy]
                FROM [CommunitySafety].[CommunityContact]
                WHERE [CommunityContactId] = @CommunityContactId
                  AND ISNULL([IsDeleted], 0) = 0;
            END;
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [CommunitySafety].[usp_CommunityContact_GetItems]
                @TenantId UNIQUEIDENTIFIER,
                @SiteId   UNIQUEIDENTIFIER = NULL,
                @FromUtc  DATETIME2(3),
                @ToUtc    DATETIME2(3)
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT
                    [CommunityContactId], [TenantId], [SiteId], [DisplayName], [ContactType], [Organization],
                    [CommunityArea], [OptedInForNotifications], [Status], [CreatedAtUtc], [CreatedBy]
                FROM [CommunitySafety].[CommunityContact]
                WHERE TenantId = @TenantId
                  AND (@SiteId IS NULL OR SiteId = @SiteId)
                  AND ISNULL([IsDeleted], 0) = 0
                  AND CreatedAtUtc >= @FromUtc
                  AND CreatedAtUtc <= @ToUtc
                ORDER BY CreatedAtUtc DESC;
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [CommunitySafety].[usp_CommunityContact_Update]
                @CorrelationId UNIQUEIDENTIFIER = NULL,
                @ActorId       UNIQUEIDENTIFIER = NULL,
                @NowUtc        DATETIME2(3),
                @CommunityContactId UNIQUEIDENTIFIER,
                @Status NVARCHAR(50),
                @OptedInForNotifications BIT,
                @ArchivedAtUtc DATETIME2(3) = NULL,
                @ArchivalReason NVARCHAR(1000) = NULL
            AS
            SET NOCOUNT ON;
            BEGIN TRY
                BEGIN TRANSACTION;
                    UPDATE [CommunitySafety].[CommunityContact]
                    SET
                        [Status] = @Status,
                        [OptedInForNotifications] = @OptedInForNotifications,
                        [ArchivedAtUtc] = @ArchivedAtUtc,
                        [ArchivalReason] = @ArchivalReason,
                        [ModifiedAtUtc] = @NowUtc,
                        [ModifiedBy] = @ActorId,
                        [CorrelationId] = @CorrelationId
                    WHERE [CommunityContactId] = @CommunityContactId
                      AND ISNULL([IsDeleted], 0) = 0;
                COMMIT TRANSACTION;
            END TRY
            BEGIN CATCH
                IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
                THROW;
            END CATCH;
            """);

        // ---- CommunityConcern -------------------------------------------------------------

        await conn.ExecuteAsync("""
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CommunityConcern' AND schema_id = SCHEMA_ID('CommunitySafety'))
            BEGIN
                CREATE TABLE [CommunitySafety].[CommunityConcern] (
                    [CommunityConcernId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                    [TenantId] UNIQUEIDENTIFIER NOT NULL,
                    [SiteId] UNIQUEIDENTIFIER NOT NULL,
                    [Status] NVARCHAR(50) NOT NULL,
                    [Category] NVARCHAR(50) NOT NULL,
                    [Severity] NVARCHAR(30) NOT NULL,
                    [FacilityArea] NVARCHAR(200) NOT NULL,
                    [Description] NVARCHAR(4000) NOT NULL,
                    [ReportedByContactId] UNIQUEIDENTIFIER NULL,
                    [RelatedIncidentId] UNIQUEIDENTIFIER NULL,
                    [LoggedAtUtc] DATETIME2(3) NOT NULL,
                    [EscalationCount] INT NOT NULL CONSTRAINT DF_CommunityConcern_EscalationCount DEFAULT (0),
                    [EscalationReason] NVARCHAR(1000) NULL,
                    [EscalatedAtUtc] DATETIME2(3) NULL,
                    [ResolutionNotes] NVARCHAR(4000) NULL,
                    [ResolvedAtUtc] DATETIME2(3) NULL,
                    [CreatedAtUtc] DATETIME2(3) NOT NULL,
                    [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
                    [IsDeleted] BIT NOT NULL CONSTRAINT DF_CommunityConcern_IsDeleted DEFAULT (0),
                    [ModifiedAtUtc] DATETIME2(3) NULL,
                    [ModifiedBy] UNIQUEIDENTIFIER NULL,
                    [CorrelationId] UNIQUEIDENTIFIER NULL,
                    [ActorId] UNIQUEIDENTIFIER NULL
                );

                CREATE NONCLUSTERED INDEX [IX_CommunityConcern_TenantId_SiteId]
                    ON [CommunitySafety].[CommunityConcern] ([TenantId], [SiteId])
                    INCLUDE ([Status], [Category], [Severity], [FacilityArea]);

                CREATE NONCLUSTERED INDEX [IX_CommunityConcern_FacilityArea_LoggedAtUtc]
                    ON [CommunitySafety].[CommunityConcern] ([TenantId], [FacilityArea], [LoggedAtUtc])
                    INCLUDE ([Status]);
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [CommunitySafety].[usp_CommunityConcern_Create]
                @CorrelationId UNIQUEIDENTIFIER = NULL,
                @ActorId       UNIQUEIDENTIFIER = NULL,
                @CommunityConcernId UNIQUEIDENTIFIER,
                @TenantId UNIQUEIDENTIFIER,
                @SiteId UNIQUEIDENTIFIER,
                @Status NVARCHAR(50),
                @Category NVARCHAR(50),
                @Severity NVARCHAR(30),
                @FacilityArea NVARCHAR(200),
                @Description NVARCHAR(4000),
                @ReportedByContactId UNIQUEIDENTIFIER = NULL,
                @RelatedIncidentId UNIQUEIDENTIFIER = NULL,
                @LoggedAtUtc DATETIME2(3),
                @CreatedAtUtc DATETIME2(3),
                @CreatedBy UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN TRY
                BEGIN TRANSACTION;
                    INSERT INTO [CommunitySafety].[CommunityConcern]
                        ([CommunityConcernId], [TenantId], [SiteId], [Status], [Category], [Severity], [FacilityArea], [Description], [ReportedByContactId], [RelatedIncidentId], [LoggedAtUtc], [CreatedAtUtc], [CreatedBy], [CorrelationId], [ActorId])
                    VALUES
                        (@CommunityConcernId, @TenantId, @SiteId, @Status, @Category, @Severity, @FacilityArea, @Description, @ReportedByContactId, @RelatedIncidentId, @LoggedAtUtc, @CreatedAtUtc, @CreatedBy, @CorrelationId, @ActorId);
                COMMIT TRANSACTION;
            END TRY
            BEGIN CATCH
                IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
                THROW;
            END CATCH;
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [CommunitySafety].[usp_CommunityConcern_GetById]
            (
                @CommunityConcernId UNIQUEIDENTIFIER,
                @TenantId UNIQUEIDENTIFIER = NULL
            )
            AS
            BEGIN
                SET NOCOUNT ON;

                SELECT
                    [CommunityConcernId], [TenantId], [SiteId], [Category], [Severity], [FacilityArea], [Description],
                    [ReportedByContactId], [RelatedIncidentId], [Status] AS [ConcernStatus], [LoggedAtUtc], [ResolvedAtUtc],
                    [CreatedAtUtc], [CreatedBy]
                FROM [CommunitySafety].[CommunityConcern]
                WHERE [CommunityConcernId] = @CommunityConcernId
                  AND (@TenantId IS NULL OR [TenantId] = @TenantId)
                  AND ISNULL([IsDeleted], 0) = 0;
            END;
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [CommunitySafety].[usp_CommunityConcern_GetOpen]
                @TenantId UNIQUEIDENTIFIER,
                @SiteId   UNIQUEIDENTIFIER = NULL
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT
                    [CommunityConcernId], [TenantId], [SiteId], [Category], [Severity], [FacilityArea], [Description],
                    [ReportedByContactId], [RelatedIncidentId], [Status] AS [ConcernStatus], [LoggedAtUtc], [ResolvedAtUtc],
                    [CreatedAtUtc], [CreatedBy]
                FROM [CommunitySafety].[CommunityConcern]
                WHERE [TenantId] = @TenantId
                  AND (@SiteId IS NULL OR [SiteId] = @SiteId)
                  AND [Status] NOT IN (N'Resolved', N'Archived')
                  AND ISNULL([IsDeleted], 0) = 0
                ORDER BY [LoggedAtUtc] DESC;
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [CommunitySafety].[usp_CommunityConcern_Update]
                @CorrelationId UNIQUEIDENTIFIER = NULL,
                @ActorId       UNIQUEIDENTIFIER = NULL,
                @NowUtc        DATETIME2(3),
                @CommunityConcernId UNIQUEIDENTIFIER,
                @Status NVARCHAR(50),
                @Severity NVARCHAR(30),
                @EscalationCount INT,
                @EscalationReason NVARCHAR(1000) = NULL,
                @EscalatedAtUtc DATETIME2(3) = NULL,
                @ResolutionNotes NVARCHAR(4000) = NULL,
                @ResolvedAtUtc DATETIME2(3) = NULL
            AS
            SET NOCOUNT ON;
            BEGIN TRY
                BEGIN TRANSACTION;
                    UPDATE [CommunitySafety].[CommunityConcern]
                    SET
                        [Status] = @Status,
                        [Severity] = @Severity,
                        [EscalationCount] = @EscalationCount,
                        [EscalationReason] = @EscalationReason,
                        [EscalatedAtUtc] = @EscalatedAtUtc,
                        [ResolutionNotes] = @ResolutionNotes,
                        [ResolvedAtUtc] = @ResolvedAtUtc,
                        [ModifiedAtUtc] = @NowUtc,
                        [ModifiedBy] = @ActorId,
                        [CorrelationId] = @CorrelationId
                    WHERE [CommunityConcernId] = @CommunityConcernId
                      AND ISNULL([IsDeleted], 0) = 0;
                COMMIT TRANSACTION;
            END TRY
            BEGIN CATCH
                IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
                THROW;
            END CATCH;
            """);

        // ---- CommunityImpactAssessment ----------------------------------------------------

        await conn.ExecuteAsync("""
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CommunityImpactAssessment' AND schema_id = SCHEMA_ID('CommunitySafety'))
            BEGIN
                CREATE TABLE [CommunitySafety].[CommunityImpactAssessment] (
                    [CommunityImpactAssessmentId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                    [TenantId] UNIQUEIDENTIFIER NOT NULL,
                    [SiteId] UNIQUEIDENTIFIER NOT NULL,
                    [Status] NVARCHAR(50) NOT NULL,
                    [IncidentId] UNIQUEIDENTIFIER NOT NULL,
                    [Severity] NVARCHAR(30) NOT NULL,
                    [AffectedArea] NVARCHAR(200) NOT NULL,
                    [EstimatedPersonsAffected] INT NULL,
                    [Description] NVARCHAR(4000) NOT NULL,
                    [RequiresRightToKnowReporting] BIT NOT NULL CONSTRAINT DF_CommunityImpactAssessment_RTK DEFAULT (0),
                    [AssessedByUserId] UNIQUEIDENTIFIER NOT NULL,
                    [AssessedAtUtc] DATETIME2(3) NOT NULL,
                    [FinalizedAtUtc] DATETIME2(3) NULL,
                    [CreatedAtUtc] DATETIME2(3) NOT NULL,
                    [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
                    [IsDeleted] BIT NOT NULL CONSTRAINT DF_CommunityImpactAssessment_IsDeleted DEFAULT (0),
                    [ModifiedAtUtc] DATETIME2(3) NULL,
                    [ModifiedBy] UNIQUEIDENTIFIER NULL,
                    [CorrelationId] UNIQUEIDENTIFIER NULL,
                    [ActorId] UNIQUEIDENTIFIER NULL
                );

                CREATE NONCLUSTERED INDEX [IX_CommunityImpactAssessment_TenantId_IncidentId]
                    ON [CommunitySafety].[CommunityImpactAssessment] ([TenantId], [IncidentId])
                    INCLUDE ([Status], [Severity]);
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [CommunitySafety].[usp_CommunityImpactAssessment_Create]
                @CorrelationId UNIQUEIDENTIFIER = NULL,
                @ActorId       UNIQUEIDENTIFIER = NULL,
                @CommunityImpactAssessmentId UNIQUEIDENTIFIER,
                @TenantId UNIQUEIDENTIFIER,
                @SiteId UNIQUEIDENTIFIER,
                @Status NVARCHAR(50),
                @IncidentId UNIQUEIDENTIFIER,
                @Severity NVARCHAR(30),
                @AffectedArea NVARCHAR(200),
                @EstimatedPersonsAffected INT = NULL,
                @Description NVARCHAR(4000),
                @RequiresRightToKnowReporting BIT,
                @AssessedByUserId UNIQUEIDENTIFIER,
                @AssessedAtUtc DATETIME2(3),
                @CreatedAtUtc DATETIME2(3),
                @CreatedBy UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN TRY
                BEGIN TRANSACTION;
                    INSERT INTO [CommunitySafety].[CommunityImpactAssessment]
                        ([CommunityImpactAssessmentId], [TenantId], [SiteId], [Status], [IncidentId], [Severity], [AffectedArea], [EstimatedPersonsAffected], [Description], [RequiresRightToKnowReporting], [AssessedByUserId], [AssessedAtUtc], [CreatedAtUtc], [CreatedBy], [CorrelationId], [ActorId])
                    VALUES
                        (@CommunityImpactAssessmentId, @TenantId, @SiteId, @Status, @IncidentId, @Severity, @AffectedArea, @EstimatedPersonsAffected, @Description, @RequiresRightToKnowReporting, @AssessedByUserId, @AssessedAtUtc, @CreatedAtUtc, @CreatedBy, @CorrelationId, @ActorId);
                COMMIT TRANSACTION;
            END TRY
            BEGIN CATCH
                IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
                THROW;
            END CATCH;
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [CommunitySafety].[usp_CommunityImpactAssessment_GetById]
            (
                @CommunityImpactAssessmentId UNIQUEIDENTIFIER
            )
            AS
            BEGIN
                SET NOCOUNT ON;

                SELECT [CommunityImpactAssessmentId], [TenantId], [SiteId], [Status], [IncidentId], [Severity], [AffectedArea], [EstimatedPersonsAffected], [Description], [RequiresRightToKnowReporting], [AssessedByUserId], [AssessedAtUtc], [CreatedAtUtc], [CreatedBy], [FinalizedAtUtc]
                FROM [CommunitySafety].[CommunityImpactAssessment]
                WHERE [CommunityImpactAssessmentId] = @CommunityImpactAssessmentId
                  AND ISNULL([IsDeleted], 0) = 0;
            END;
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [CommunitySafety].[usp_CommunityImpactAssessment_GetByIncident]
                @TenantId   UNIQUEIDENTIFIER,
                @IncidentId UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT
                    [CommunityImpactAssessmentId], [TenantId], [SiteId], [IncidentId], [Severity], [AffectedArea],
                    [EstimatedPersonsAffected], [Description], [RequiresRightToKnowReporting],
                    [Status] AS [AssessmentStatus], [AssessedAtUtc], [CreatedAtUtc], [CreatedBy]
                FROM [CommunitySafety].[CommunityImpactAssessment]
                WHERE [TenantId] = @TenantId
                  AND [IncidentId] = @IncidentId
                  AND ISNULL([IsDeleted], 0) = 0
                ORDER BY [AssessedAtUtc] DESC;
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [CommunitySafety].[usp_CommunityImpactAssessment_Update]
                @CorrelationId UNIQUEIDENTIFIER = NULL,
                @ActorId       UNIQUEIDENTIFIER = NULL,
                @NowUtc        DATETIME2(3),
                @CommunityImpactAssessmentId UNIQUEIDENTIFIER,
                @Status NVARCHAR(50),
                @FinalizedAtUtc DATETIME2(3) = NULL
            AS
            SET NOCOUNT ON;
            BEGIN TRY
                BEGIN TRANSACTION;
                    UPDATE [CommunitySafety].[CommunityImpactAssessment]
                    SET
                        [Status] = @Status,
                        [FinalizedAtUtc] = @FinalizedAtUtc,
                        [ModifiedAtUtc] = @NowUtc,
                        [ModifiedBy] = @ActorId,
                        [CorrelationId] = @CorrelationId
                    WHERE [CommunityImpactAssessmentId] = @CommunityImpactAssessmentId
                      AND ISNULL([IsDeleted], 0) = 0;
                COMMIT TRANSACTION;
            END TRY
            BEGIN CATCH
                IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
                THROW;
            END CATCH;
            """);

        // ---- CommunityNotification ---------------------------------------------------------

        await conn.ExecuteAsync("""
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CommunityNotification' AND schema_id = SCHEMA_ID('CommunitySafety'))
            BEGIN
                CREATE TABLE [CommunitySafety].[CommunityNotification] (
                    [CommunityNotificationId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                    [TenantId] UNIQUEIDENTIFIER NOT NULL,
                    [SiteId] UNIQUEIDENTIFIER NULL,
                    [Status] NVARCHAR(50) NOT NULL,
                    [Channel] NVARCHAR(50) NOT NULL,
                    [RecipientContactId] UNIQUEIDENTIFIER NOT NULL,
                    [Subject] NVARCHAR(300) NOT NULL,
                    [MessageBody] NVARCHAR(MAX) NOT NULL,
                    [RelatedConcernId] UNIQUEIDENTIFIER NULL,
                    [RelatedIncidentId] UNIQUEIDENTIFIER NULL,
                    [SentAtUtc] DATETIME2(3) NOT NULL,
                    [AcknowledgedAtUtc] DATETIME2(3) NULL,
                    [FailureReason] NVARCHAR(1000) NULL,
                    [CreatedAtUtc] DATETIME2(3) NOT NULL,
                    [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
                    [IsDeleted] BIT NOT NULL CONSTRAINT DF_CommunityNotification_IsDeleted DEFAULT (0),
                    [ModifiedAtUtc] DATETIME2(3) NULL,
                    [ModifiedBy] UNIQUEIDENTIFIER NULL,
                    [CorrelationId] UNIQUEIDENTIFIER NULL,
                    [ActorId] UNIQUEIDENTIFIER NULL
                );

                CREATE NONCLUSTERED INDEX [IX_CommunityNotification_TenantId_SiteId_SentAtUtc]
                    ON [CommunitySafety].[CommunityNotification] ([TenantId], [SiteId], [SentAtUtc])
                    INCLUDE ([Status], [Channel], [RecipientContactId]);
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [CommunitySafety].[usp_CommunityNotification_Create]
                @CorrelationId UNIQUEIDENTIFIER = NULL,
                @ActorId       UNIQUEIDENTIFIER = NULL,
                @CommunityNotificationId UNIQUEIDENTIFIER,
                @TenantId UNIQUEIDENTIFIER,
                @SiteId UNIQUEIDENTIFIER = NULL,
                @Status NVARCHAR(50),
                @Channel NVARCHAR(50),
                @RecipientContactId UNIQUEIDENTIFIER,
                @Subject NVARCHAR(300),
                @MessageBody NVARCHAR(MAX),
                @RelatedConcernId UNIQUEIDENTIFIER = NULL,
                @RelatedIncidentId UNIQUEIDENTIFIER = NULL,
                @SentAtUtc DATETIME2(3),
                @CreatedAtUtc DATETIME2(3),
                @CreatedBy UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN TRY
                BEGIN TRANSACTION;
                    INSERT INTO [CommunitySafety].[CommunityNotification]
                        ([CommunityNotificationId], [TenantId], [SiteId], [Status], [Channel], [RecipientContactId], [Subject], [MessageBody], [RelatedConcernId], [RelatedIncidentId], [SentAtUtc], [CreatedAtUtc], [CreatedBy], [CorrelationId], [ActorId])
                    VALUES
                        (@CommunityNotificationId, @TenantId, @SiteId, @Status, @Channel, @RecipientContactId, @Subject, @MessageBody, @RelatedConcernId, @RelatedIncidentId, @SentAtUtc, @CreatedAtUtc, @CreatedBy, @CorrelationId, @ActorId);
                COMMIT TRANSACTION;
            END TRY
            BEGIN CATCH
                IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
                THROW;
            END CATCH;
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [CommunitySafety].[usp_CommunityNotification_GetById]
            (
                @CommunityNotificationId UNIQUEIDENTIFIER
            )
            AS
            BEGIN
                SET NOCOUNT ON;

                SELECT [CommunityNotificationId], [TenantId], [SiteId], [Status], [Channel], [RecipientContactId], [Subject], [MessageBody], [RelatedConcernId], [RelatedIncidentId], [SentAtUtc], [AcknowledgedAtUtc], [FailureReason], [CreatedAtUtc], [CreatedBy]
                FROM [CommunitySafety].[CommunityNotification]
                WHERE [CommunityNotificationId] = @CommunityNotificationId
                  AND ISNULL([IsDeleted], 0) = 0;
            END;
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [CommunitySafety].[usp_CommunityNotification_GetItems]
                @TenantId UNIQUEIDENTIFIER,
                @SiteId   UNIQUEIDENTIFIER = NULL,
                @FromUtc  DATETIME2(3),
                @ToUtc    DATETIME2(3)
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT
                    [CommunityNotificationId],
                    [TenantId],
                    [SiteId],
                    [Channel],
                    [RecipientContactId],
                    [Subject],
                    [RelatedConcernId],
                    [RelatedIncidentId],
                    [Status] AS [NotificationStatus],
                    [SentAtUtc],
                    [AcknowledgedAtUtc],
                    [CreatedAtUtc],
                    [CreatedBy]
                FROM [CommunitySafety].[CommunityNotification]
                WHERE TenantId = @TenantId
                  AND (@SiteId IS NULL OR SiteId = @SiteId)
                  AND ISNULL([IsDeleted], 0) = 0
                  AND SentAtUtc >= @FromUtc
                  AND SentAtUtc <= @ToUtc
                ORDER BY SentAtUtc DESC;
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [CommunitySafety].[usp_CommunityNotification_Update]
                @CorrelationId UNIQUEIDENTIFIER = NULL,
                @ActorId       UNIQUEIDENTIFIER = NULL,
                @NowUtc        DATETIME2(3),
                @CommunityNotificationId UNIQUEIDENTIFIER,
                @Status NVARCHAR(50),
                @AcknowledgedAtUtc DATETIME2(3) = NULL,
                @FailureReason NVARCHAR(1000) = NULL
            AS
            SET NOCOUNT ON;
            BEGIN TRY
                BEGIN TRANSACTION;
                    UPDATE [CommunitySafety].[CommunityNotification]
                    SET
                        [Status] = @Status,
                        [AcknowledgedAtUtc] = @AcknowledgedAtUtc,
                        [FailureReason] = @FailureReason,
                        [ModifiedAtUtc] = @NowUtc,
                        [ModifiedBy] = @ActorId,
                        [CorrelationId] = @CorrelationId
                    WHERE [CommunityNotificationId] = @CommunityNotificationId
                      AND ISNULL([IsDeleted], 0) = 0;
                COMMIT TRANSACTION;
            END TRY
            BEGIN CATCH
                IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
                THROW;
            END CATCH;
            """);

        // ---- CommunityMeetingRecord ---------------------------------------------------------

        await conn.ExecuteAsync("""
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CommunityMeetingRecord' AND schema_id = SCHEMA_ID('CommunitySafety'))
            BEGIN
                CREATE TABLE [CommunitySafety].[CommunityMeetingRecord] (
                    [CommunityMeetingRecordId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                    [TenantId] UNIQUEIDENTIFIER NOT NULL,
                    [SiteId] UNIQUEIDENTIFIER NOT NULL,
                    [Status] NVARCHAR(50) NOT NULL,
                    [MeetingTitle] NVARCHAR(300) NOT NULL,
                    [MeetingDateUtc] DATETIME2(3) NOT NULL,
                    [Location] NVARCHAR(300) NOT NULL,
                    [AttendeeCount] INT NOT NULL CONSTRAINT DF_CommunityMeetingRecord_AttendeeCount DEFAULT (0),
                    [TopicsSummary] NVARCHAR(4000) NOT NULL,
                    [ActionItems] NVARCHAR(4000) NULL,
                    [RecordedByUserId] UNIQUEIDENTIFIER NOT NULL,
                    [CreatedAtUtc] DATETIME2(3) NOT NULL,
                    [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
                    [IsDeleted] BIT NOT NULL CONSTRAINT DF_CommunityMeetingRecord_IsDeleted DEFAULT (0),
                    [ModifiedAtUtc] DATETIME2(3) NULL,
                    [ModifiedBy] UNIQUEIDENTIFIER NULL,
                    [CorrelationId] UNIQUEIDENTIFIER NULL,
                    [ActorId] UNIQUEIDENTIFIER NULL
                );

                CREATE NONCLUSTERED INDEX [IX_CommunityMeetingRecord_TenantId_SiteId_MeetingDateUtc]
                    ON [CommunitySafety].[CommunityMeetingRecord] ([TenantId], [SiteId], [MeetingDateUtc])
                    INCLUDE ([Status], [MeetingTitle]);
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [CommunitySafety].[usp_CommunityMeetingRecord_Create]
                @CorrelationId UNIQUEIDENTIFIER = NULL,
                @ActorId       UNIQUEIDENTIFIER = NULL,
                @CommunityMeetingRecordId UNIQUEIDENTIFIER,
                @TenantId UNIQUEIDENTIFIER,
                @SiteId UNIQUEIDENTIFIER,
                @Status NVARCHAR(50),
                @MeetingTitle NVARCHAR(300),
                @MeetingDateUtc DATETIME2(3),
                @Location NVARCHAR(300),
                @AttendeeCount INT,
                @TopicsSummary NVARCHAR(4000),
                @ActionItems NVARCHAR(4000) = NULL,
                @RecordedByUserId UNIQUEIDENTIFIER,
                @CreatedAtUtc DATETIME2(3),
                @CreatedBy UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN TRY
                BEGIN TRANSACTION;
                    INSERT INTO [CommunitySafety].[CommunityMeetingRecord]
                        ([CommunityMeetingRecordId], [TenantId], [SiteId], [Status], [MeetingTitle], [MeetingDateUtc], [Location], [AttendeeCount], [TopicsSummary], [ActionItems], [RecordedByUserId], [CreatedAtUtc], [CreatedBy], [CorrelationId], [ActorId])
                    VALUES
                        (@CommunityMeetingRecordId, @TenantId, @SiteId, @Status, @MeetingTitle, @MeetingDateUtc, @Location, @AttendeeCount, @TopicsSummary, @ActionItems, @RecordedByUserId, @CreatedAtUtc, @CreatedBy, @CorrelationId, @ActorId);
                COMMIT TRANSACTION;
            END TRY
            BEGIN CATCH
                IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
                THROW;
            END CATCH;
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [CommunitySafety].[usp_CommunityMeetingRecord_GetById]
            (
                @CommunityMeetingRecordId UNIQUEIDENTIFIER
            )
            AS
            BEGIN
                SET NOCOUNT ON;

                SELECT [CommunityMeetingRecordId], [TenantId], [SiteId], [Status], [MeetingTitle], [MeetingDateUtc], [Location], [AttendeeCount], [TopicsSummary], [ActionItems], [RecordedByUserId], [CreatedAtUtc], [CreatedBy]
                FROM [CommunitySafety].[CommunityMeetingRecord]
                WHERE [CommunityMeetingRecordId] = @CommunityMeetingRecordId
                  AND ISNULL([IsDeleted], 0) = 0;
            END;
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [CommunitySafety].[usp_CommunityMeetingRecord_GetItems]
                @TenantId UNIQUEIDENTIFIER,
                @SiteId   UNIQUEIDENTIFIER = NULL,
                @FromUtc  DATETIME2(3),
                @ToUtc    DATETIME2(3)
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT
                    [CommunityMeetingRecordId],
                    [TenantId],
                    [SiteId],
                    [MeetingTitle],
                    [MeetingDateUtc],
                    [Location],
                    [AttendeeCount],
                    [TopicsSummary],
                    [ActionItems],
                    [Status] AS [RecordStatus],
                    [CreatedAtUtc],
                    [CreatedBy]
                FROM [CommunitySafety].[CommunityMeetingRecord]
                WHERE TenantId = @TenantId
                  AND (@SiteId IS NULL OR SiteId = @SiteId)
                  AND ISNULL([IsDeleted], 0) = 0
                  AND MeetingDateUtc >= @FromUtc
                  AND MeetingDateUtc <= @ToUtc
                ORDER BY MeetingDateUtc DESC;
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [CommunitySafety].[usp_CommunityMeetingRecord_Update]
                @CorrelationId UNIQUEIDENTIFIER = NULL,
                @ActorId       UNIQUEIDENTIFIER = NULL,
                @NowUtc        DATETIME2(3),
                @CommunityMeetingRecordId UNIQUEIDENTIFIER,
                @Status NVARCHAR(50)
            AS
            SET NOCOUNT ON;
            BEGIN TRY
                BEGIN TRANSACTION;
                    UPDATE [CommunitySafety].[CommunityMeetingRecord]
                    SET
                        [Status] = @Status,
                        [ModifiedAtUtc] = @NowUtc,
                        [ModifiedBy] = @ActorId,
                        [CorrelationId] = @CorrelationId
                    WHERE [CommunityMeetingRecordId] = @CommunityMeetingRecordId
                      AND ISNULL([IsDeleted], 0) = 0;
                COMMIT TRANSACTION;
            END TRY
            BEGIN CATCH
                IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
                THROW;
            END CATCH;
            """);
    }

    /// <summary>
    /// Creates the <c>[PublicSafetyCommunication]</c> schema (PublicSafetyAgency, AgencyContact,
    /// IncidentNotification, PublicAlertBroadcast, CommunicationLogEntry tables) and their backing
    /// stored procedures against the Testcontainers database, mirroring
    /// PlantSafety.Infrastructure/PublicSafetyCommunication/Sql/Tables and /Sql/Procedures
    /// verbatim except that the AuditLog/ErrorLog CATCH-block writes are stripped from
    /// <c>[PlantSafety.AuditDB].dbo.</c> / <c>[PlantSafety.ErrorDB].dbo.</c> down to plain
    /// <c>dbo.AuditLog</c> / <c>dbo.ErrorLog</c>, matching the single-database Testcontainers
    /// topology (both tables are created by <see cref="ApplySharedAuditLoggingSchemaAsync"/>,
    /// which must run before this method -- see that method's own remarks for the same pattern
    /// applied to every other module in this file).
    /// </summary>
    public static async Task ApplyPublicSafetyCommunicationSchemaAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken);

        await conn.ExecuteAsync("""
            IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'PublicSafetyCommunication')
                EXEC('CREATE SCHEMA [PublicSafetyCommunication] AUTHORIZATION dbo;');
            """);

        await conn.ExecuteAsync("""
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PublicSafetyAgency' AND schema_id = SCHEMA_ID('PublicSafetyCommunication'))
            BEGIN
                CREATE TABLE [PublicSafetyCommunication].[PublicSafetyAgency]
                (
                    PublicSafetyAgencyId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_PublicSafetyAgency PRIMARY KEY,
                    TenantId UNIQUEIDENTIFIER NOT NULL,
                    SiteId UNIQUEIDENTIFIER NULL,
                    Status NVARCHAR(50) NOT NULL,
                    AgencyName NVARCHAR(400) NOT NULL,
                    AgencyType NVARCHAR(64) NOT NULL,
                    JurisdictionDescription NVARCHAR(400) NOT NULL,
                    PrimaryPhoneNumber NVARCHAR(64) NOT NULL,
                    PrimaryEmail NVARCHAR(320) NULL,
                    VerificationStatus NVARCHAR(50) NOT NULL,
                    LastVerifiedAtUtc DATETIME2(3) NULL,
                    NextVerificationDueAtUtc DATETIME2(3) NULL,
                    CreatedAtUtc DATETIME2(3) NOT NULL,
                    CreatedBy UNIQUEIDENTIFIER NOT NULL,
                    UpdatedAtUtc DATETIME2(3) NULL,
                    UpdatedBy UNIQUEIDENTIFIER NULL,
                    IsDeleted BIT NOT NULL CONSTRAINT DF_PublicSafetyAgency_IsDeleted DEFAULT (0),
                    RowVersion ROWVERSION NOT NULL
                );

                CREATE NONCLUSTERED INDEX IX_PublicSafetyAgency_TenantId_Site ON [PublicSafetyCommunication].[PublicSafetyAgency] (TenantId, SiteId, IsDeleted);
                CREATE NONCLUSTERED INDEX IX_PublicSafetyAgency_Status ON [PublicSafetyCommunication].[PublicSafetyAgency] (Status, IsDeleted);
                CREATE NONCLUSTERED INDEX IX_PublicSafetyAgency_AgencyType ON [PublicSafetyCommunication].[PublicSafetyAgency] (AgencyType, IsDeleted);
                CREATE NONCLUSTERED INDEX IX_PublicSafetyAgency_NextVerificationDueAtUtc ON [PublicSafetyCommunication].[PublicSafetyAgency] (NextVerificationDueAtUtc);
            END
            """);

        await conn.ExecuteAsync("""
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AgencyContact' AND schema_id = SCHEMA_ID('PublicSafetyCommunication'))
            BEGIN
                CREATE TABLE [PublicSafetyCommunication].[AgencyContact]
                (
                    AgencyContactId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_AgencyContact PRIMARY KEY,
                    PublicSafetyAgencyId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_AgencyContact_PublicSafetyAgency REFERENCES [PublicSafetyCommunication].[PublicSafetyAgency] (PublicSafetyAgencyId),
                    Role NVARCHAR(64) NOT NULL,
                    ContactName NVARCHAR(400) NOT NULL,
                    PhoneNumber NVARCHAR(64) NOT NULL,
                    Email NVARCHAR(320) NULL,
                    LastVerifiedAtUtc DATETIME2(3) NULL,
                    CreatedAtUtc DATETIME2(3) NOT NULL
                );

                CREATE NONCLUSTERED INDEX IX_AgencyContact_PublicSafetyAgencyId ON [PublicSafetyCommunication].[AgencyContact] (PublicSafetyAgencyId);
            END
            """);

        await conn.ExecuteAsync("""
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'IncidentNotification' AND schema_id = SCHEMA_ID('PublicSafetyCommunication'))
            BEGIN
                CREATE TABLE [PublicSafetyCommunication].[IncidentNotification]
                (
                    IncidentNotificationId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_IncidentNotification PRIMARY KEY,
                    TenantId UNIQUEIDENTIFIER NOT NULL,
                    SiteId UNIQUEIDENTIFIER NULL,
                    Status NVARCHAR(50) NOT NULL,
                    IncidentId UNIQUEIDENTIFIER NULL,
                    PublicSafetyAgencyId UNIQUEIDENTIFIER NOT NULL,
                    ChannelCode NVARCHAR(64) NOT NULL,
                    Message NVARCHAR(MAX) NOT NULL,
                    DispatchedAtUtc DATETIME2(3) NULL,
                    DispatchedBy UNIQUEIDENTIFIER NULL,
                    AcknowledgedAtUtc DATETIME2(3) NULL,
                    AcknowledgedByContactName NVARCHAR(400) NULL,
                    ClosedAtUtc DATETIME2(3) NULL,
                    FailureReason NVARCHAR(400) NULL,
                    CreatedAtUtc DATETIME2(3) NOT NULL,
                    CreatedBy UNIQUEIDENTIFIER NOT NULL,
                    UpdatedAtUtc DATETIME2(3) NULL,
                    UpdatedBy UNIQUEIDENTIFIER NULL,
                    IsDeleted BIT NOT NULL CONSTRAINT DF_IncidentNotification_IsDeleted DEFAULT (0),
                    RowVersion ROWVERSION NOT NULL
                );

                CREATE NONCLUSTERED INDEX IX_IncidentNotification_TenantId_Site ON [PublicSafetyCommunication].[IncidentNotification] (TenantId, SiteId, IsDeleted);
                CREATE NONCLUSTERED INDEX IX_IncidentNotification_IncidentId ON [PublicSafetyCommunication].[IncidentNotification] (IncidentId);
                CREATE NONCLUSTERED INDEX IX_IncidentNotification_Status ON [PublicSafetyCommunication].[IncidentNotification] (Status, IsDeleted);
                CREATE NONCLUSTERED INDEX IX_IncidentNotification_PublicSafetyAgencyId ON [PublicSafetyCommunication].[IncidentNotification] (PublicSafetyAgencyId);
            END
            """);

        await conn.ExecuteAsync("""
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PublicAlertBroadcast' AND schema_id = SCHEMA_ID('PublicSafetyCommunication'))
            BEGIN
                CREATE TABLE [PublicSafetyCommunication].[PublicAlertBroadcast]
                (
                    PublicAlertBroadcastId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_PublicAlertBroadcast PRIMARY KEY,
                    TenantId UNIQUEIDENTIFIER NOT NULL,
                    SiteId UNIQUEIDENTIFIER NULL,
                    Status NVARCHAR(50) NOT NULL,
                    IncidentId UNIQUEIDENTIFIER NULL,
                    SeverityCode NVARCHAR(64) NOT NULL,
                    ChannelCode NVARCHAR(64) NOT NULL,
                    Headline NVARCHAR(200) NOT NULL,
                    Message NVARCHAR(MAX) NOT NULL,
                    TargetArea NVARCHAR(400) NOT NULL,
                    IssuedAtUtc DATETIME2(3) NULL,
                    IssuedBy UNIQUEIDENTIFIER NULL,
                    DeliveryConfirmedAtUtc DATETIME2(3) NULL,
                    FailureReason NVARCHAR(400) NULL,
                    CancelledAtUtc DATETIME2(3) NULL,
                    CreatedAtUtc DATETIME2(3) NOT NULL,
                    CreatedBy UNIQUEIDENTIFIER NOT NULL,
                    UpdatedAtUtc DATETIME2(3) NULL,
                    UpdatedBy UNIQUEIDENTIFIER NULL,
                    IsDeleted BIT NOT NULL CONSTRAINT DF_PublicAlertBroadcast_IsDeleted DEFAULT (0),
                    RowVersion ROWVERSION NOT NULL
                );

                CREATE NONCLUSTERED INDEX IX_PublicAlertBroadcast_TenantId_Site ON [PublicSafetyCommunication].[PublicAlertBroadcast] (TenantId, SiteId, IsDeleted);
                CREATE NONCLUSTERED INDEX IX_PublicAlertBroadcast_IncidentId ON [PublicSafetyCommunication].[PublicAlertBroadcast] (IncidentId);
                CREATE NONCLUSTERED INDEX IX_PublicAlertBroadcast_Status ON [PublicSafetyCommunication].[PublicAlertBroadcast] (Status, IsDeleted);
            END
            """);

        await conn.ExecuteAsync("""
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CommunicationLogEntry' AND schema_id = SCHEMA_ID('PublicSafetyCommunication'))
            BEGIN
                CREATE TABLE [PublicSafetyCommunication].[CommunicationLogEntry]
                (
                    CommunicationLogEntryId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_CommunicationLogEntry PRIMARY KEY,
                    TenantId UNIQUEIDENTIFIER NOT NULL,
                    SiteId UNIQUEIDENTIFIER NULL,
                    IncidentId UNIQUEIDENTIFIER NULL,
                    RelatedIncidentNotificationId UNIQUEIDENTIFIER NULL,
                    RelatedPublicAlertBroadcastId UNIQUEIDENTIFIER NULL,
                    Direction NVARCHAR(32) NOT NULL,
                    Medium NVARCHAR(32) NOT NULL,
                    CounterpartyName NVARCHAR(400) NOT NULL,
                    Summary NVARCHAR(MAX) NOT NULL,
                    OccurredAtUtc DATETIME2(3) NOT NULL,
                    CreatedAtUtc DATETIME2(3) NOT NULL,
                    CreatedBy UNIQUEIDENTIFIER NOT NULL
                );

                CREATE NONCLUSTERED INDEX IX_CommunicationLogEntry_TenantId_Site ON [PublicSafetyCommunication].[CommunicationLogEntry] (TenantId, SiteId);
                CREATE NONCLUSTERED INDEX IX_CommunicationLogEntry_IncidentId ON [PublicSafetyCommunication].[CommunicationLogEntry] (IncidentId, OccurredAtUtc);
            END
            """);

        // ---- Stored procedures: PublicSafetyAgency / AgencyContact ------------------------

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [PublicSafetyCommunication].[usp_PublicSafetyAgency_Insert]
                @PublicSafetyAgencyId UNIQUEIDENTIFIER,
                @TenantId UNIQUEIDENTIFIER,
                @SiteId UNIQUEIDENTIFIER = NULL,
                @Status NVARCHAR(50),
                @AgencyName NVARCHAR(400),
                @AgencyType NVARCHAR(64),
                @JurisdictionDescription NVARCHAR(400),
                @PrimaryPhoneNumber NVARCHAR(64),
                @PrimaryEmail NVARCHAR(320) = NULL,
                @VerificationStatus NVARCHAR(50),
                @CreatedAtUtc DATETIME2(3),
                @CreatedBy UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN
                BEGIN TRY
                    INSERT INTO [PublicSafetyCommunication].[PublicSafetyAgency]
                    (
                        PublicSafetyAgencyId, TenantId, SiteId, Status, AgencyName, AgencyType, JurisdictionDescription,
                        PrimaryPhoneNumber, PrimaryEmail, VerificationStatus, CreatedAtUtc, CreatedBy, IsDeleted
                    )
                    VALUES
                    (
                        @PublicSafetyAgencyId, @TenantId, @SiteId, @Status, @AgencyName, @AgencyType, @JurisdictionDescription,
                        @PrimaryPhoneNumber, @PrimaryEmail, @VerificationStatus, @CreatedAtUtc, @CreatedBy, 0
                    );

                    BEGIN TRY
                        INSERT INTO dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
                        VALUES (N'PublicSafetyAgency', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'PublicSafetyAgencyId=', @PublicSafetyAgencyId), 0, OBJECT_NAME(@@PROCID));
                    END TRY
                    BEGIN CATCH
                    END CATCH;
                END TRY
                BEGIN CATCH
                    DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'PublicSafetyAgencyId=', @PublicSafetyAgencyId, N'; User=', SYSTEM_USER);
                    DECLARE @__ErrorDetails nvarchar(max) = CONCAT(
                        N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
                        N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
                        N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());

                    INSERT INTO dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
                    VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

                    THROW;
                END CATCH
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [PublicSafetyCommunication].[usp_PublicSafetyAgency_GetById]
                @PublicSafetyAgencyId UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT PublicSafetyAgencyId, TenantId, SiteId, Status, AgencyName, AgencyType, JurisdictionDescription,
                    PrimaryPhoneNumber, PrimaryEmail, VerificationStatus, LastVerifiedAtUtc, NextVerificationDueAtUtc,
                    CreatedAtUtc, CreatedBy, UpdatedAtUtc, UpdatedBy,
                    CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
                FROM [PublicSafetyCommunication].[PublicSafetyAgency]
                WHERE PublicSafetyAgencyId = @PublicSafetyAgencyId AND IsDeleted = 0;
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [PublicSafetyCommunication].[usp_PublicSafetyAgency_GetAllActive]
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT PublicSafetyAgencyId, TenantId, SiteId, Status, AgencyName, AgencyType, JurisdictionDescription,
                    PrimaryPhoneNumber, PrimaryEmail, VerificationStatus, LastVerifiedAtUtc, NextVerificationDueAtUtc,
                    CreatedAtUtc, CreatedBy, UpdatedAtUtc, UpdatedBy
                FROM [PublicSafetyCommunication].[PublicSafetyAgency]
                WHERE IsDeleted = 0;
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [PublicSafetyCommunication].[usp_PublicSafetyAgency_ListByTenant]
                @TenantId    UNIQUEIDENTIFIER,
                @SiteId      UNIQUEIDENTIFIER = NULL,
                @AgencyType  NVARCHAR(64) = NULL,
                @ActiveOnly  BIT = 0
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT PublicSafetyAgencyId, TenantId, SiteId, Status, AgencyName, AgencyType, JurisdictionDescription,
                    PrimaryPhoneNumber, PrimaryEmail, VerificationStatus, LastVerifiedAtUtc, NextVerificationDueAtUtc, CreatedAtUtc,
                    CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
                FROM [PublicSafetyCommunication].[PublicSafetyAgency]
                WHERE TenantId = @TenantId
                  AND (@SiteId IS NULL OR SiteId = @SiteId)
                  AND (@AgencyType IS NULL OR AgencyType = @AgencyType)
                  AND (@ActiveOnly = 0 OR Status = N'Active')
                  AND IsDeleted = 0
                ORDER BY AgencyName ASC;
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [PublicSafetyCommunication].[usp_PublicSafetyAgency_Update]
                @PublicSafetyAgencyId UNIQUEIDENTIFIER,
                @Status NVARCHAR(50),
                @AgencyName NVARCHAR(400),
                @AgencyType NVARCHAR(64),
                @JurisdictionDescription NVARCHAR(400),
                @PrimaryPhoneNumber NVARCHAR(64),
                @PrimaryEmail NVARCHAR(320) = NULL,
                @VerificationStatus NVARCHAR(50),
                @LastVerifiedAtUtc DATETIME2(3) = NULL,
                @NextVerificationDueAtUtc DATETIME2(3) = NULL,
                @UpdatedBy UNIQUEIDENTIFIER = NULL,
                @ETag NVARCHAR(64) = NULL
            AS
            SET NOCOUNT ON;
            BEGIN
                BEGIN TRY
                    IF NOT EXISTS (SELECT 1 FROM [PublicSafetyCommunication].[PublicSafetyAgency] WHERE PublicSafetyAgencyId = @PublicSafetyAgencyId AND IsDeleted = 0)
                        THROW 50004, N'PublicSafetyAgency not found or has been deleted.', 1;

                    UPDATE [PublicSafetyCommunication].[PublicSafetyAgency]
                    SET
                        Status = @Status,
                        AgencyName = @AgencyName,
                        AgencyType = @AgencyType,
                        JurisdictionDescription = @JurisdictionDescription,
                        PrimaryPhoneNumber = @PrimaryPhoneNumber,
                        PrimaryEmail = @PrimaryEmail,
                        VerificationStatus = @VerificationStatus,
                        LastVerifiedAtUtc = @LastVerifiedAtUtc,
                        NextVerificationDueAtUtc = @NextVerificationDueAtUtc,
                        UpdatedBy = @UpdatedBy,
                        UpdatedAtUtc = SYSUTCDATETIME()
                    WHERE PublicSafetyAgencyId = @PublicSafetyAgencyId AND IsDeleted = 0
                      AND (@ETag IS NULL OR CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) = @ETag);

                    DECLARE @RowsAffected INT = @@ROWCOUNT;

                    BEGIN TRY
                        INSERT INTO dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
                        VALUES (N'PublicSafetyAgency', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'PublicSafetyAgencyId=', @PublicSafetyAgencyId), 0, OBJECT_NAME(@@PROCID));
                    END TRY
                    BEGIN CATCH
                    END CATCH;

                    SELECT @RowsAffected;
                END TRY
                BEGIN CATCH
                    DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'PublicSafetyAgencyId=', @PublicSafetyAgencyId, N'; User=', SYSTEM_USER);
                    DECLARE @__ErrorDetails nvarchar(max) = CONCAT(
                        N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
                        N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
                        N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());

                    INSERT INTO dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
                    VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

                    THROW;
                END CATCH
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [PublicSafetyCommunication].[usp_AgencyContact_Insert]
                @AgencyContactId UNIQUEIDENTIFIER,
                @PublicSafetyAgencyId UNIQUEIDENTIFIER,
                @Role NVARCHAR(64),
                @ContactName NVARCHAR(400),
                @PhoneNumber NVARCHAR(64),
                @Email NVARCHAR(320) = NULL,
                @CreatedAtUtc DATETIME2(3)
            AS
            SET NOCOUNT ON;
            BEGIN
                BEGIN TRY
                    INSERT INTO [PublicSafetyCommunication].[AgencyContact]
                    (AgencyContactId, PublicSafetyAgencyId, Role, ContactName, PhoneNumber, Email, CreatedAtUtc)
                    VALUES
                    (@AgencyContactId, @PublicSafetyAgencyId, @Role, @ContactName, @PhoneNumber, @Email, @CreatedAtUtc);
                END TRY
                BEGIN CATCH
                    DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'AgencyContactId=', @AgencyContactId, N'; User=', SYSTEM_USER);
                    DECLARE @__ErrorDetails nvarchar(max) = CONCAT(
                        N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
                        N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
                        N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());

                    INSERT INTO dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
                    VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

                    THROW;
                END CATCH
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [PublicSafetyCommunication].[usp_AgencyContact_GetByAgencyId]
                @PublicSafetyAgencyId UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT AgencyContactId, PublicSafetyAgencyId, Role, ContactName, PhoneNumber, Email, LastVerifiedAtUtc, CreatedAtUtc
                FROM [PublicSafetyCommunication].[AgencyContact]
                WHERE PublicSafetyAgencyId = @PublicSafetyAgencyId
                ORDER BY CreatedAtUtc ASC;
            END
            """);

        // ---- Stored procedures: IncidentNotification ---------------------------------------

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [PublicSafetyCommunication].[usp_IncidentNotification_Insert]
                @IncidentNotificationId UNIQUEIDENTIFIER,
                @TenantId UNIQUEIDENTIFIER,
                @SiteId UNIQUEIDENTIFIER = NULL,
                @Status NVARCHAR(50),
                @IncidentId UNIQUEIDENTIFIER = NULL,
                @PublicSafetyAgencyId UNIQUEIDENTIFIER,
                @ChannelCode NVARCHAR(64),
                @Message NVARCHAR(MAX),
                @DispatchedAtUtc DATETIME2(3) = NULL,
                @DispatchedBy UNIQUEIDENTIFIER = NULL,
                @CreatedAtUtc DATETIME2(3),
                @CreatedBy UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN
                BEGIN TRY
                    INSERT INTO [PublicSafetyCommunication].[IncidentNotification]
                    (
                        IncidentNotificationId, TenantId, SiteId, Status, IncidentId, PublicSafetyAgencyId, ChannelCode, Message,
                        DispatchedAtUtc, DispatchedBy, CreatedAtUtc, CreatedBy, IsDeleted
                    )
                    VALUES
                    (
                        @IncidentNotificationId, @TenantId, @SiteId, @Status, @IncidentId, @PublicSafetyAgencyId, @ChannelCode, @Message,
                        @DispatchedAtUtc, @DispatchedBy, @CreatedAtUtc, @CreatedBy, 0
                    );

                    BEGIN TRY
                        INSERT INTO dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
                        VALUES (N'IncidentNotification', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'IncidentNotificationId=', @IncidentNotificationId), 0, OBJECT_NAME(@@PROCID));
                    END TRY
                    BEGIN CATCH
                    END CATCH;
                END TRY
                BEGIN CATCH
                    DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'IncidentNotificationId=', @IncidentNotificationId, N'; User=', SYSTEM_USER);
                    DECLARE @__ErrorDetails nvarchar(max) = CONCAT(
                        N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
                        N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
                        N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());

                    INSERT INTO dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
                    VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

                    THROW;
                END CATCH
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [PublicSafetyCommunication].[usp_IncidentNotification_GetById]
                @IncidentNotificationId UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT IncidentNotificationId, TenantId, SiteId, Status, IncidentId, PublicSafetyAgencyId, ChannelCode, Message,
                    DispatchedAtUtc, DispatchedBy, AcknowledgedAtUtc, AcknowledgedByContactName, ClosedAtUtc, FailureReason,
                    CreatedAtUtc, CreatedBy, UpdatedAtUtc, UpdatedBy,
                    CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
                FROM [PublicSafetyCommunication].[IncidentNotification]
                WHERE IncidentNotificationId = @IncidentNotificationId AND IsDeleted = 0;
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [PublicSafetyCommunication].[usp_IncidentNotification_GetAllActive]
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT IncidentNotificationId, TenantId, SiteId, Status, IncidentId, PublicSafetyAgencyId, ChannelCode, Message,
                    DispatchedAtUtc, DispatchedBy, AcknowledgedAtUtc, AcknowledgedByContactName, ClosedAtUtc, FailureReason,
                    CreatedAtUtc, CreatedBy, UpdatedAtUtc, UpdatedBy
                FROM [PublicSafetyCommunication].[IncidentNotification]
                WHERE IsDeleted = 0;
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [PublicSafetyCommunication].[usp_IncidentNotification_GetStatus]
                @TenantId   UNIQUEIDENTIFIER,
                @SiteId     UNIQUEIDENTIFIER = NULL,
                @IncidentId UNIQUEIDENTIFIER = NULL,
                @FromUtc    DATETIME2(3),
                @ToUtc      DATETIME2(3)
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT IncidentNotificationId, TenantId, SiteId, Status, IncidentId, PublicSafetyAgencyId, ChannelCode, Message,
                    DispatchedAtUtc, AcknowledgedAtUtc, AcknowledgedByContactName, ClosedAtUtc, FailureReason, CreatedAtUtc,
                    CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
                FROM [PublicSafetyCommunication].[IncidentNotification]
                WHERE TenantId = @TenantId
                  AND (@SiteId IS NULL OR SiteId = @SiteId)
                  AND (@IncidentId IS NULL OR IncidentId = @IncidentId)
                  AND CreatedAtUtc >= @FromUtc AND CreatedAtUtc <= @ToUtc
                  AND IsDeleted = 0
                ORDER BY CreatedAtUtc DESC;
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [PublicSafetyCommunication].[usp_IncidentNotification_Update]
                @IncidentNotificationId UNIQUEIDENTIFIER,
                @Status NVARCHAR(50),
                @AcknowledgedAtUtc DATETIME2(3) = NULL,
                @AcknowledgedByContactName NVARCHAR(400) = NULL,
                @ClosedAtUtc DATETIME2(3) = NULL,
                @FailureReason NVARCHAR(400) = NULL,
                @UpdatedBy UNIQUEIDENTIFIER = NULL,
                @ETag NVARCHAR(64) = NULL
            AS
            SET NOCOUNT ON;
            BEGIN
                BEGIN TRY
                    IF NOT EXISTS (SELECT 1 FROM [PublicSafetyCommunication].[IncidentNotification] WHERE IncidentNotificationId = @IncidentNotificationId AND IsDeleted = 0)
                        THROW 50004, N'IncidentNotification not found or has been deleted.', 1;

                    UPDATE [PublicSafetyCommunication].[IncidentNotification]
                    SET
                        Status = @Status,
                        AcknowledgedAtUtc = @AcknowledgedAtUtc,
                        AcknowledgedByContactName = @AcknowledgedByContactName,
                        ClosedAtUtc = @ClosedAtUtc,
                        FailureReason = @FailureReason,
                        UpdatedBy = @UpdatedBy,
                        UpdatedAtUtc = SYSUTCDATETIME()
                    WHERE IncidentNotificationId = @IncidentNotificationId AND IsDeleted = 0
                      AND (@ETag IS NULL OR CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) = @ETag);

                    DECLARE @RowsAffected INT = @@ROWCOUNT;

                    BEGIN TRY
                        INSERT INTO dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
                        VALUES (N'IncidentNotification', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'IncidentNotificationId=', @IncidentNotificationId), 0, OBJECT_NAME(@@PROCID));
                    END TRY
                    BEGIN CATCH
                    END CATCH;

                    SELECT @RowsAffected;
                END TRY
                BEGIN CATCH
                    DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'IncidentNotificationId=', @IncidentNotificationId, N'; User=', SYSTEM_USER);
                    DECLARE @__ErrorDetails nvarchar(max) = CONCAT(
                        N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
                        N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
                        N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());

                    INSERT INTO dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
                    VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

                    THROW;
                END CATCH
            END
            """);

        // ---- Stored procedures: PublicAlertBroadcast ----------------------------------------

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [PublicSafetyCommunication].[usp_PublicAlertBroadcast_Insert]
                @PublicAlertBroadcastId UNIQUEIDENTIFIER,
                @TenantId UNIQUEIDENTIFIER,
                @SiteId UNIQUEIDENTIFIER = NULL,
                @Status NVARCHAR(50),
                @IncidentId UNIQUEIDENTIFIER = NULL,
                @SeverityCode NVARCHAR(64),
                @ChannelCode NVARCHAR(64),
                @Headline NVARCHAR(200),
                @Message NVARCHAR(MAX),
                @TargetArea NVARCHAR(400),
                @IssuedAtUtc DATETIME2(3) = NULL,
                @IssuedBy UNIQUEIDENTIFIER = NULL,
                @CreatedAtUtc DATETIME2(3),
                @CreatedBy UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN
                BEGIN TRY
                    INSERT INTO [PublicSafetyCommunication].[PublicAlertBroadcast]
                    (
                        PublicAlertBroadcastId, TenantId, SiteId, Status, IncidentId, SeverityCode, ChannelCode, Headline, Message,
                        TargetArea, IssuedAtUtc, IssuedBy, CreatedAtUtc, CreatedBy, IsDeleted
                    )
                    VALUES
                    (
                        @PublicAlertBroadcastId, @TenantId, @SiteId, @Status, @IncidentId, @SeverityCode, @ChannelCode, @Headline, @Message,
                        @TargetArea, @IssuedAtUtc, @IssuedBy, @CreatedAtUtc, @CreatedBy, 0
                    );

                    BEGIN TRY
                        INSERT INTO dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
                        VALUES (N'PublicAlertBroadcast', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'PublicAlertBroadcastId=', @PublicAlertBroadcastId), 0, OBJECT_NAME(@@PROCID));
                    END TRY
                    BEGIN CATCH
                    END CATCH;
                END TRY
                BEGIN CATCH
                    DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'PublicAlertBroadcastId=', @PublicAlertBroadcastId, N'; User=', SYSTEM_USER);
                    DECLARE @__ErrorDetails nvarchar(max) = CONCAT(
                        N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
                        N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
                        N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());

                    INSERT INTO dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
                    VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

                    THROW;
                END CATCH
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [PublicSafetyCommunication].[usp_PublicAlertBroadcast_GetById]
                @PublicAlertBroadcastId UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT PublicAlertBroadcastId, TenantId, SiteId, Status, IncidentId, SeverityCode, ChannelCode, Headline, Message,
                    TargetArea, IssuedAtUtc, IssuedBy, DeliveryConfirmedAtUtc, FailureReason, CancelledAtUtc,
                    CreatedAtUtc, CreatedBy, UpdatedAtUtc, UpdatedBy,
                    CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
                FROM [PublicSafetyCommunication].[PublicAlertBroadcast]
                WHERE PublicAlertBroadcastId = @PublicAlertBroadcastId AND IsDeleted = 0;
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [PublicSafetyCommunication].[usp_PublicAlertBroadcast_GetAllActive]
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT PublicAlertBroadcastId, TenantId, SiteId, Status, IncidentId, SeverityCode, ChannelCode, Headline, Message,
                    TargetArea, IssuedAtUtc, IssuedBy, DeliveryConfirmedAtUtc, FailureReason, CancelledAtUtc,
                    CreatedAtUtc, CreatedBy, UpdatedAtUtc, UpdatedBy
                FROM [PublicSafetyCommunication].[PublicAlertBroadcast]
                WHERE IsDeleted = 0;
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [PublicSafetyCommunication].[usp_PublicAlertBroadcast_ListActive]
                @TenantId UNIQUEIDENTIFIER,
                @SiteId   UNIQUEIDENTIFIER = NULL
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT PublicAlertBroadcastId, TenantId, SiteId, Status, IncidentId, SeverityCode, ChannelCode, Headline, Message,
                    TargetArea, IssuedAtUtc, DeliveryConfirmedAtUtc, FailureReason, CreatedAtUtc,
                    CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
                FROM [PublicSafetyCommunication].[PublicAlertBroadcast]
                WHERE TenantId = @TenantId
                  AND (@SiteId IS NULL OR SiteId = @SiteId)
                  AND Status IN (N'Sent', N'Delivered')
                  AND IsDeleted = 0
                ORDER BY IssuedAtUtc DESC;
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [PublicSafetyCommunication].[usp_PublicAlertBroadcast_Update]
                @PublicAlertBroadcastId UNIQUEIDENTIFIER,
                @Status NVARCHAR(50),
                @DeliveryConfirmedAtUtc DATETIME2(3) = NULL,
                @FailureReason NVARCHAR(400) = NULL,
                @CancelledAtUtc DATETIME2(3) = NULL,
                @UpdatedBy UNIQUEIDENTIFIER = NULL,
                @ETag NVARCHAR(64) = NULL
            AS
            SET NOCOUNT ON;
            BEGIN
                BEGIN TRY
                    IF NOT EXISTS (SELECT 1 FROM [PublicSafetyCommunication].[PublicAlertBroadcast] WHERE PublicAlertBroadcastId = @PublicAlertBroadcastId AND IsDeleted = 0)
                        THROW 50004, N'PublicAlertBroadcast not found or has been deleted.', 1;

                    UPDATE [PublicSafetyCommunication].[PublicAlertBroadcast]
                    SET
                        Status = @Status,
                        DeliveryConfirmedAtUtc = @DeliveryConfirmedAtUtc,
                        FailureReason = @FailureReason,
                        CancelledAtUtc = @CancelledAtUtc,
                        UpdatedBy = @UpdatedBy,
                        UpdatedAtUtc = SYSUTCDATETIME()
                    WHERE PublicAlertBroadcastId = @PublicAlertBroadcastId AND IsDeleted = 0
                      AND (@ETag IS NULL OR CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) = @ETag);

                    DECLARE @RowsAffected INT = @@ROWCOUNT;

                    BEGIN TRY
                        INSERT INTO dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
                        VALUES (N'PublicAlertBroadcast', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'PublicAlertBroadcastId=', @PublicAlertBroadcastId), 0, OBJECT_NAME(@@PROCID));
                    END TRY
                    BEGIN CATCH
                    END CATCH;

                    SELECT @RowsAffected;
                END TRY
                BEGIN CATCH
                    DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'PublicAlertBroadcastId=', @PublicAlertBroadcastId, N'; User=', SYSTEM_USER);
                    DECLARE @__ErrorDetails nvarchar(max) = CONCAT(
                        N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
                        N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
                        N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());

                    INSERT INTO dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
                    VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

                    THROW;
                END CATCH
            END
            """);

        // ---- Stored procedures: CommunicationLogEntry (append-only) ------------------------

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [PublicSafetyCommunication].[usp_CommunicationLogEntry_Insert]
                @CommunicationLogEntryId UNIQUEIDENTIFIER,
                @TenantId UNIQUEIDENTIFIER,
                @SiteId UNIQUEIDENTIFIER = NULL,
                @IncidentId UNIQUEIDENTIFIER = NULL,
                @RelatedIncidentNotificationId UNIQUEIDENTIFIER = NULL,
                @RelatedPublicAlertBroadcastId UNIQUEIDENTIFIER = NULL,
                @Direction NVARCHAR(32),
                @Medium NVARCHAR(32),
                @CounterpartyName NVARCHAR(400),
                @Summary NVARCHAR(MAX),
                @OccurredAtUtc DATETIME2(3),
                @CreatedAtUtc DATETIME2(3),
                @CreatedBy UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN
                BEGIN TRY
                    INSERT INTO [PublicSafetyCommunication].[CommunicationLogEntry]
                    (
                        CommunicationLogEntryId, TenantId, SiteId, IncidentId, RelatedIncidentNotificationId, RelatedPublicAlertBroadcastId,
                        Direction, Medium, CounterpartyName, Summary, OccurredAtUtc, CreatedAtUtc, CreatedBy
                    )
                    VALUES
                    (
                        @CommunicationLogEntryId, @TenantId, @SiteId, @IncidentId, @RelatedIncidentNotificationId, @RelatedPublicAlertBroadcastId,
                        @Direction, @Medium, @CounterpartyName, @Summary, @OccurredAtUtc, @CreatedAtUtc, @CreatedBy
                    );

                    BEGIN TRY
                        INSERT INTO dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
                        VALUES (N'CommunicationLogEntry', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'CommunicationLogEntryId=', @CommunicationLogEntryId), 0, OBJECT_NAME(@@PROCID));
                    END TRY
                    BEGIN CATCH
                    END CATCH;
                END TRY
                BEGIN CATCH
                    DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'CommunicationLogEntryId=', @CommunicationLogEntryId, N'; User=', SYSTEM_USER);
                    DECLARE @__ErrorDetails nvarchar(max) = CONCAT(
                        N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
                        N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
                        N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());

                    INSERT INTO dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
                    VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

                    THROW;
                END CATCH
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [PublicSafetyCommunication].[usp_CommunicationLogEntry_GetById]
                @CommunicationLogEntryId UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT CommunicationLogEntryId, TenantId, SiteId, IncidentId, RelatedIncidentNotificationId, RelatedPublicAlertBroadcastId,
                    Direction, Medium, CounterpartyName, Summary, OccurredAtUtc, CreatedAtUtc, CreatedBy
                FROM [PublicSafetyCommunication].[CommunicationLogEntry]
                WHERE CommunicationLogEntryId = @CommunicationLogEntryId;
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [PublicSafetyCommunication].[usp_CommunicationLogEntry_GetAllActive]
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT CommunicationLogEntryId, TenantId, SiteId, IncidentId, RelatedIncidentNotificationId, RelatedPublicAlertBroadcastId,
                    Direction, Medium, CounterpartyName, Summary, OccurredAtUtc, CreatedAtUtc, CreatedBy
                FROM [PublicSafetyCommunication].[CommunicationLogEntry];
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [PublicSafetyCommunication].[usp_CommunicationLogEntry_GetByIncident]
                @TenantId   UNIQUEIDENTIFIER,
                @IncidentId UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT CommunicationLogEntryId, TenantId, SiteId, IncidentId, RelatedIncidentNotificationId, RelatedPublicAlertBroadcastId,
                    Direction, Medium, CounterpartyName, Summary, OccurredAtUtc, CreatedAtUtc
                FROM [PublicSafetyCommunication].[CommunicationLogEntry]
                WHERE TenantId = @TenantId AND IncidentId = @IncidentId
                ORDER BY OccurredAtUtc ASC;
            END
            """);
    }

    /// <summary>
    /// Creates the <c>[MachineSafety].[CraneHoistSystems]</c> table and its 8 stored procedures
    /// (Create, Update, GetById, GetAll, GetItems, GetItemById, ListDueForInspection,
    /// ListExpiringQualifications) against the Testcontainers database, mirroring
    /// <c>PlantSafety.Infrastructure\MachineSafety\Sql\Tables\CraneHoistSystems.sql</c> and
    /// <c>...\Sql\Procedures\usp_CraneHoistSystems_*.sql</c> verbatim. Backs
    /// <c>CraneHoistSystemsDapperRepository</c> (both the Domain write repository and the
    /// Application read-model repository). Scoped only to the CraneHoistSystems sub-aggregate of
    /// MachineSafety -- the other 6 MachineSafety sub-aggregates (LotoSystems,
    /// EmergencyStopDevices, MachineGuardsBarriers, PresenceSensingDevices, SafetyValvesRelays,
    /// TwoHandControls) are out of scope for this method and are applied separately, if/when
    /// their own integration-test suites are built out.
    /// </summary>
    public static async Task ApplyMachineSafetyCraneHoistSystemsSchemaAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken);

        await conn.ExecuteAsync("""
            IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'MachineSafety')
                EXEC('CREATE SCHEMA [MachineSafety]');
            """);

        await conn.ExecuteAsync("""
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CraneHoistSystems' AND schema_id = SCHEMA_ID('MachineSafety'))
            BEGIN
                CREATE TABLE [MachineSafety].[CraneHoistSystems] (
                    [CraneHoistSystemsId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                    [TenantId] UNIQUEIDENTIFIER NOT NULL,
                    [SiteId] UNIQUEIDENTIFIER NULL,
                    [Status] NVARCHAR(100) NOT NULL,
                    [AssetTag] NVARCHAR(400) NOT NULL,
                    [AssetTitle] NVARCHAR(400) NOT NULL,
                    [AssetType] NVARCHAR(100) NOT NULL,
                    [RatedLoadCapacityKg] DECIMAL(12,2) NOT NULL,
                    [LastObservedLoadKg] DECIMAL(12,2) NULL,
                    [LastLiftAtUtc] DATETIME2(3) NULL,
                    [OverloadEventCount] INT NOT NULL CONSTRAINT DF_CraneHoistSystems_OverloadEventCount DEFAULT (0),
                    [OverloadFlagged] BIT NOT NULL CONSTRAINT DF_CraneHoistSystems_OverloadFlagged DEFAULT (0),
                    [InspectionDueAtUtc] DATETIME2(3) NULL,
                    [LastInspectionAtUtc] DATETIME2(3) NULL,
                    [LastInspectionPassed] BIT NULL,
                    [OperatorUserId] UNIQUEIDENTIFIER NOT NULL,
                    [OperatorQualifiedAtUtc] DATETIME2(3) NULL,
                    [OperatorQualificationExpiresAtUtc] DATETIME2(3) NULL,
                    [OperatorQualificationSuspended] BIT NOT NULL CONSTRAINT DF_CraneHoistSystems_OperatorQualificationSuspended DEFAULT (0),
                    [LastRiggingInspectionAtUtc] DATETIME2(3) NULL,
                    [LastRiggingDefectDetected] BIT NULL,
                    [CreatedAtUtc] DATETIME2(3) NOT NULL,
                    [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
                    [IsDeleted] BIT NOT NULL CONSTRAINT DF_CraneHoistSystems_IsDeleted DEFAULT (0),
                    [ModifiedAtUtc] DATETIME2(3) NULL,
                    [ModifiedBy] UNIQUEIDENTIFIER NULL,
                    [CorrelationId] UNIQUEIDENTIFIER NULL,
                    [ActorId] UNIQUEIDENTIFIER NULL
                );

                CREATE NONCLUSTERED INDEX [IX_CraneHoistSystems_TenantId_SiteId]
                    ON [MachineSafety].[CraneHoistSystems] ([TenantId], [SiteId])
                    INCLUDE ([Status], [AssetTitle]);

                CREATE NONCLUSTERED INDEX [IX_CraneHoistSystems_InspectionDueAtUtc]
                    ON [MachineSafety].[CraneHoistSystems] ([TenantId], [InspectionDueAtUtc])
                    INCLUDE ([SiteId], [Status]);

                CREATE NONCLUSTERED INDEX [IX_CraneHoistSystems_OperatorQualificationExpiresAtUtc]
                    ON [MachineSafety].[CraneHoistSystems] ([TenantId], [OperatorQualificationExpiresAtUtc])
                    INCLUDE ([SiteId], [OperatorUserId]);
            END;
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [MachineSafety].[usp_CraneHoistSystems_Create]
                @CorrelationId UNIQUEIDENTIFIER = NULL,
                @ActorId       UNIQUEIDENTIFIER = NULL,
                @CraneHoistSystemsId UNIQUEIDENTIFIER,
                @TenantId UNIQUEIDENTIFIER,
                @SiteId UNIQUEIDENTIFIER = NULL,
                @Status NVARCHAR(100),
                @AssetTag NVARCHAR(400),
                @AssetTitle NVARCHAR(400),
                @AssetType NVARCHAR(100),
                @RatedLoadCapacityKg DECIMAL(12,2),
                @LastObservedLoadKg DECIMAL(12,2) = NULL,
                @LastLiftAtUtc DATETIME2(3) = NULL,
                @OverloadEventCount INT,
                @OverloadFlagged BIT,
                @InspectionDueAtUtc DATETIME2(3) = NULL,
                @LastInspectionAtUtc DATETIME2(3) = NULL,
                @LastInspectionPassed BIT = NULL,
                @OperatorUserId UNIQUEIDENTIFIER,
                @OperatorQualifiedAtUtc DATETIME2(3) = NULL,
                @OperatorQualificationExpiresAtUtc DATETIME2(3) = NULL,
                @OperatorQualificationSuspended BIT,
                @LastRiggingInspectionAtUtc DATETIME2(3) = NULL,
                @LastRiggingDefectDetected BIT = NULL,
                @CreatedAtUtc DATETIME2(3),
                @CreatedBy UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN TRY
                BEGIN TRANSACTION;
                    INSERT INTO [MachineSafety].[CraneHoistSystems]
                        ([CraneHoistSystemsId], [TenantId], [SiteId], [Status], [AssetTag], [AssetTitle], [AssetType], [RatedLoadCapacityKg], [LastObservedLoadKg], [LastLiftAtUtc], [OverloadEventCount], [OverloadFlagged], [InspectionDueAtUtc], [LastInspectionAtUtc], [LastInspectionPassed], [OperatorUserId], [OperatorQualifiedAtUtc], [OperatorQualificationExpiresAtUtc], [OperatorQualificationSuspended], [LastRiggingInspectionAtUtc], [LastRiggingDefectDetected], [CreatedAtUtc], [CreatedBy], [CorrelationId], [ActorId])
                    VALUES
                        (@CraneHoistSystemsId, @TenantId, @SiteId, @Status, @AssetTag, @AssetTitle, @AssetType, @RatedLoadCapacityKg, @LastObservedLoadKg, @LastLiftAtUtc, @OverloadEventCount, @OverloadFlagged, @InspectionDueAtUtc, @LastInspectionAtUtc, @LastInspectionPassed, @OperatorUserId, @OperatorQualifiedAtUtc, @OperatorQualificationExpiresAtUtc, @OperatorQualificationSuspended, @LastRiggingInspectionAtUtc, @LastRiggingDefectDetected, @CreatedAtUtc, @CreatedBy, @CorrelationId, @ActorId);
                COMMIT TRANSACTION;
            END TRY
            BEGIN CATCH
                IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
                THROW;
            END CATCH;
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [MachineSafety].[usp_CraneHoistSystems_Update]
                @CorrelationId UNIQUEIDENTIFIER = NULL,
                @ActorId       UNIQUEIDENTIFIER = NULL,
                @NowUtc        DATETIME2(3),
                @CraneHoistSystemsId UNIQUEIDENTIFIER,
                @TenantId UNIQUEIDENTIFIER,
                @SiteId UNIQUEIDENTIFIER = NULL,
                @Status NVARCHAR(100),
                @AssetTag NVARCHAR(400),
                @AssetTitle NVARCHAR(400),
                @AssetType NVARCHAR(100),
                @RatedLoadCapacityKg DECIMAL(12,2),
                @LastObservedLoadKg DECIMAL(12,2) = NULL,
                @LastLiftAtUtc DATETIME2(3) = NULL,
                @OverloadEventCount INT,
                @OverloadFlagged BIT,
                @InspectionDueAtUtc DATETIME2(3) = NULL,
                @LastInspectionAtUtc DATETIME2(3) = NULL,
                @LastInspectionPassed BIT = NULL,
                @OperatorUserId UNIQUEIDENTIFIER,
                @OperatorQualifiedAtUtc DATETIME2(3) = NULL,
                @OperatorQualificationExpiresAtUtc DATETIME2(3) = NULL,
                @OperatorQualificationSuspended BIT,
                @LastRiggingInspectionAtUtc DATETIME2(3) = NULL,
                @LastRiggingDefectDetected BIT = NULL
            AS
            SET NOCOUNT ON;
            BEGIN TRY
                BEGIN TRANSACTION;
                    UPDATE [MachineSafety].[CraneHoistSystems]
                    SET
                    [SiteId] = @SiteId,
                    [Status] = @Status,
                    [AssetTag] = @AssetTag,
                    [AssetTitle] = @AssetTitle,
                    [AssetType] = @AssetType,
                    [RatedLoadCapacityKg] = @RatedLoadCapacityKg,
                    [LastObservedLoadKg] = @LastObservedLoadKg,
                    [LastLiftAtUtc] = @LastLiftAtUtc,
                    [OverloadEventCount] = @OverloadEventCount,
                    [OverloadFlagged] = @OverloadFlagged,
                    [InspectionDueAtUtc] = @InspectionDueAtUtc,
                    [LastInspectionAtUtc] = @LastInspectionAtUtc,
                    [LastInspectionPassed] = @LastInspectionPassed,
                    [OperatorUserId] = @OperatorUserId,
                    [OperatorQualifiedAtUtc] = @OperatorQualifiedAtUtc,
                    [OperatorQualificationExpiresAtUtc] = @OperatorQualificationExpiresAtUtc,
                    [OperatorQualificationSuspended] = @OperatorQualificationSuspended,
                    [LastRiggingInspectionAtUtc] = @LastRiggingInspectionAtUtc,
                    [LastRiggingDefectDetected] = @LastRiggingDefectDetected,
                    [ModifiedAtUtc] = @NowUtc,
                    [ModifiedBy] = @ActorId,
                    [CorrelationId] = @CorrelationId
                    WHERE [CraneHoistSystemsId] = @CraneHoistSystemsId
                      AND [TenantId] = @TenantId
                      AND ISNULL([IsDeleted], 0) = 0;
                COMMIT TRANSACTION;
            END TRY
            BEGIN CATCH
                IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
                THROW;
            END CATCH;
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [MachineSafety].[usp_CraneHoistSystems_GetById]
            (
                @CraneHoistSystemsId UNIQUEIDENTIFIER
            )
            AS
            BEGIN
                SET NOCOUNT ON;

                SELECT [CraneHoistSystemsId], [TenantId], [SiteId], [Status], [AssetTag], [AssetTitle], [AssetType], [RatedLoadCapacityKg], [LastObservedLoadKg], [LastLiftAtUtc], [OverloadEventCount], [OverloadFlagged], [InspectionDueAtUtc], [LastInspectionAtUtc], [LastInspectionPassed], [OperatorUserId], [OperatorQualifiedAtUtc], [OperatorQualificationExpiresAtUtc], [OperatorQualificationSuspended], [LastRiggingInspectionAtUtc], [LastRiggingDefectDetected], [CreatedAtUtc], [CreatedBy]
                FROM [MachineSafety].[CraneHoistSystems]
                WHERE [CraneHoistSystemsId] = @CraneHoistSystemsId
                  AND ISNULL([IsDeleted], 0) = 0;
            END;
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [MachineSafety].[usp_CraneHoistSystems_GetAll]
            AS
            BEGIN
                SET NOCOUNT ON;

                SELECT [CraneHoistSystemsId], [TenantId], [SiteId], [Status], [AssetTag], [AssetTitle], [AssetType], [RatedLoadCapacityKg], [LastObservedLoadKg], [LastLiftAtUtc], [OverloadEventCount], [OverloadFlagged], [InspectionDueAtUtc], [LastInspectionAtUtc], [LastInspectionPassed], [OperatorUserId], [OperatorQualifiedAtUtc], [OperatorQualificationExpiresAtUtc], [OperatorQualificationSuspended], [LastRiggingInspectionAtUtc], [LastRiggingDefectDetected], [CreatedAtUtc], [CreatedBy]
                FROM [MachineSafety].[CraneHoistSystems]
                WHERE ISNULL([IsDeleted], 0) = 0;
            END;
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [MachineSafety].[usp_CraneHoistSystems_GetItems]
                @TenantId UNIQUEIDENTIFIER,
                @SiteId   UNIQUEIDENTIFIER = NULL,
                @FromUtc  DATETIME2(3),
                @ToUtc    DATETIME2(3)
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT
                    [CraneHoistSystemsId],
                    [TenantId],
                    [SiteId],
                    [AssetTag],
                    [AssetTitle],
                    [AssetType],
                    [RatedLoadCapacityKg],
                    [LastObservedLoadKg],
                    [OverloadEventCount],
                    [InspectionDueAtUtc],
                    [OperatorQualificationExpiresAtUtc],
                    [Status] AS [SystemStatus],
                    [CreatedAtUtc],
                    [CreatedBy]
                FROM [MachineSafety].[CraneHoistSystems]
                WHERE TenantId = @TenantId
                  AND (@SiteId IS NULL OR SiteId = @SiteId)
                  AND ISNULL([IsDeleted], 0) = 0
                  AND CreatedAtUtc >= @FromUtc
                  AND CreatedAtUtc <= @ToUtc
                ORDER BY CreatedAtUtc DESC;
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [MachineSafety].[usp_CraneHoistSystems_GetItemById]
                @TenantId UNIQUEIDENTIFIER,
                @CraneHoistSystemsId UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT
                    [CraneHoistSystemsId],
                    [TenantId],
                    [SiteId],
                    [AssetTag],
                    [AssetTitle],
                    [AssetType],
                    [RatedLoadCapacityKg],
                    [LastObservedLoadKg],
                    [OverloadEventCount],
                    [InspectionDueAtUtc],
                    [OperatorQualificationExpiresAtUtc],
                    [Status] AS [SystemStatus],
                    [CreatedAtUtc],
                    [CreatedBy]
                FROM [MachineSafety].[CraneHoistSystems]
                WHERE TenantId = @TenantId
                  AND CraneHoistSystemsId = @CraneHoistSystemsId
                  AND ISNULL([IsDeleted], 0) = 0;
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [MachineSafety].[usp_CraneHoistSystems_ListDueForInspection]
                @TenantId UNIQUEIDENTIFIER,
                @SiteId   UNIQUEIDENTIFIER = NULL,
                @AsOfUtc  DATETIME2(3)
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT
                    [CraneHoistSystemsId],
                    [TenantId],
                    [SiteId],
                    [AssetTag],
                    [AssetTitle],
                    [AssetType],
                    [RatedLoadCapacityKg],
                    [LastObservedLoadKg],
                    [OverloadEventCount],
                    [InspectionDueAtUtc],
                    [OperatorQualificationExpiresAtUtc],
                    [Status] AS [SystemStatus],
                    [CreatedAtUtc],
                    [CreatedBy]
                FROM [MachineSafety].[CraneHoistSystems]
                WHERE TenantId = @TenantId
                  AND (@SiteId IS NULL OR SiteId = @SiteId)
                  AND ISNULL([IsDeleted], 0) = 0
                  AND (
                        [Status] IN (N'InspectionDue', N'InspectionOverdue')
                        OR ([InspectionDueAtUtc] IS NOT NULL AND [InspectionDueAtUtc] <= @AsOfUtc)
                      )
                ORDER BY [InspectionDueAtUtc] ASC;
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [MachineSafety].[usp_CraneHoistSystems_ListExpiringQualifications]
                @TenantId   UNIQUEIDENTIFIER,
                @SiteId     UNIQUEIDENTIFIER = NULL,
                @WithinDays INT
            AS
            SET NOCOUNT ON;
            BEGIN
                DECLARE @NowUtc DATETIME2(3) = SYSUTCDATETIME();
                DECLARE @HorizonUtc DATETIME2(3) = DATEADD(DAY, @WithinDays, @NowUtc);

                SELECT
                    [CraneHoistSystemsId],
                    [TenantId],
                    [SiteId],
                    [AssetTag],
                    [AssetTitle],
                    [AssetType],
                    [RatedLoadCapacityKg],
                    [LastObservedLoadKg],
                    [OverloadEventCount],
                    [InspectionDueAtUtc],
                    [OperatorQualificationExpiresAtUtc],
                    [Status] AS [SystemStatus],
                    [CreatedAtUtc],
                    [CreatedBy]
                FROM [MachineSafety].[CraneHoistSystems]
                WHERE TenantId = @TenantId
                  AND (@SiteId IS NULL OR SiteId = @SiteId)
                  AND ISNULL([IsDeleted], 0) = 0
                  AND ISNULL([OperatorQualificationSuspended], 0) = 0
                  AND [OperatorQualificationExpiresAtUtc] IS NOT NULL
                  AND [OperatorQualificationExpiresAtUtc] BETWEEN @NowUtc AND @HorizonUtc
                ORDER BY [OperatorQualificationExpiresAtUtc] ASC;
            END
            """);
    }

    /// <summary>
    /// Creates the <c>[HumanFactorsCognitiveLoadAnalytics]</c> schema's tables (CognitiveLoadLedger
    /// header + CognitiveLoadReading/TaskComplexityProfile/WorkloadTrendRecord children +
    /// CognitiveRiskPrediction) and stored procedures, backing
    /// CognitiveLoadLedgerDapperRepository / CognitiveRiskPredictionDapperRepository /
    /// CognitiveLoadAnalyticsReadDapperRepository (HumanFactors.CognitiveLoadAnalytics sub-module,
    /// Testing Agent build-out, 2026-08-04). Mirrors
    /// PlantSafety.Infrastructure\HumanFactors\CognitiveLoadAnalytics\Sql\Tables\
    /// CognitiveLoadAnalytics.sql and \Procedures\*.sql verbatim, minus the leading
    /// <c>USE [PlantSafety.CoreDB];</c> statements and with the <c>[PlantSafety.AuditDB]</c> /
    /// <c>[PlantSafety.ErrorDB]</c> three-part prefixes stripped down to <c>dbo.AuditLog</c> /
    /// <c>dbo.ErrorLog</c>, matching every other ApplyXSchemaAsync method above (both tables are
    /// created by <see cref="ApplySharedAuditLoggingSchemaAsync"/>).
    /// </summary>
    public static async Task ApplyHumanFactorsCognitiveLoadAnalyticsSchemaAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken);

        await conn.ExecuteAsync("""
            IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'HumanFactorsCognitiveLoadAnalytics')
                EXEC('CREATE SCHEMA HumanFactorsCognitiveLoadAnalytics AUTHORIZATION dbo;');
            """);

        await conn.ExecuteAsync("""
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CognitiveLoadLedger' AND schema_id = SCHEMA_ID('HumanFactorsCognitiveLoadAnalytics'))
            BEGIN
                CREATE TABLE [HumanFactorsCognitiveLoadAnalytics].[CognitiveLoadLedger]
                (
                    CognitiveLoadLedgerId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_CognitiveLoadLedger PRIMARY KEY,
                    TenantId               UNIQUEIDENTIFIER NOT NULL,
                    SiteId                 UNIQUEIDENTIFIER NULL,
                    ShiftType               NVARCHAR(20)     NOT NULL,
                    ShiftDateUtc             DATETIME2(3)     NOT NULL,
                    Status                   NVARCHAR(50)     NOT NULL CONSTRAINT DF_CognitiveLoadLedger_Status DEFAULT (N'Open'),
                    CreatedAtUtc             DATETIME2(3)     NOT NULL,
                    CreatedBy                UNIQUEIDENTIFIER NOT NULL,
                    UpdatedAtUtc             DATETIME2(3)     NULL,
                    UpdatedBy                UNIQUEIDENTIFIER NULL,
                    IsDeleted                BIT              NOT NULL CONSTRAINT DF_CognitiveLoadLedger_IsDeleted DEFAULT (0),
                    RowVersion               ROWVERSION       NOT NULL
                );

                CREATE NONCLUSTERED INDEX IX_CognitiveLoadLedger_TenantId_Site ON [HumanFactorsCognitiveLoadAnalytics].[CognitiveLoadLedger] (TenantId, SiteId, IsDeleted);
                CREATE NONCLUSTERED INDEX IX_CognitiveLoadLedger_OpenShift ON [HumanFactorsCognitiveLoadAnalytics].[CognitiveLoadLedger] (TenantId, SiteId, ShiftType, ShiftDateUtc, Status) WHERE IsDeleted = 0;
            END
            """);

        await conn.ExecuteAsync("""
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CognitiveLoadReading' AND schema_id = SCHEMA_ID('HumanFactorsCognitiveLoadAnalytics'))
            BEGIN
                CREATE TABLE [HumanFactorsCognitiveLoadAnalytics].[CognitiveLoadReading]
                (
                    CognitiveLoadReadingId  UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_CognitiveLoadReading PRIMARY KEY,
                    CognitiveLoadLedgerId    UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_CognitiveLoadReading_Ledger REFERENCES [HumanFactorsCognitiveLoadAnalytics].[CognitiveLoadLedger] (CognitiveLoadLedgerId),
                    TenantId                  UNIQUEIDENTIFIER NOT NULL,
                    WorkerUserId               UNIQUEIDENTIFIER NOT NULL,
                    TaskComplexityProfileId    UNIQUEIDENTIFIER NULL,
                    CognitiveLoadScore         DECIMAL(5,2)     NOT NULL,
                    WorkloadLevel               NVARCHAR(20)     NOT NULL,
                    MeasurementMethod           NVARCHAR(100)    NOT NULL,
                    MeasuredAtUtc                DATETIME2(3)     NOT NULL,
                    RecordedAtUtc                DATETIME2(3)     NOT NULL,
                    RecordedBy                    UNIQUEIDENTIFIER NOT NULL
                );

                CREATE NONCLUSTERED INDEX IX_CognitiveLoadReading_Ledger ON [HumanFactorsCognitiveLoadAnalytics].[CognitiveLoadReading] (CognitiveLoadLedgerId, MeasuredAtUtc);
                CREATE NONCLUSTERED INDEX IX_CognitiveLoadReading_TenantId_Worker ON [HumanFactorsCognitiveLoadAnalytics].[CognitiveLoadReading] (TenantId, WorkerUserId, MeasuredAtUtc);
            END
            """);

        await conn.ExecuteAsync("""
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'TaskComplexityProfile' AND schema_id = SCHEMA_ID('HumanFactorsCognitiveLoadAnalytics'))
            BEGIN
                CREATE TABLE [HumanFactorsCognitiveLoadAnalytics].[TaskComplexityProfile]
                (
                    TaskComplexityProfileId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_TaskComplexityProfile PRIMARY KEY,
                    CognitiveLoadLedgerId     UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_TaskComplexityProfile_Ledger REFERENCES [HumanFactorsCognitiveLoadAnalytics].[CognitiveLoadLedger] (CognitiveLoadLedgerId),
                    TenantId                   UNIQUEIDENTIFIER NOT NULL,
                    TaskCode                   NVARCHAR(50)     NOT NULL,
                    TaskName                   NVARCHAR(200)    NOT NULL,
                    ComplexityScore             INT              NOT NULL,
                    Description                 NVARCHAR(1000)   NOT NULL CONSTRAINT DF_TaskComplexityProfile_Description DEFAULT (N''),
                    DefinedAtUtc                DATETIME2(3)     NOT NULL,
                    DefinedBy                    UNIQUEIDENTIFIER NOT NULL
                );

                CREATE NONCLUSTERED INDEX IX_TaskComplexityProfile_Ledger ON [HumanFactorsCognitiveLoadAnalytics].[TaskComplexityProfile] (CognitiveLoadLedgerId);
                CREATE NONCLUSTERED INDEX IX_TaskComplexityProfile_TenantId_Code ON [HumanFactorsCognitiveLoadAnalytics].[TaskComplexityProfile] (TenantId, TaskCode);
            END
            """);

        await conn.ExecuteAsync("""
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'WorkloadTrendRecord' AND schema_id = SCHEMA_ID('HumanFactorsCognitiveLoadAnalytics'))
            BEGIN
                CREATE TABLE [HumanFactorsCognitiveLoadAnalytics].[WorkloadTrendRecord]
                (
                    WorkloadTrendRecordId   UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_WorkloadTrendRecord PRIMARY KEY,
                    CognitiveLoadLedgerId     UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_WorkloadTrendRecord_Ledger REFERENCES [HumanFactorsCognitiveLoadAnalytics].[CognitiveLoadLedger] (CognitiveLoadLedgerId),
                    TenantId                   UNIQUEIDENTIFIER NOT NULL,
                    PeriodStartUtc              DATETIME2(3)     NOT NULL,
                    PeriodEndUtc                 DATETIME2(3)     NOT NULL,
                    MeanCognitiveLoadScore       DECIMAL(5,2)     NOT NULL,
                    OverloadEventCount            INT              NOT NULL,
                    ReadingCount                   INT              NOT NULL,
                    RecalculatedAtUtc               DATETIME2(3)     NOT NULL,
                    RecalculatedBy                   UNIQUEIDENTIFIER NOT NULL
                );

                CREATE NONCLUSTERED INDEX IX_WorkloadTrendRecord_Ledger ON [HumanFactorsCognitiveLoadAnalytics].[WorkloadTrendRecord] (CognitiveLoadLedgerId, RecalculatedAtUtc);
            END
            """);

        await conn.ExecuteAsync("""
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CognitiveRiskPrediction' AND schema_id = SCHEMA_ID('HumanFactorsCognitiveLoadAnalytics'))
            BEGIN
                CREATE TABLE [HumanFactorsCognitiveLoadAnalytics].[CognitiveRiskPrediction]
                (
                    CognitiveRiskPredictionId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_CognitiveRiskPrediction PRIMARY KEY,
                    TenantId                   UNIQUEIDENTIFIER NOT NULL,
                    SiteId                     UNIQUEIDENTIFIER NULL,
                    WorkerUserId                UNIQUEIDENTIFIER NOT NULL,
                    SourceLedgerId               UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_CognitiveRiskPrediction_Ledger REFERENCES [HumanFactorsCognitiveLoadAnalytics].[CognitiveLoadLedger] (CognitiveLoadLedgerId),
                    PredictedRiskLevel            NVARCHAR(20)     NOT NULL,
                    PredictionScore                DECIMAL(6,4)     NOT NULL,
                    ModelVersion                    NVARCHAR(100)    NOT NULL,
                    PredictionBasis                  NVARCHAR(1000)   NOT NULL CONSTRAINT DF_CognitiveRiskPrediction_Basis DEFAULT (N''),
                    Status                            NVARCHAR(20)     NOT NULL CONSTRAINT DF_CognitiveRiskPrediction_Status DEFAULT (N'Generated'),
                    GeneratedAtUtc                     DATETIME2(3)     NOT NULL,
                    GeneratedBy                         UNIQUEIDENTIFIER NOT NULL,
                    AcknowledgedAtUtc                    DATETIME2(3)     NULL,
                    AcknowledgedBy                        UNIQUEIDENTIFIER NULL,
                    AcknowledgementNotes                   NVARCHAR(1000)   NULL,
                    IsDeleted                                BIT              NOT NULL CONSTRAINT DF_CognitiveRiskPrediction_IsDeleted DEFAULT (0)
                );

                CREATE NONCLUSTERED INDEX IX_CognitiveRiskPrediction_TenantId_Site ON [HumanFactorsCognitiveLoadAnalytics].[CognitiveRiskPrediction] (TenantId, SiteId, IsDeleted);
                CREATE NONCLUSTERED INDEX IX_CognitiveRiskPrediction_Elevated ON [HumanFactorsCognitiveLoadAnalytics].[CognitiveRiskPrediction] (TenantId, SiteId, PredictedRiskLevel, Status) WHERE IsDeleted = 0;
                CREATE NONCLUSTERED INDEX IX_CognitiveRiskPrediction_SourceLedger ON [HumanFactorsCognitiveLoadAnalytics].[CognitiveRiskPrediction] (SourceLedgerId);
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [HumanFactorsCognitiveLoadAnalytics].[usp_CognitiveLoadLedger_Insert]
                @CognitiveLoadLedgerId UNIQUEIDENTIFIER,
                @TenantId UNIQUEIDENTIFIER,
                @SiteId UNIQUEIDENTIFIER = NULL,
                @ShiftType NVARCHAR(20),
                @ShiftDateUtc DATETIME2(3),
                @Status NVARCHAR(50),
                @CreatedAtUtc DATETIME2(3),
                @CreatedBy UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN
                BEGIN TRY
                    INSERT INTO [HumanFactorsCognitiveLoadAnalytics].[CognitiveLoadLedger]
                    (CognitiveLoadLedgerId, TenantId, SiteId, ShiftType, ShiftDateUtc, Status, CreatedAtUtc, CreatedBy, IsDeleted)
                    VALUES
                    (@CognitiveLoadLedgerId, @TenantId, @SiteId, @ShiftType, @ShiftDateUtc, @Status, @CreatedAtUtc, @CreatedBy, 0);

                    BEGIN TRY
                        INSERT INTO dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
                        VALUES (N'CognitiveLoadLedger', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'CognitiveLoadLedgerId=', @CognitiveLoadLedgerId), 0, OBJECT_NAME(@@PROCID));
                    END TRY
                    BEGIN CATCH
                    END CATCH;
                END TRY
                BEGIN CATCH
                    DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'CognitiveLoadLedgerId=', @CognitiveLoadLedgerId, N'; User=', SYSTEM_USER);
                    DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

                    INSERT INTO dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
                    VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

                    THROW;
                END CATCH
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [HumanFactorsCognitiveLoadAnalytics].[usp_CognitiveLoadLedger_GetById]
                @CognitiveLoadLedgerId UNIQUEIDENTIFIER,
                @TenantId UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT CognitiveLoadLedgerId, TenantId, SiteId, ShiftType, ShiftDateUtc, Status, CreatedAtUtc, CreatedBy
                FROM [HumanFactorsCognitiveLoadAnalytics].[CognitiveLoadLedger]
                WHERE CognitiveLoadLedgerId = @CognitiveLoadLedgerId AND TenantId = @TenantId AND IsDeleted = 0;
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [HumanFactorsCognitiveLoadAnalytics].[usp_CognitiveLoadLedger_GetOpenByShift]
                @TenantId UNIQUEIDENTIFIER,
                @SiteId UNIQUEIDENTIFIER = NULL,
                @ShiftType NVARCHAR(20),
                @ShiftDateUtc DATETIME2(3)
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT TOP (1) CognitiveLoadLedgerId, TenantId, SiteId, ShiftType, ShiftDateUtc, Status, CreatedAtUtc, CreatedBy
                FROM [HumanFactorsCognitiveLoadAnalytics].[CognitiveLoadLedger]
                WHERE TenantId = @TenantId
                  AND ((@SiteId IS NULL AND SiteId IS NULL) OR SiteId = @SiteId)
                  AND ShiftType = @ShiftType
                  AND ShiftDateUtc = @ShiftDateUtc
                  AND Status = N'Open'
                  AND IsDeleted = 0;
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [HumanFactorsCognitiveLoadAnalytics].[usp_CognitiveLoadLedger_UpdateStatus]
                @CognitiveLoadLedgerId UNIQUEIDENTIFIER,
                @TenantId UNIQUEIDENTIFIER,
                @Status NVARCHAR(50),
                @UpdatedBy UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN
                UPDATE [HumanFactorsCognitiveLoadAnalytics].[CognitiveLoadLedger]
                SET Status = @Status, UpdatedAtUtc = SYSUTCDATETIME(), UpdatedBy = @UpdatedBy
                WHERE CognitiveLoadLedgerId = @CognitiveLoadLedgerId AND TenantId = @TenantId AND IsDeleted = 0;

                SELECT @@ROWCOUNT AS RowsAffected;
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [HumanFactorsCognitiveLoadAnalytics].[usp_CognitiveLoadReading_Insert]
                @CognitiveLoadReadingId UNIQUEIDENTIFIER,
                @CognitiveLoadLedgerId UNIQUEIDENTIFIER,
                @TenantId UNIQUEIDENTIFIER,
                @WorkerUserId UNIQUEIDENTIFIER,
                @TaskComplexityProfileId UNIQUEIDENTIFIER = NULL,
                @CognitiveLoadScore DECIMAL(5,2),
                @WorkloadLevel NVARCHAR(20),
                @MeasurementMethod NVARCHAR(100),
                @MeasuredAtUtc DATETIME2(3),
                @RecordedAtUtc DATETIME2(3),
                @RecordedBy UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN
                IF NOT EXISTS (SELECT 1 FROM [HumanFactorsCognitiveLoadAnalytics].[CognitiveLoadReading] WHERE CognitiveLoadReadingId = @CognitiveLoadReadingId)
                INSERT INTO [HumanFactorsCognitiveLoadAnalytics].[CognitiveLoadReading]
                (CognitiveLoadReadingId, CognitiveLoadLedgerId, TenantId, WorkerUserId, TaskComplexityProfileId, CognitiveLoadScore, WorkloadLevel, MeasurementMethod, MeasuredAtUtc, RecordedAtUtc, RecordedBy)
                VALUES
                (@CognitiveLoadReadingId, @CognitiveLoadLedgerId, @TenantId, @WorkerUserId, @TaskComplexityProfileId, @CognitiveLoadScore, @WorkloadLevel, @MeasurementMethod, @MeasuredAtUtc, @RecordedAtUtc, @RecordedBy);
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [HumanFactorsCognitiveLoadAnalytics].[usp_CognitiveLoadReading_ListByLedger]
                @CognitiveLoadLedgerId UNIQUEIDENTIFIER,
                @TenantId UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT CognitiveLoadReadingId, WorkerUserId, TaskComplexityProfileId, CognitiveLoadScore, WorkloadLevel, MeasurementMethod, MeasuredAtUtc, RecordedAtUtc, RecordedBy
                FROM [HumanFactorsCognitiveLoadAnalytics].[CognitiveLoadReading]
                WHERE CognitiveLoadLedgerId = @CognitiveLoadLedgerId AND TenantId = @TenantId
                ORDER BY MeasuredAtUtc ASC;
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [HumanFactorsCognitiveLoadAnalytics].[usp_TaskComplexityProfile_Insert]
                @TaskComplexityProfileId UNIQUEIDENTIFIER,
                @CognitiveLoadLedgerId UNIQUEIDENTIFIER,
                @TenantId UNIQUEIDENTIFIER,
                @TaskCode NVARCHAR(50),
                @TaskName NVARCHAR(200),
                @ComplexityScore INT,
                @Description NVARCHAR(1000),
                @DefinedAtUtc DATETIME2(3),
                @DefinedBy UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN
                IF NOT EXISTS (SELECT 1 FROM [HumanFactorsCognitiveLoadAnalytics].[TaskComplexityProfile] WHERE TaskComplexityProfileId = @TaskComplexityProfileId)
                INSERT INTO [HumanFactorsCognitiveLoadAnalytics].[TaskComplexityProfile]
                (TaskComplexityProfileId, CognitiveLoadLedgerId, TenantId, TaskCode, TaskName, ComplexityScore, Description, DefinedAtUtc, DefinedBy)
                VALUES
                (@TaskComplexityProfileId, @CognitiveLoadLedgerId, @TenantId, @TaskCode, @TaskName, @ComplexityScore, @Description, @DefinedAtUtc, @DefinedBy);
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [HumanFactorsCognitiveLoadAnalytics].[usp_TaskComplexityProfile_ListByLedger]
                @CognitiveLoadLedgerId UNIQUEIDENTIFIER,
                @TenantId UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT TaskComplexityProfileId, TaskCode, TaskName, ComplexityScore, Description, DefinedAtUtc, DefinedBy
                FROM [HumanFactorsCognitiveLoadAnalytics].[TaskComplexityProfile]
                WHERE CognitiveLoadLedgerId = @CognitiveLoadLedgerId AND TenantId = @TenantId
                ORDER BY DefinedAtUtc ASC;
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [HumanFactorsCognitiveLoadAnalytics].[usp_TaskComplexityProfile_GetByCode]
                @TenantId UNIQUEIDENTIFIER,
                @TaskCode NVARCHAR(50)
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT TOP (1)
                    TaskComplexityProfileId, CognitiveLoadLedgerId, TaskCode, TaskName, ComplexityScore, Description, DefinedAtUtc
                FROM [HumanFactorsCognitiveLoadAnalytics].[TaskComplexityProfile]
                WHERE TenantId = @TenantId AND TaskCode = @TaskCode
                ORDER BY DefinedAtUtc DESC;
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [HumanFactorsCognitiveLoadAnalytics].[usp_WorkloadTrendRecord_Insert]
                @WorkloadTrendRecordId UNIQUEIDENTIFIER,
                @CognitiveLoadLedgerId UNIQUEIDENTIFIER,
                @TenantId UNIQUEIDENTIFIER,
                @PeriodStartUtc DATETIME2(3),
                @PeriodEndUtc DATETIME2(3),
                @MeanCognitiveLoadScore DECIMAL(5,2),
                @OverloadEventCount INT,
                @ReadingCount INT,
                @RecalculatedAtUtc DATETIME2(3),
                @RecalculatedBy UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN
                IF NOT EXISTS (SELECT 1 FROM [HumanFactorsCognitiveLoadAnalytics].[WorkloadTrendRecord] WHERE WorkloadTrendRecordId = @WorkloadTrendRecordId)
                INSERT INTO [HumanFactorsCognitiveLoadAnalytics].[WorkloadTrendRecord]
                (WorkloadTrendRecordId, CognitiveLoadLedgerId, TenantId, PeriodStartUtc, PeriodEndUtc, MeanCognitiveLoadScore, OverloadEventCount, ReadingCount, RecalculatedAtUtc, RecalculatedBy)
                VALUES
                (@WorkloadTrendRecordId, @CognitiveLoadLedgerId, @TenantId, @PeriodStartUtc, @PeriodEndUtc, @MeanCognitiveLoadScore, @OverloadEventCount, @ReadingCount, @RecalculatedAtUtc, @RecalculatedBy);
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [HumanFactorsCognitiveLoadAnalytics].[usp_WorkloadTrendRecord_ListByLedger]
                @CognitiveLoadLedgerId UNIQUEIDENTIFIER,
                @TenantId UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT WorkloadTrendRecordId, PeriodStartUtc, PeriodEndUtc, MeanCognitiveLoadScore, OverloadEventCount, ReadingCount, RecalculatedAtUtc, RecalculatedBy
                FROM [HumanFactorsCognitiveLoadAnalytics].[WorkloadTrendRecord]
                WHERE CognitiveLoadLedgerId = @CognitiveLoadLedgerId AND TenantId = @TenantId
                ORDER BY RecalculatedAtUtc ASC;
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [HumanFactorsCognitiveLoadAnalytics].[usp_CognitiveRiskPrediction_GetById]
                @CognitiveRiskPredictionId UNIQUEIDENTIFIER,
                @TenantId UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT
                    CognitiveRiskPredictionId, TenantId, SiteId, WorkerUserId, SourceLedgerId, PredictedRiskLevel, PredictionScore,
                    ModelVersion, PredictionBasis, Status, GeneratedAtUtc, GeneratedBy, AcknowledgedAtUtc, AcknowledgedBy, AcknowledgementNotes
                FROM [HumanFactorsCognitiveLoadAnalytics].[CognitiveRiskPrediction]
                WHERE CognitiveRiskPredictionId = @CognitiveRiskPredictionId AND TenantId = @TenantId AND IsDeleted = 0;
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [HumanFactorsCognitiveLoadAnalytics].[usp_CognitiveRiskPrediction_Insert]
                @CognitiveRiskPredictionId UNIQUEIDENTIFIER,
                @TenantId UNIQUEIDENTIFIER,
                @SiteId UNIQUEIDENTIFIER = NULL,
                @WorkerUserId UNIQUEIDENTIFIER,
                @SourceLedgerId UNIQUEIDENTIFIER,
                @PredictedRiskLevel NVARCHAR(20),
                @PredictionScore DECIMAL(6,4),
                @ModelVersion NVARCHAR(100),
                @PredictionBasis NVARCHAR(1000),
                @Status NVARCHAR(20),
                @GeneratedAtUtc DATETIME2(3),
                @GeneratedBy UNIQUEIDENTIFIER
            AS
            SET NOCOUNT ON;
            BEGIN
                BEGIN TRY
                    INSERT INTO [HumanFactorsCognitiveLoadAnalytics].[CognitiveRiskPrediction]
                    (CognitiveRiskPredictionId, TenantId, SiteId, WorkerUserId, SourceLedgerId, PredictedRiskLevel, PredictionScore, ModelVersion, PredictionBasis, Status, GeneratedAtUtc, GeneratedBy, IsDeleted)
                    VALUES
                    (@CognitiveRiskPredictionId, @TenantId, @SiteId, @WorkerUserId, @SourceLedgerId, @PredictedRiskLevel, @PredictionScore, @ModelVersion, @PredictionBasis, @Status, @GeneratedAtUtc, @GeneratedBy, 0);

                    BEGIN TRY
                        INSERT INTO dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
                        VALUES (N'CognitiveRiskPrediction', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'CognitiveRiskPredictionId=', @CognitiveRiskPredictionId), 0, OBJECT_NAME(@@PROCID));
                    END TRY
                    BEGIN CATCH
                    END CATCH;
                END TRY
                BEGIN CATCH
                    DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'CognitiveRiskPredictionId=', @CognitiveRiskPredictionId, N'; User=', SYSTEM_USER);
                    DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

                    INSERT INTO dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
                    VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

                    THROW;
                END CATCH
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [HumanFactorsCognitiveLoadAnalytics].[usp_CognitiveRiskPrediction_ListElevatedByTenant]
                @TenantId UNIQUEIDENTIFIER,
                @SiteId UNIQUEIDENTIFIER = NULL
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT
                    CognitiveRiskPredictionId, WorkerUserId, SourceLedgerId, PredictedRiskLevel, PredictionScore,
                    ModelVersion, Status, GeneratedAtUtc, AcknowledgedAtUtc
                FROM [HumanFactorsCognitiveLoadAnalytics].[CognitiveRiskPrediction]
                WHERE TenantId = @TenantId
                  AND ((@SiteId IS NULL) OR SiteId = @SiteId)
                  AND PredictedRiskLevel IN (N'Elevated', N'Critical')
                  AND Status <> N'Dismissed'
                  AND IsDeleted = 0
                ORDER BY GeneratedAtUtc DESC;
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [HumanFactorsCognitiveLoadAnalytics].[usp_CognitiveRiskPrediction_UpdateAcknowledgement]
                @CognitiveRiskPredictionId UNIQUEIDENTIFIER,
                @TenantId UNIQUEIDENTIFIER,
                @Status NVARCHAR(20),
                @AcknowledgedAtUtc DATETIME2(3) = NULL,
                @AcknowledgedBy UNIQUEIDENTIFIER = NULL,
                @AcknowledgementNotes NVARCHAR(1000) = NULL
            AS
            SET NOCOUNT ON;
            BEGIN
                UPDATE [HumanFactorsCognitiveLoadAnalytics].[CognitiveRiskPrediction]
                SET Status = @Status,
                    AcknowledgedAtUtc = @AcknowledgedAtUtc,
                    AcknowledgedBy = @AcknowledgedBy,
                    AcknowledgementNotes = @AcknowledgementNotes
                WHERE CognitiveRiskPredictionId = @CognitiveRiskPredictionId AND TenantId = @TenantId AND IsDeleted = 0;

                SELECT @@ROWCOUNT AS RowsAffected;
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [HumanFactorsCognitiveLoadAnalytics].[usp_CognitiveLoadAnalytics_GetCognitiveLoadByWorker]
                @TenantId UNIQUEIDENTIFIER,
                @WorkerUserId UNIQUEIDENTIFIER,
                @PeriodStartUtc DATETIME2(3),
                @PeriodEndUtc DATETIME2(3)
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT
                    @WorkerUserId AS WorkerUserId,
                    @PeriodStartUtc AS PeriodStartUtc,
                    @PeriodEndUtc AS PeriodEndUtc,
                    ISNULL(AVG(r.CognitiveLoadScore), 0) AS MeanCognitiveLoadScore,
                    ISNULL(MAX(r.CognitiveLoadScore), 0) AS PeakCognitiveLoadScore,
                    SUM(CASE WHEN r.WorkloadLevel = N'Overload' THEN 1 ELSE 0 END) AS OverloadEventCount,
                    COUNT(r.CognitiveLoadReadingId) AS ReadingCount
                FROM [HumanFactorsCognitiveLoadAnalytics].[CognitiveLoadReading] AS r
                WHERE r.TenantId = @TenantId
                  AND r.WorkerUserId = @WorkerUserId
                  AND r.MeasuredAtUtc >= @PeriodStartUtc
                  AND r.MeasuredAtUtc <= @PeriodEndUtc
                HAVING COUNT(r.CognitiveLoadReadingId) > 0;
            END
            """);

        await conn.ExecuteAsync("""
            CREATE OR ALTER PROCEDURE [HumanFactorsCognitiveLoadAnalytics].[usp_CognitiveLoadAnalytics_GetWorkloadTrendByShift]
                @TenantId UNIQUEIDENTIFIER,
                @SiteId UNIQUEIDENTIFIER = NULL,
                @ShiftType NVARCHAR(20),
                @PeriodStartUtc DATETIME2(3),
                @PeriodEndUtc DATETIME2(3)
            AS
            SET NOCOUNT ON;
            BEGIN
                SELECT
                    t.WorkloadTrendRecordId, t.CognitiveLoadLedgerId, l.ShiftType,
                    t.PeriodStartUtc, t.PeriodEndUtc, t.MeanCognitiveLoadScore, t.OverloadEventCount, t.ReadingCount, t.RecalculatedAtUtc
                FROM [HumanFactorsCognitiveLoadAnalytics].[WorkloadTrendRecord] AS t
                INNER JOIN [HumanFactorsCognitiveLoadAnalytics].[CognitiveLoadLedger] AS l ON l.CognitiveLoadLedgerId = t.CognitiveLoadLedgerId
                WHERE l.TenantId = @TenantId
                  AND ((@SiteId IS NULL AND l.SiteId IS NULL) OR l.SiteId = @SiteId)
                  AND l.ShiftType = @ShiftType
                  AND t.PeriodStartUtc >= @PeriodStartUtc
                  AND t.PeriodEndUtc <= @PeriodEndUtc
                  AND l.IsDeleted = 0
                ORDER BY t.RecalculatedAtUtc ASC;
            END
            """);
    }
}

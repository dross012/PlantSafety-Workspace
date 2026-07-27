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
                    MessageId     UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
                    EventType     NVARCHAR(200)    NOT NULL,
                    SchemaVersion NVARCHAR(20)     NOT NULL DEFAULT '2026.1',
                    TenantId      UNIQUEIDENTIFIER NOT NULL,
                    ActorId       UNIQUEIDENTIFIER NOT NULL,
                    CorrelationId UNIQUEIDENTIFIER NOT NULL,
                    OccurredAt    DATETIME2(3)     NOT NULL,
                    Payload       NVARCHAR(MAX)    NOT NULL,
                    Processed     BIT              NOT NULL DEFAULT 0,
                    ProcessedAt   DATETIME2(3)         NULL
                );
            END;

            IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES
                           WHERE TABLE_SCHEMA = 'System' AND TABLE_NAME = 'ProcessedEvents')
            BEGIN
                CREATE TABLE [System].[ProcessedEvents] (
                    EventId     UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                    ProcessedAt DATETIME2(3)     NOT NULL
                );
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
    /// <see cref="ApplySharedAuditLoggingSchemaAsync"/>, no statements need to be stripped here --
    /// the write procedures' cross-database AuditLog/ErrorLog writes are already wrapped in their
    /// own defensive BEGIN TRY/END TRY (empty CATCH) blocks in the canonical scripts, so they
    /// silently no-op against the Testcontainers instance's single-database topology instead of
    /// failing the surrounding operation.
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
                        INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
                        VALUES (N'Incident', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'IncidentId=', @IncidentId, N'; CorrelationId=', @CorrelationId), 0, OBJECT_NAME(@@PROCID));
                    END TRY
                    BEGIN CATCH
                    END CATCH;
                END TRY
                BEGIN CATCH
                    DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'IncidentId=', @IncidentId, N'; CorrelationId=', @CorrelationId, N'; User=', SYSTEM_USER);
                    DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

                    BEGIN TRY
                        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
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

                    BEGIN TRY
                        INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
                        VALUES (N'Incident', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'IncidentId=', @IncidentId, N'; CorrelationId=', @CorrelationId), 0, OBJECT_NAME(@@PROCID));
                    END TRY
                    BEGIN CATCH
                    END CATCH;
                END TRY
                BEGIN CATCH
                    DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'IncidentId=', @IncidentId, N'; CorrelationId=', @CorrelationId, N'; User=', SYSTEM_USER);
                    DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

                    BEGIN TRY
                        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
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

                    BEGIN TRY
                        INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
                        VALUES (N'Incident', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'IncidentId=', @IncidentId, N'; CorrelationId=', @CorrelationId), 0, OBJECT_NAME(@@PROCID));
                    END TRY
                    BEGIN CATCH
                    END CATCH;
                END TRY
                BEGIN CATCH
                    DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'IncidentId=', @IncidentId, N'; CorrelationId=', @CorrelationId, N'; User=', SYSTEM_USER);
                    DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

                    BEGIN TRY
                        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
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
                    LastModifiedByActorId, LastModifiedAtUtc, CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
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
                    LastModifiedByActorId, LastModifiedAtUtc, CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
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
                    LastModifiedByActorId, LastModifiedAtUtc, CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
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
                        INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
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
                        INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
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
                        INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
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
                        INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
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
                        INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
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
                        INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
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
}

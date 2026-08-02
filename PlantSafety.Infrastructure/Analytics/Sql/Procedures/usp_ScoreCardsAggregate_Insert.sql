-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.ScoreCards
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_ScoreCardsAggregate_Insert.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Inserts a new ScoreCards aggregate root row. Backs IScoreCardsRepository.AddAsync.
--                  TRY/CATCH with AuditDB/ErrorDB logging per the canonical write-procedure
--                  template (mirrors usp_Permits_Insert.sql). Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Analytics].[usp_ScoreCardsAggregate_Insert]
    @ScoreCardsId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(50),
    @ScoreCardName NVARCHAR(400),
    @ReportingPeriod NVARCHAR(50),
    @KpiCategory NVARCHAR(200),
    @PerformanceIndex DECIMAL(18,4),
    @GeneratedForSiteId UNIQUEIDENTIFIER = NULL,
    @GeneratedAtUtc DATETIME2(3),
    @PublishedByUserId UNIQUEIDENTIFIER = NULL,
    @PublishedAtUtc DATETIME2(3) = NULL,
    @FinalizedByUserId UNIQUEIDENTIFIER = NULL,
    @FinalizedAtUtc DATETIME2(3) = NULL,
    @ClosedByUserId UNIQUEIDENTIFIER = NULL,
    @ClosedAtUtc DATETIME2(3) = NULL,
    @ArchivedAtUtc DATETIME2(3) = NULL,
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        INSERT INTO [Analytics].[ScoreCardsAggregate]
        (
            ScoreCardsId, TenantId, SiteId, Status, ScoreCardName, ReportingPeriod, KpiCategory, PerformanceIndex, GeneratedForSiteId, GeneratedAtUtc, PublishedByUserId, PublishedAtUtc, FinalizedByUserId, FinalizedAtUtc, ClosedByUserId, ClosedAtUtc, ArchivedAtUtc, CreatedAtUtc, CreatedBy, IsDeleted
        )
        VALUES
        (
            @ScoreCardsId, @TenantId, @SiteId, @Status, @ScoreCardName, @ReportingPeriod, @KpiCategory, @PerformanceIndex, @GeneratedForSiteId, @GeneratedAtUtc, @PublishedByUserId, @PublishedAtUtc, @FinalizedByUserId, @FinalizedAtUtc, @ClosedByUserId, @ClosedAtUtc, @ArchivedAtUtc, @CreatedAtUtc, @CreatedBy, 0
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'ScoreCardsAggregate', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'ScoreCardsId=', @ScoreCardsId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ScoreCardsId=', @ScoreCardsId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
            N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
            N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO

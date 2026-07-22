-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Scorecards
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_Scorecard_Insert.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Inserts a new Scorecard aggregate root row. Backs IScorecardRepository.AddAsync.
--                  TRY/CATCH with AuditDB/ErrorDB logging per the canonical write-procedure
--                  template. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Scorecards].[usp_Scorecard_Insert]
    @ScorecardId       UNIQUEIDENTIFIER,
    @TenantId          UNIQUEIDENTIFIER,
    @SiteId            UNIQUEIDENTIFIER = NULL,
    @Status            NVARCHAR(30),
    @ScorecardTitle    NVARCHAR(400),
    @ScorecardVersion  INT,
    @Category          NVARCHAR(30),
    @PeriodType        NVARCHAR(30),
    @PeriodStartUtc    DATETIME2(3),
    @PeriodEndUtc      DATETIME2(3),
    @OwnerId           UNIQUEIDENTIFIER,
    @PublishedAtUtc    DATETIME2(3) = NULL,
    @DistributionCount INT,
    @ReviewRequired    BIT,
    @ReviewCount       INT,
    @CreatedAtUtc      DATETIME2(3),
    @CreatedBy         UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        INSERT INTO [Scorecards].[Scorecard]
        (
            ScorecardId, TenantId, SiteId, Status, ScorecardTitle, ScorecardVersion, Category, PeriodType,
            PeriodStartUtc, PeriodEndUtc, OwnerId, PublishedAtUtc, DistributionCount, ReviewRequired,
            ReviewCount, CreatedAtUtc, CreatedBy, IsDeleted
        )
        VALUES
        (
            @ScorecardId, @TenantId, @SiteId, @Status, @ScorecardTitle, @ScorecardVersion, @Category, @PeriodType,
            @PeriodStartUtc, @PeriodEndUtc, @OwnerId, @PublishedAtUtc, @DistributionCount, @ReviewRequired,
            @ReviewCount, @CreatedAtUtc, @CreatedBy, 0
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'Scorecard', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'ScorecardId=', @ScorecardId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ScorecardId=', @ScorecardId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO

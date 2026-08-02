-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyEvents.NearMisses
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_NearMisses_Insert.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Inserts a new NearMisses aggregate root row. Backs
--                  INearMissesRepository.AddAsync (Domain write-side repository). TRY/CATCH with
--                  AuditDB/ErrorDB logging per the canonical write-procedure template (mirrors
--                  usp_PredictiveMaintenanceAlert_Insert.sql / usp_Permits_Insert.sql).
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [SafetyEvents].[usp_NearMisses_Insert]
    @NearMissesId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(50),
    @Category NVARCHAR(100),
    @PotentialSeverity NVARCHAR(50),
    @ReportedAtUtc DATETIME2(3),
    @ReportedBy UNIQUEIDENTIFIER,
    @ReviewStartedBy UNIQUEIDENTIFIER = NULL,
    @ReviewStartedAtUtc DATETIME2(3) = NULL,
    @CompletedBy UNIQUEIDENTIFIER = NULL,
    @Findings NVARCHAR(2000) = NULL,
    @CompletedAtUtc DATETIME2(3) = NULL,
    @ClosedBy UNIQUEIDENTIFIER = NULL,
    @ClosedAtUtc DATETIME2(3) = NULL,
    @ArchivedAtUtc DATETIME2(3) = NULL,
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        INSERT INTO [SafetyEvents].[NearMisses]
        (
            NearMissId, TenantId, SiteId, Status, Category, PotentialSeverity, ReportedAtUtc, ReportedBy,
            ReviewStartedBy, ReviewStartedAtUtc, CompletedBy, Findings, CompletedAtUtc,
            ClosedBy, ClosedAtUtc, ArchivedAtUtc, CreatedAtUtc, CreatedBy, IsDeleted
        )
        VALUES
        (
            @NearMissesId, @TenantId, @SiteId, @Status, @Category, @PotentialSeverity, @ReportedAtUtc, @ReportedBy,
            @ReviewStartedBy, @ReviewStartedAtUtc, @CompletedBy, @Findings, @CompletedAtUtc,
            @ClosedBy, @ClosedAtUtc, @ArchivedAtUtc, @CreatedAtUtc, @CreatedBy, 0
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'NearMisses', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'NearMissesId=', @NearMissesId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'NearMissesId=', @NearMissesId, N'; User=', SYSTEM_USER);
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

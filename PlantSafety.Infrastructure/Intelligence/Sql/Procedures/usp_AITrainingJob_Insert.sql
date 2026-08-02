-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Intelligence.AITraining
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_AITrainingJob_Insert.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Inserts a new Intelligence.AITraining AITrainingJob aggregate root row. Backs
--                  IAITrainingJobRepository.AddAsync (Domain write-side repository). TRY/CATCH with
--                  AuditDB/ErrorDB logging per the canonical write-procedure template (mirrors
--                  usp_RCA_Insert.sql).
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Intelligence].[usp_AITrainingJob_Insert]
    @AITrainingJobId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @ModelName NVARCHAR(200),
    @ModelVersion NVARCHAR(50),
    @DatasetReference NVARCHAR(400) = NULL,
    @Status NVARCHAR(50),
    @StartedAtUtc DATETIME2(3),
    @InitiatedBy UNIQUEIDENTIFIER,
    @RunStartedBy UNIQUEIDENTIFIER = NULL,
    @RunStartedAtUtc DATETIME2(3) = NULL,
    @ModelArtifactReference NVARCHAR(400) = NULL,
    @CompletedAtUtc DATETIME2(3) = NULL,
    @FailureReason NVARCHAR(MAX) = NULL,
    @FailedAtUtc DATETIME2(3) = NULL,
    @ArchivedAtUtc DATETIME2(3) = NULL,
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        INSERT INTO [Intelligence].[AITrainingJob]
        (
            AITrainingJobId, TenantId, SiteId, ModelName, ModelVersion, DatasetReference, Status, StartedAtUtc, InitiatedBy,
            RunStartedBy, RunStartedAtUtc, ModelArtifactReference, CompletedAtUtc, FailureReason, FailedAtUtc, ArchivedAtUtc,
            CreatedAtUtc, CreatedBy, IsDeleted
        )
        VALUES
        (
            @AITrainingJobId, @TenantId, @SiteId, @ModelName, @ModelVersion, @DatasetReference, @Status, @StartedAtUtc, @InitiatedBy,
            @RunStartedBy, @RunStartedAtUtc, @ModelArtifactReference, @CompletedAtUtc, @FailureReason, @FailedAtUtc, @ArchivedAtUtc,
            @CreatedAtUtc, @CreatedBy, 0
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'AITrainingJob', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'AITrainingJobId=', @AITrainingJobId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'AITrainingJobId=', @AITrainingJobId, N'; User=', SYSTEM_USER);
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

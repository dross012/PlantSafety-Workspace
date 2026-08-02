-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         ContinuousImprovement.RCA
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_RCA_Insert.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Inserts a new ContinuousImprovement RCA aggregate root row. Backs
--                  IRCARepository.AddAsync (Domain write-side repository). TRY/CATCH with
--                  AuditDB/ErrorDB logging per the canonical write-procedure template (mirrors
--                  usp_RootCauseAnalysis_Insert.sql / usp_CorrectiveAction_Create.sql). Does not
--                  collide with the pre-existing usp_RCA_GetRcaRecords read-list proc.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [ContinuousImprovement].[usp_RCA_Insert]
    @RcaId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @ProblemStatement NVARCHAR(MAX),
    @Methodology NVARCHAR(50),
    @Status NVARCHAR(50),
    @InitiatedAtUtc DATETIME2(3),
    @OwnerUserId UNIQUEIDENTIFIER,
    @StartedBy UNIQUEIDENTIFIER = NULL,
    @StartedAtUtc DATETIME2(3) = NULL,
    @SubmittedBy UNIQUEIDENTIFIER = NULL,
    @Findings NVARCHAR(MAX) = NULL,
    @SubmittedAtUtc DATETIME2(3) = NULL,
    @ClosedBy UNIQUEIDENTIFIER = NULL,
    @RootCauseSummary NVARCHAR(MAX) = NULL,
    @ClosedAtUtc DATETIME2(3) = NULL,
    @ArchivedAtUtc DATETIME2(3) = NULL,
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        INSERT INTO [ContinuousImprovement].[RCA]
        (
            RcaId, TenantId, SiteId, ProblemStatement, Methodology, Status, InitiatedAtUtc, OwnerUserId,
            StartedBy, StartedAtUtc, SubmittedBy, Findings, SubmittedAtUtc,
            ClosedBy, RootCauseSummary, ClosedAtUtc, ArchivedAtUtc,
            CreatedAtUtc, CreatedBy, IsDeleted
        )
        VALUES
        (
            @RcaId, @TenantId, @SiteId, @ProblemStatement, @Methodology, @Status, @InitiatedAtUtc, @OwnerUserId,
            @StartedBy, @StartedAtUtc, @SubmittedBy, @Findings, @SubmittedAtUtc,
            @ClosedBy, @RootCauseSummary, @ClosedAtUtc, @ArchivedAtUtc,
            @CreatedAtUtc, @CreatedBy, 0
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'RCA', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'RcaId=', @RcaId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'RcaId=', @RcaId, N'; User=', SYSTEM_USER);
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

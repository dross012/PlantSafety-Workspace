-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PermitToWork
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_PermitWorkflow_Update.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Overwrites the full mutable state of a PermitWorkflow aggregate root row.
--                  Backs IPermitWorkflowRepository.UpdateAsync. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [PermitToWork].[usp_PermitWorkflow_Update]
    @PermitWorkflowId UNIQUEIDENTIFIER,
    @Status NVARCHAR(50),
    @PermitNumber NVARCHAR(400),
    @PermitTypeCode NVARCHAR(400),
    @RequesterUserId UNIQUEIDENTIFIER = NULL,
    @IssuerUserId UNIQUEIDENTIFIER = NULL,
    @WorkDescription NVARCHAR(400),
    @RequiredControls NVARCHAR(400) = NULL,
    @RiskScore INT = NULL,
    @ControlsConfirmedBy NVARCHAR(400) = NULL,
    @ControlsImplementedAtUtc DATETIME2(3) = NULL,
    @ApprovedBy UNIQUEIDENTIFIER = NULL,
    @ApprovedAtUtc DATETIME2(3) = NULL,
    @IssuedAtUtc DATETIME2(3) = NULL,
    @ExpiresAtUtc DATETIME2(3) = NULL,
    @ClosureSignedBy UNIQUEIDENTIFIER = NULL,
    @ClosureNotes NVARCHAR(400) = NULL,
    @ClosedAtUtc DATETIME2(3) = NULL,
    @ArchivedAtUtc DATETIME2(3) = NULL,
    @RejectedReason NVARCHAR(400) = NULL,
    @UpdatedBy UNIQUEIDENTIFIER = NULL
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM [PermitToWork].[PermitWorkflow] WHERE PermitWorkflowId = @PermitWorkflowId AND IsDeleted = 0)
            THROW 50004, N'PermitWorkflow not found or has been deleted.', 1;

        UPDATE [PermitToWork].[PermitWorkflow]
        SET
            Status = @Status,
            PermitNumber = @PermitNumber,
            PermitTypeCode = @PermitTypeCode,
            RequesterUserId = @RequesterUserId,
            IssuerUserId = @IssuerUserId,
            WorkDescription = @WorkDescription,
            RequiredControls = @RequiredControls,
            RiskScore = @RiskScore,
            ControlsConfirmedBy = @ControlsConfirmedBy,
            ControlsImplementedAtUtc = @ControlsImplementedAtUtc,
            ApprovedBy = @ApprovedBy,
            ApprovedAtUtc = @ApprovedAtUtc,
            IssuedAtUtc = @IssuedAtUtc,
            ExpiresAtUtc = @ExpiresAtUtc,
            ClosureSignedBy = @ClosureSignedBy,
            ClosureNotes = @ClosureNotes,
            ClosedAtUtc = @ClosedAtUtc,
            ArchivedAtUtc = @ArchivedAtUtc,
            RejectedReason = @RejectedReason,
            UpdatedBy = @UpdatedBy,
            UpdatedAtUtc = SYSUTCDATETIME(),
            ConcurrencyToken = NEWID()
        WHERE PermitWorkflowId = @PermitWorkflowId AND IsDeleted = 0;

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'PermitWorkflow', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'PermitWorkflowId=', @PermitWorkflowId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'PermitWorkflowId=', @PermitWorkflowId, N'; User=', SYSTEM_USER);
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

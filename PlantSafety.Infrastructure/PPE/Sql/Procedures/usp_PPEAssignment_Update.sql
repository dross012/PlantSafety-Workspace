-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PPE
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_PPEAssignment_Update.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Overwrites the full mutable state of a PPEAssignment aggregate root row.
--                  Backs IPPEAssignmentRepository.UpdateAsync. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [PPE].[usp_PPEAssignment_Update]
    @PPEAssignmentId UNIQUEIDENTIFIER,
    @Status NVARCHAR(50),
    @WorkerId UNIQUEIDENTIFIER,
    @JobRole NVARCHAR(400),
    @HazardZone NVARCHAR(400),
    @AssignedPpeItems NVARCHAR(400),
    @FitTestExpiresAtUtc DATETIME2(3) = NULL,
    @ReplacementReason NVARCHAR(400) = NULL,
    @UpdatedBy UNIQUEIDENTIFIER = NULL
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM [PPE].[PPEAssignment] WHERE PPEAssignmentId = @PPEAssignmentId AND IsDeleted = 0)
            THROW 50004, N'PPEAssignment not found or has been deleted.', 1;

        UPDATE [PPE].[PPEAssignment]
        SET
            Status = @Status,
            WorkerId = @WorkerId,
            JobRole = @JobRole,
            HazardZone = @HazardZone,
            AssignedPpeItems = @AssignedPpeItems,
            FitTestExpiresAtUtc = @FitTestExpiresAtUtc,
            ReplacementReason = @ReplacementReason,
            UpdatedBy = @UpdatedBy,
            UpdatedAtUtc = SYSUTCDATETIME(),
            ConcurrencyToken = NEWID()
        WHERE PPEAssignmentId = @PPEAssignmentId AND IsDeleted = 0;

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'PPEAssignment', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'PPEAssignmentId=', @PPEAssignmentId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'PPEAssignmentId=', @PPEAssignmentId, N'; User=', SYSTEM_USER);
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

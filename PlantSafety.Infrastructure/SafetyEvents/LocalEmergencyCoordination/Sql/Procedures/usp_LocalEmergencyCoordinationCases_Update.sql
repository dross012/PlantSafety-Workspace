-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyEvents.LocalEmergencyCoordination
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_LocalEmergencyCoordinationCases_Update.sql
--  Created:        2026-08-04
--  SchemaVersion:  2026.1
--  Description:    Overwrites the mutable case-level state of a LocalEmergencyCoordination case.
--                  Backs ILocalEmergencyCoordinationRepository.UpdateAsync. @ETag enforces
--                  optimistic concurrency against the native RowVersion column. @RowsAffected is
--                  captured via @@ROWCOUNT before the audit INSERT and returned via SELECT --
--                  callers MUST invoke this proc via ExecuteScalarAsync (SET NOCOUNT ON makes
--                  ExecuteNonQuery always return -1). Mirrors usp_EmergencyManagementItems_Update.sql.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [SafetyEvents].[usp_LocalEmergencyCoordinationCases_Update]
    @LocalEmergencyCoordinationId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @Status NVARCHAR(50),
    @ClosedAtUtc DATETIME2(3) = NULL,
    @ArchivedAtUtc DATETIME2(3) = NULL,
    @ETag NVARCHAR(64) = NULL
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM [SafetyEvents].[LocalEmergencyCoordinationCases] WHERE LocalEmergencyCoordinationId = @LocalEmergencyCoordinationId AND TenantId = @TenantId AND IsDeleted = 0)
            THROW 50004, N'LocalEmergencyCoordination case not found or has been deleted.', 1;

        UPDATE [SafetyEvents].[LocalEmergencyCoordinationCases]
        SET
            Status = @Status,
            ClosedAtUtc = @ClosedAtUtc,
            ArchivedAtUtc = @ArchivedAtUtc
        WHERE LocalEmergencyCoordinationId = @LocalEmergencyCoordinationId AND TenantId = @TenantId AND IsDeleted = 0
          AND (@ETag IS NULL OR CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) = @ETag);

        DECLARE @RowsAffected INT = @@ROWCOUNT;

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'LocalEmergencyCoordinationCases', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'LocalEmergencyCoordinationId=', @LocalEmergencyCoordinationId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;

        SELECT @RowsAffected;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'LocalEmergencyCoordinationId=', @LocalEmergencyCoordinationId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO

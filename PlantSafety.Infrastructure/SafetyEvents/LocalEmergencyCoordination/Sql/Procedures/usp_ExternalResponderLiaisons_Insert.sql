-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyEvents.LocalEmergencyCoordination
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_ExternalResponderLiaisons_Insert.sql
--  Created:        2026-08-04
--  SchemaVersion:  2026.1
--  Description:    Idempotently inserts an ExternalResponderLiaison child entity row (INSERT
--                  WHERE NOT EXISTS -- mirrors usp_MutualAidAgreements_Insert.sql).
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [SafetyEvents].[usp_ExternalResponderLiaisons_Insert]
    @ExternalResponderLiaisonId UNIQUEIDENTIFIER,
    @LocalEmergencyCoordinationId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @IncidentId UNIQUEIDENTIFIER = NULL,
    @AgencyName NVARCHAR(200),
    @LiaisonUserId UNIQUEIDENTIFIER,
    @AssignedAtUtc DATETIME2(3),
    @AssignedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM [SafetyEvents].[ExternalResponderLiaisons] WHERE ExternalResponderLiaisonId = @ExternalResponderLiaisonId)
        BEGIN
            INSERT INTO [SafetyEvents].[ExternalResponderLiaisons]
            (ExternalResponderLiaisonId, LocalEmergencyCoordinationId, TenantId, IncidentId, AgencyName, LiaisonUserId, AssignedAtUtc, AssignedBy)
            VALUES
            (@ExternalResponderLiaisonId, @LocalEmergencyCoordinationId, @TenantId, @IncidentId, @AgencyName, @LiaisonUserId, @AssignedAtUtc, @AssignedBy);
        END
        -- Canonical audit trail (Handbook section 2.11): recorded after the write, in its own
        -- TRY/CATCH so an audit failure can never fail the operation it describes.
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'ExternalResponderLiaisons', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'ExternalResponderLiaisonId=', @ExternalResponderLiaisonId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ExternalResponderLiaisonId=', @ExternalResponderLiaisonId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO

-- ============================================================================
--  Module: CyberPhysicalSecurity | Layer: Infrastructure -- SQL Scripts
--  File: usp_ControlChangeEvent_Insert.sql | Created: 2026-08-04
--  Description: Appends a new, immutable ControlChangeEvent row. Tamper-evident: RecordHash and
--                PriorRecordHash are never modified after insert (see ControlChangeEvent.sql
--                header). Backs IControlChangeEventRepository.AddAsync.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [CyberPhysicalSecurity].[usp_ControlChangeEvent_Insert]
    @ControlChangeEventId UNIQUEIDENTIFIER,
    @TenantId             UNIQUEIDENTIFIER,
    @SiteId               UNIQUEIDENTIFIER,
    @OtAssetId            UNIQUEIDENTIFIER,
    @ChangeDescription    NVARCHAR(2000),
    @ChangedParameter     NVARCHAR(256),
    @OldValue             NVARCHAR(512),
    @NewValue             NVARCHAR(512),
    @ChangeSourceUserId   UNIQUEIDENTIFIER = NULL,
    @AuthorizationStatus  NVARCHAR(20),
    @DetectedAtUtc        DATETIME2(3),
    @PriorRecordHash      CHAR(64) = NULL,
    @RecordHash           CHAR(64),
    @CreatedAtUtc         DATETIME2(3),
    @CreatedBy            UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        INSERT INTO [CyberPhysicalSecurity].[ControlChangeEvent]
        (ControlChangeEventId, TenantId, SiteId, OtAssetId, ChangeDescription, ChangedParameter, OldValue, NewValue,
         ChangeSourceUserId, AuthorizationStatus, DetectedAtUtc, PriorRecordHash, RecordHash, CreatedAtUtc, CreatedBy)
        VALUES
        (@ControlChangeEventId, @TenantId, @SiteId, @OtAssetId, @ChangeDescription, @ChangedParameter, @OldValue, @NewValue,
         @ChangeSourceUserId, @AuthorizationStatus, @DetectedAtUtc, @PriorRecordHash, @RecordHash, @CreatedAtUtc, @CreatedBy);
        -- Canonical audit trail (Handbook section 2.11): recorded after the write, in its own
        -- TRY/CATCH so an audit failure can never fail the operation it describes.
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'ControlChangeEvent', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'ControlChangeEventId=', @ControlChangeEventId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ControlChangeEventId=', @ControlChangeEventId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
            -- Error logging must never mask the original error
        END CATCH;

        THROW;
    END CATCH
END
GO

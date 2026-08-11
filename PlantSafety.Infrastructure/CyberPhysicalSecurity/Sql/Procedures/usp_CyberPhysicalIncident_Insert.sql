-- ============================================================================
--  Module: CyberPhysicalSecurity | Layer: Infrastructure -- SQL Scripts
--  File: usp_CyberPhysicalIncident_Insert.sql | Created: 2026-08-04
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [CyberPhysicalSecurity].[usp_CyberPhysicalIncident_Insert]
    @CyberPhysicalIncidentId    UNIQUEIDENTIFIER,
    @TenantId                   UNIQUEIDENTIFIER,
    @SiteId                     UNIQUEIDENTIFIER,
    @Title                      NVARCHAR(256),
    @Description                NVARCHAR(4000),
    @Severity                   NVARCHAR(20),
    @Status                     NVARCHAR(20),
    @RelatedSabotageIndicatorIds NVARCHAR(4000) = NULL,
    @OwnerId                    UNIQUEIDENTIFIER,
    @OpenedAtUtc                DATETIME2(3),
    @ClosedAtUtc                DATETIME2(3) = NULL,
    @ResponseTimeMinutes        INT = NULL,
    @CreatedAtUtc                DATETIME2(3),
    @CreatedBy                   UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        INSERT INTO [CyberPhysicalSecurity].[CyberPhysicalIncident]
        (CyberPhysicalIncidentId, TenantId, SiteId, Title, Description, Severity, Status, RelatedSabotageIndicatorIds,
         OwnerId, OpenedAtUtc, ClosedAtUtc, ResponseTimeMinutes, CreatedAtUtc, CreatedBy)
        VALUES
        (@CyberPhysicalIncidentId, @TenantId, @SiteId, @Title, @Description, @Severity, @Status, @RelatedSabotageIndicatorIds,
         @OwnerId, @OpenedAtUtc, @ClosedAtUtc, @ResponseTimeMinutes, @CreatedAtUtc, @CreatedBy);
        -- Canonical audit trail (Handbook section 2.11): recorded after the write, in its own
        -- TRY/CATCH so an audit failure can never fail the operation it describes.
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'CyberPhysicalIncident', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'CyberPhysicalIncidentId=', @CyberPhysicalIncidentId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'CyberPhysicalIncidentId=', @CyberPhysicalIncidentId, N'; User=', SYSTEM_USER);
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

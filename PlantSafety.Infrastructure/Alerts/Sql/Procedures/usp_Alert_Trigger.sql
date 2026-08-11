-- ============================================================================
--  Module:  Alerts.Alert | Layer: Infrastructure (SQL) | Created: 2026-07-21
--  Description: Inserts a newly triggered Alert aggregate row.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER PROCEDURE [Alerts].[usp_Alert_Trigger]
    @AlertId                         UNIQUEIDENTIFIER,
    @TenantId                        UNIQUEIDENTIFIER,
    @SiteId                          UNIQUEIDENTIFIER = NULL,
    @CategoryCode                    NVARCHAR(50),
    @SeverityCode                    NVARCHAR(20),
    @SourceType                      NVARCHAR(30),
    @Title                           NVARCHAR(200),
    @LocationZoneName                NVARCHAR(200),
    @LocationLatitude                FLOAT = NULL,
    @LocationLongitude               FLOAT = NULL,
    @LocationIndoorPositionReference NVARCHAR(200) = NULL,
    @TriggeredAtUtc                  DATETIME2(3),
    @CreatedBy                       UNIQUEIDENTIFIER = NULL,
    @CorrelationId                   UNIQUEIDENTIFIER,
    @ActorId                         UNIQUEIDENTIFIER,
    @NowUtc                          DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        INSERT INTO [Alerts].[Alert]
            ([AlertId], [TenantId], [SiteId], [Status], [CategoryCode], [SeverityCode], [SourceType],
             [Title], [LocationZoneName], [LocationLatitude], [LocationLongitude], [LocationIndoorPositionReference],
             [TriggeredAtUtc], [CreatedAtUtc], [CreatedBy])
        VALUES
            (@AlertId, @TenantId, @SiteId, N'Triggered', @CategoryCode, @SeverityCode, @SourceType,
             @Title, @LocationZoneName, @LocationLatitude, @LocationLongitude, @LocationIndoorPositionReference,
             @TriggeredAtUtc, @NowUtc, @CreatedBy);
        -- Canonical audit trail (Handbook section 2.11): recorded after the write, in its own
        -- TRY/CATCH so an audit failure can never fail the operation it describes.
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'Alert', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'AlertId=', @AlertId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'AlertId=', @AlertId, N'; User=', SYSTEM_USER);
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

-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PhysicalAccessControl.AccessEvents
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_AccessEvents_Update.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Updates an existing AccessEvent aggregate row (processing status
--                  transitions). Best-effort audit-log side effect.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [PhysicalAccessControl].[usp_AccessEvents_Update]
    @NowUtc              DATETIME2(3),
    @AccessEventId       UNIQUEIDENTIFIER,
    @TenantId            UNIQUEIDENTIFIER,
    @SiteId              UNIQUEIDENTIFIER = NULL,
    @Status              NVARCHAR(100),
    @AccessPointId       UNIQUEIDENTIFIER,
    @CredentialId        UNIQUEIDENTIFIER = NULL,
    @SafetyZoneId        UNIQUEIDENTIFIER = NULL,
    @EventType           NVARCHAR(100),
    @Direction           NVARCHAR(20),
    @FlaggedAnomalyType  NVARCHAR(100) = NULL
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        UPDATE [PhysicalAccessControl].[AccessEvents]
        SET [Status]             = @Status,
            [AccessPointId]      = @AccessPointId,
            [CredentialId]       = @CredentialId,
            [SafetyZoneId]       = @SafetyZoneId,
            [EventType]          = @EventType,
            [Direction]          = @Direction,
            [FlaggedAnomalyType] = @FlaggedAnomalyType,
            [ModifiedAtUtc]      = @NowUtc
        WHERE [AccessEventId] = @AccessEventId
          AND [TenantId] = @TenantId;

        INSERT INTO dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime)
        VALUES ('PhysicalAccessControl.AccessEvents', DB_NAME(), 'UPDATE', NULL, SYSUTCDATETIME());
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'User=', SYSTEM_USER);
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

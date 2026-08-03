-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Auditing.AuditTimeline
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_ConcurrencyConflictRecord_GetById.sql
--  Created:        2026-08-02
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Retrieves a single ConcurrencyConflictRecord row by identity, tenant-scoped.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Auditing].[usp_ConcurrencyConflictRecord_GetById]
    @ConcurrencyConflictRecordId UNIQUEIDENTIFIER,
    @TenantId                    UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        SELECT ConcurrencyConflictRecordId, TenantId, EntityType, EntityId, ExpectedETag, ActualETag, AttemptedOperation, ActorId, OccurredAtUtc, CorrelationId
        FROM [Auditing].[ConcurrencyConflictRecord]
        WHERE ConcurrencyConflictRecordId = @ConcurrencyConflictRecordId AND TenantId = @TenantId;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ConcurrencyConflictRecordId=', @ConcurrencyConflictRecordId, N'; User=', SYSTEM_USER);
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

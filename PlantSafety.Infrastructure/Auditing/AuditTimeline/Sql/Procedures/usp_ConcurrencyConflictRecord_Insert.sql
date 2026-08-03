-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Auditing.AuditTimeline
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_ConcurrencyConflictRecord_Insert.sql
--  Created:        2026-08-02
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Inserts a newly captured ConcurrencyConflictRecord row. Backs
--                  IConcurrencyConflictRepository.AddAsync. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Auditing].[usp_ConcurrencyConflictRecord_Insert]
    @ConcurrencyConflictRecordId UNIQUEIDENTIFIER,
    @TenantId                    UNIQUEIDENTIFIER,
    @EntityType                  NVARCHAR(200),
    @EntityId                    UNIQUEIDENTIFIER,
    @ExpectedETag                NVARCHAR(64),
    @ActualETag                  NVARCHAR(64) = NULL,
    @AttemptedOperation          NVARCHAR(100),
    @ActorId                     UNIQUEIDENTIFIER = NULL,
    @OccurredAtUtc                DATETIME2(3),
    @CorrelationId                UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        INSERT INTO [Auditing].[ConcurrencyConflictRecord]
        (ConcurrencyConflictRecordId, TenantId, EntityType, EntityId, ExpectedETag, ActualETag, AttemptedOperation, ActorId, OccurredAtUtc, CorrelationId)
        VALUES
        (@ConcurrencyConflictRecordId, @TenantId, @EntityType, @EntityId, @ExpectedETag, @ActualETag, @AttemptedOperation, @ActorId, @OccurredAtUtc, @CorrelationId);

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'ConcurrencyConflictRecord', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'ConcurrencyConflictRecordId=', @ConcurrencyConflictRecordId), 1, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
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

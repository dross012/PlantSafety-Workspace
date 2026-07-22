-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         AccidentManagement
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_CorrectiveAction_Upsert.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Inserts a new CorrectiveAction row, or updates all mutable fields of an
--                  existing one (matched by CorrectiveActionId). Used by
--                  IAccidentRepository.UpdateAsync to reconcile the in-memory
--                  Accident.CorrectiveActions collection, since CAPA items are never removed
--                  once created -- only their lifecycle status/evidence changes.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [AccidentManagement].[usp_CorrectiveAction_Upsert]
    @CorrectiveActionId  UNIQUEIDENTIFIER,
    @AccidentId          UNIQUEIDENTIFIER,
    @TenantId            UNIQUEIDENTIFIER,
    @Category            NVARCHAR(30),
    @Description         NVARCHAR(2000),
    @ResponsiblePersonId UNIQUEIDENTIFIER,
    @DueAtUtc            DATETIME2(3),
    @Status              NVARCHAR(30),
    @VerificationSteps   NVARCHAR(2000) = NULL,
    @CompletionEvidence  NVARCHAR(2000) = NULL,
    @CompletedAtUtc      DATETIME2(3) = NULL,
    @VerifiedAtUtc       DATETIME2(3) = NULL,
    @VerifiedBy          UNIQUEIDENTIFIER = NULL,
    @CreatedAtUtc        DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        IF EXISTS (SELECT 1 FROM [AccidentManagement].[CorrectiveAction] WHERE CorrectiveActionId = @CorrectiveActionId)
        BEGIN
            UPDATE [AccidentManagement].[CorrectiveAction]
            SET
                Status = @Status,
                VerificationSteps = @VerificationSteps,
                CompletionEvidence = @CompletionEvidence,
                CompletedAtUtc = @CompletedAtUtc,
                VerifiedAtUtc = @VerifiedAtUtc,
                VerifiedBy = @VerifiedBy
            WHERE CorrectiveActionId = @CorrectiveActionId;
        END
        ELSE
        BEGIN
            INSERT INTO [AccidentManagement].[CorrectiveAction]
            (
                CorrectiveActionId, AccidentId, TenantId, Category, Description, ResponsiblePersonId, DueAtUtc,
                Status, VerificationSteps, CompletionEvidence, CompletedAtUtc, VerifiedAtUtc, VerifiedBy,
                CreatedAtUtc, IsDeleted
            )
            VALUES
            (
                @CorrectiveActionId, @AccidentId, @TenantId, @Category, @Description, @ResponsiblePersonId, @DueAtUtc,
                @Status, @VerificationSteps, @CompletionEvidence, @CompletedAtUtc, @VerifiedAtUtc, @VerifiedBy,
                @CreatedAtUtc, 0
            );
        END
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'CorrectiveActionId=', @CorrectiveActionId, N'; AccidentId=', @AccidentId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO

-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyPolicies
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_SafetyPolicy_Update.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Updates all mutable fields of a SafetyPolicy aggregate root row. Backs
--                  ISafetyPolicyRepository.UpdateAsync. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [SafetyPolicies].[usp_SafetyPolicy_Update]
    @SafetyPolicyId              UNIQUEIDENTIFIER,
    @Status                      NVARCHAR(30),
    @SafetyPolicyVersion         INT,
    @Category                    NVARCHAR(100),
    @ApprovedBy                  UNIQUEIDENTIFIER = NULL,
    @ApprovedAtUtc               DATETIME2(3) = NULL,
    @EffectiveAtUtc              DATETIME2(3) = NULL,
    @ReviewCadence               NVARCHAR(30),
    @LastReviewedAtUtc           DATETIME2(3) = NULL,
    @NextReviewDueUtc            DATETIME2(3) = NULL,
    @ReviewCount                 INT,
    @UpdatedBy                   UNIQUEIDENTIFIER = NULL
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM [SafetyPolicies].[SafetyPolicy] WHERE SafetyPolicyId = @SafetyPolicyId AND IsDeleted = 0)
            THROW 50004, N'SafetyPolicy not found or has been deleted.', 1;

        UPDATE [SafetyPolicies].[SafetyPolicy]
        SET
            Status = @Status,
            SafetyPolicyVersion = @SafetyPolicyVersion,
            Category = @Category,
            ApprovedBy = @ApprovedBy,
            ApprovedAtUtc = @ApprovedAtUtc,
            EffectiveAtUtc = @EffectiveAtUtc,
            ReviewCadence = @ReviewCadence,
            LastReviewedAtUtc = @LastReviewedAtUtc,
            NextReviewDueUtc = @NextReviewDueUtc,
            ReviewCount = @ReviewCount,
            UpdatedBy = @UpdatedBy,
            UpdatedAtUtc = SYSUTCDATETIME(),
            ConcurrencyToken = NEWID()
        WHERE SafetyPolicyId = @SafetyPolicyId AND IsDeleted = 0;

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'SafetyPolicy', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'SafetyPolicyId=', @SafetyPolicyId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'SafetyPolicyId=', @SafetyPolicyId, N'; User=', SYSTEM_USER);
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

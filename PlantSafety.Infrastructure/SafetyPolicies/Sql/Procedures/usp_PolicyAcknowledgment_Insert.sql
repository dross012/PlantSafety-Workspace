-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyPolicies
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_PolicyAcknowledgment_Insert.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Inserts a single PolicyAcknowledgment child-entity row. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [SafetyPolicies].[usp_PolicyAcknowledgment_Insert]
    @AcknowledgmentId          UNIQUEIDENTIFIER,
    @SafetyPolicyId            UNIQUEIDENTIFIER,
    @TenantId                  UNIQUEIDENTIFIER,
    @AcknowledgedPolicyVersion INT,
    @UserId                    UNIQUEIDENTIFIER,
    @AcknowledgedAtUtc         DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        INSERT INTO [SafetyPolicies].[PolicyAcknowledgment]
            (AcknowledgmentId, SafetyPolicyId, TenantId, AcknowledgedPolicyVersion, UserId, AcknowledgedAtUtc, IsDeleted)
        VALUES
            (@AcknowledgmentId, @SafetyPolicyId, @TenantId, @AcknowledgedPolicyVersion, @UserId, @AcknowledgedAtUtc, 0);
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'SafetyPolicyId=', @SafetyPolicyId, N'; AcknowledgmentId=', @AcknowledgmentId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO

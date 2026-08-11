-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyEvents.LocalEmergencyCoordination
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_JointResponsePlans_Insert.sql
--  Created:        2026-08-04
--  SchemaVersion:  2026.1
--  Description:    Idempotently inserts a JointResponsePlan child entity row, or synchronises its
--                  active/deactivated state if the row already exists (mirrors
--                  usp_MutualAidAgreements_Insert.sql's idempotent-upsert pattern).
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [SafetyEvents].[usp_JointResponsePlans_Insert]
    @JointResponsePlanId UNIQUEIDENTIFIER,
    @LocalEmergencyCoordinationId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @IncidentId UNIQUEIDENTIFIER = NULL,
    @PlanName NVARCHAR(200),
    @IsActive BIT,
    @ActivatedAtUtc DATETIME2(3),
    @ActivatedBy UNIQUEIDENTIFIER,
    @DeactivatedAtUtc DATETIME2(3) = NULL,
    @DeactivatedBy UNIQUEIDENTIFIER = NULL
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM [SafetyEvents].[JointResponsePlans] WHERE JointResponsePlanId = @JointResponsePlanId)
        BEGIN
            INSERT INTO [SafetyEvents].[JointResponsePlans]
            (JointResponsePlanId, LocalEmergencyCoordinationId, TenantId, IncidentId, PlanName, IsActive, ActivatedAtUtc, ActivatedBy, DeactivatedAtUtc, DeactivatedBy)
            VALUES
            (@JointResponsePlanId, @LocalEmergencyCoordinationId, @TenantId, @IncidentId, @PlanName, @IsActive, @ActivatedAtUtc, @ActivatedBy, @DeactivatedAtUtc, @DeactivatedBy);
        END
        ELSE
        BEGIN
            UPDATE [SafetyEvents].[JointResponsePlans]
            SET IsActive = @IsActive, DeactivatedAtUtc = @DeactivatedAtUtc, DeactivatedBy = @DeactivatedBy
            WHERE JointResponsePlanId = @JointResponsePlanId;
        END
        -- Canonical audit trail (Handbook section 2.11): recorded after the write, in its own
        -- TRY/CATCH so an audit failure can never fail the operation it describes.
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'JointResponsePlans', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'JointResponsePlanId=', @JointResponsePlanId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'JointResponsePlanId=', @JointResponsePlanId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO

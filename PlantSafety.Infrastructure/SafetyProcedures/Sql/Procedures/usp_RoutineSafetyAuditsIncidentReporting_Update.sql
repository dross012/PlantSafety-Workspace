-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyProcedures.RoutineSafetyAuditsIncidentReporting
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_RoutineSafetyAuditsIncidentReporting_Update.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Full-row update procedure for [SafetyProcedures].[RoutineSafetyAuditsIncidentReporting], backing
--                  IRoutineSafetyAuditsIncidentReportingRepository.UpdateAsync (all lifecycle transitions persist the
--                  aggregate's full current state). Standard error envelope: TRY/CATCH with
--                  transaction rollback and THROW. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyProcedures].[usp_RoutineSafetyAuditsIncidentReporting_Update]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
    @NowUtc        DATETIME2(3),
    @RoutineSafetyAuditsIncidentReportingId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(100),
    @AuditTitle NVARCHAR(400),
    @ReportingCategory NVARCHAR(100),
    @FacilityArea NVARCHAR(400),
    @ConductedByUserId UNIQUEIDENTIFIER,
    @ConductedAtUtc DATETIME2(3) = NULL,
    @NextAuditDueAtUtc DATETIME2(3) = NULL,
    @FindingCount INT,
    @ComplianceScore DECIMAL(9,2) = NULL,
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        UPDATE [SafetyProcedures].[RoutineSafetyAuditsIncidentReporting]
        SET
        [SiteId] = @SiteId,
        [Status] = @Status,
        [AuditTitle] = @AuditTitle,
        [ReportingCategory] = @ReportingCategory,
        [FacilityArea] = @FacilityArea,
        [ConductedByUserId] = @ConductedByUserId,
        [ConductedAtUtc] = @ConductedAtUtc,
        [NextAuditDueAtUtc] = @NextAuditDueAtUtc,
        [FindingCount] = @FindingCount,
        [ComplianceScore] = @ComplianceScore,
        [ModifiedAtUtc] = @NowUtc,
        [ModifiedBy] = @ActorId,
        [CorrelationId] = @CorrelationId
        WHERE [RoutineSafetyAuditsIncidentReportingId] = @RoutineSafetyAuditsIncidentReportingId
          AND [TenantId] = @TenantId
          AND ISNULL([IsDeleted], 0) = 0;
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

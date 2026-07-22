-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyProcedures.RoutineSafetyAuditsIncidentReporting
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_RoutineSafetyAuditsIncidentReporting_Create.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Insert procedure for [SafetyProcedures].[RoutineSafetyAuditsIncidentReporting], backing
--                  IRoutineSafetyAuditsIncidentReportingRepository.AddAsync. Standard error envelope: TRY/CATCH with
--                  transaction rollback and THROW. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyProcedures].[usp_RoutineSafetyAuditsIncidentReporting_Create]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
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
        INSERT INTO [SafetyProcedures].[RoutineSafetyAuditsIncidentReporting]
            ([RoutineSafetyAuditsIncidentReportingId], [TenantId], [SiteId], [Status], [AuditTitle], [ReportingCategory], [FacilityArea], [ConductedByUserId], [ConductedAtUtc], [NextAuditDueAtUtc], [FindingCount], [ComplianceScore], [CreatedAtUtc], [CreatedBy], [CorrelationId], [ActorId])
        VALUES
            (@RoutineSafetyAuditsIncidentReportingId, @TenantId, @SiteId, @Status, @AuditTitle, @ReportingCategory, @FacilityArea, @ConductedByUserId, @ConductedAtUtc, @NextAuditDueAtUtc, @FindingCount, @ComplianceScore, @CreatedAtUtc, @CreatedBy, @CorrelationId, @ActorId);
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

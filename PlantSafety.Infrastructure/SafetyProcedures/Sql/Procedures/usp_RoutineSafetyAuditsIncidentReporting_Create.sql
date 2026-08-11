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
        -- Canonical audit trail (Handbook §2.11): the write is recorded after it commits, in its
        -- own TRY/CATCH so an audit failure can never fail the operation it is describing.
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'RoutineSafetyAuditsIncidentReporting', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'RoutineSafetyAuditsIncidentReportingId=', @RoutineSafetyAuditsIncidentReportingId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'RoutineSafetyAuditsIncidentReportingId=', @RoutineSafetyAuditsIncidentReportingId, N'; User=', SYSTEM_USER);
    DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

    INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
    VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

    THROW;
END CATCH;
GO

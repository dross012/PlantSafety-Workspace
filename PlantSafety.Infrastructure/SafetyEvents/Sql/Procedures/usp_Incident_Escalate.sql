-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyEvents.IncidentManagement (Features-tree Incident aggregate)
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_Incident_Escalate.sql
--  Created:        2026-08-03
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Escalates an existing [SafetyEvents].[Incident] row (severity bump, escalation
--                  reason/count/timestamp). Backs IIncidentRepository.EscalateAsync /
--                  IncidentDapperRepository.EscalateAsync. Severity/Status/EscalationCount are
--                  pre-computed by the Domain aggregate (IncidentManagement.Escalate).
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyEvents].[usp_Incident_Escalate]
    @TenantId          UNIQUEIDENTIFIER,
    @CorrelationId     UNIQUEIDENTIFIER,
    @ActorId           UNIQUEIDENTIFIER,
    @NowUtc            DATETIME2(3),
    @IncidentId        UNIQUEIDENTIFIER,
    @Status            NVARCHAR(30),
    @Severity          NVARCHAR(30),
    @EscalationReason  NVARCHAR(1000),
    @EscalatedAtUtc    DATETIME2(3),
    @EscalationCount   INT
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        UPDATE [SafetyEvents].[Incident]
        SET
            Status = @Status,
            Severity = @Severity,
            EscalationReason = @EscalationReason,
            EscalatedAtUtc = @EscalatedAtUtc,
            EscalationCount = @EscalationCount,
            LastModifiedByActorId = @ActorId,
            LastModifiedAtUtc = @NowUtc
        WHERE IncidentId = @IncidentId
          AND TenantId = @TenantId
          AND IsDeleted = 0;

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'Incident', DB_NAME(), N'ESCALATE', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'IncidentId=', @IncidentId, N'; CorrelationId=', @CorrelationId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'IncidentId=', @IncidentId, N'; CorrelationId=', @CorrelationId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;

        THROW;
    END CATCH
END
GO

-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyEvents.IncidentManagement (Features-tree Incident aggregate)
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_IncidentRead_GetById.sql
--  Created:        2026-07-27
--  Author:         PlantSafety Platform Team (Claude Code procedure-architect-agent)
--  SchemaVersion:  2026.1
--  Description:    Read-only single-row lookup of [SafetyEvents].[Incident] by identity.
--                  Backs IIncidentReadRepository.GetByIdAsync / IncidentReadDapperRepository.
--                  GetByIdAsync. Column list and aliases mirror the IncidentReadModel
--                  contract (PlantSafety.Application.Features.SafetyEvents.Mappings.
--                  IncidentManagement.IncidentMappingProfile) exactly. ETag is projected from
--                  the native ROWVERSION the same way as ProcessManagement.usp_Process_GetById
--                  (CONVERT(NVARCHAR(34), RowVersion, 1)), matching the concurrency-token
--                  convention established by usp_Incident_Update/usp_Incident_SoftDelete.
--                  TRY/CATCH + THROW only (no AuditLog/ErrorLog writes), matching the
--                  established read-procedure convention in this module (see
--                  usp_CrisisEvent_List / usp_CrisisEvent_Count) -- read procedures do not
--                  write audit or error-log entries.
--                  UPDATED 2026-07-27 (Claude Code refactoring-agent): added CreatedAtUtc/CreatedBy
--                  to the result set (real, pre-existing columns on [SafetyEvents].[Incident]) so
--                  IncidentReadDapperRepository.GetByIdAsync's IncidentReadModel carries everything
--                  UpdateIncidentCommandHandler needs to call IncidentManagement.Rehydrate(...)
--                  without a second query -- part of wiring Create/Update through the
--                  IncidentManagement aggregate (see plantsafety-architecture.md Sec.5.1).
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyEvents].[usp_IncidentRead_GetById]
(
    @TenantId   UNIQUEIDENTIFIER,
    @IncidentId UNIQUEIDENTIFIER
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        SELECT
            IncidentId,
            TenantId,
            SiteId,
            Title,
            Description,
            IncidentOccurredAtUtc,
            ReportedAtUtc,
            Severity,
            Status,
            Location,
            ReportedByWorkerId,
            AssignedInvestigatorId,
            ClosedAtUtc,
            LastModifiedByActorId,
            LastModifiedAtUtc,
            CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag,
            CreatedAtUtc,
            CreatedBy,
            EscalationReason,
            EscalatedAtUtc,
            EscalationCount,
            ResolutionNotes
        FROM [SafetyEvents].[Incident]
        WHERE IncidentId = @IncidentId
          AND TenantId = @TenantId
          AND IsDeleted = 0;

        RETURN 0;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorNumber INT = ERROR_NUMBER();
        THROW;
        RETURN @ErrorNumber;
    END CATCH;
END;
GO

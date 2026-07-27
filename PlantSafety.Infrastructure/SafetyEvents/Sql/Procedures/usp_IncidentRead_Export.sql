-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyEvents.IncidentManagement (Features-tree Incident aggregate)
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_IncidentRead_Export.sql
--  Created:        2026-07-27
--  Author:         PlantSafety Platform Team (Claude Code procedure-architect-agent)
--  SchemaVersion:  2026.1
--  Description:    Bounded, deterministic bulk-export projection of [SafetyEvents].[Incident]
--                  rows. Backs IIncidentReadRepository.ExportAsync / IncidentReadDapperRepository.
--                  ExportAsync. Parameter names/types mirror the StoredProcedureBuilder Add*
--                  calls in IncidentReadDapperRepository.ExportAsync exactly (TenantId, SiteId,
--                  OccurredFromUtc, OccurredToUtc, MaxRecords). Column list mirrors
--                  IncidentReadModel. TOP (@MaxRecords) caps result-set size; deterministic
--                  ORDER BY (IncidentOccurredAtUtc DESC, IncidentId ASC tiebreaker), no
--                  OFFSET/FETCH paging since exports return a single bounded batch. TRY/CATCH +
--                  THROW only (no AuditLog/ErrorLog writes) per the established read-procedure
--                  convention. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyEvents].[usp_IncidentRead_Export]
(
    @TenantId        UNIQUEIDENTIFIER,
    @SiteId          UNIQUEIDENTIFIER = NULL,
    @OccurredFromUtc DATETIME2(3)     = NULL,
    @OccurredToUtc   DATETIME2(3)     = NULL,
    @MaxRecords      INT              = 10000
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        SELECT TOP (@MaxRecords)
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
            CONVERT(NVARCHAR(34), RowVersion, 1) AS ETag
        FROM [SafetyEvents].[Incident]
        WHERE TenantId = @TenantId
          AND IsDeleted = 0
          AND (@SiteId IS NULL OR SiteId = @SiteId)
          AND (@OccurredFromUtc IS NULL OR IncidentOccurredAtUtc >= @OccurredFromUtc)
          AND (@OccurredToUtc IS NULL OR IncidentOccurredAtUtc <= @OccurredToUtc)
        ORDER BY
            IncidentOccurredAtUtc DESC,
            IncidentId ASC;

        RETURN 0;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorNumber INT = ERROR_NUMBER();
        THROW;
        RETURN @ErrorNumber;
    END CATCH;
END;
GO

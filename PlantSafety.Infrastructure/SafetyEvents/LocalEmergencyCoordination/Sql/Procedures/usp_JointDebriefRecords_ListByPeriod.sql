-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyEvents.LocalEmergencyCoordination
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_JointDebriefRecords_ListByPeriod.sql
--  Created:        2026-08-04
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing
--                  ILocalEmergencyCoordinationRepository.ListJointDebriefsByPeriodAsync
--                  (Application read-model) -- the Post-Incident Debrief Summary dashboard.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyEvents].[usp_JointDebriefRecords_ListByPeriod]
    @TenantId UNIQUEIDENTIFIER,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT JointDebriefRecordId, LocalEmergencyCoordinationId, TenantId, IncidentId, AgenciesInvolved, Summary, RecordedAtUtc, RecordedBy
    FROM [SafetyEvents].[JointDebriefRecords]
    WHERE TenantId = @TenantId AND RecordedAtUtc >= @FromUtc AND RecordedAtUtc <= @ToUtc
    ORDER BY RecordedAtUtc DESC;
END
GO

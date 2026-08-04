-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyEvents.LocalEmergencyCoordination
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_MutualAidAgreements_List.sql
--  Created:        2026-08-04
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing
--                  ILocalEmergencyCoordinationRepository.ListMutualAidAgreementsAsync
--                  (Application read-model) -- the Mutual Aid Agreement Registry dashboard.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyEvents].[usp_MutualAidAgreements_List]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        a.MutualAidAgreementId, a.LocalEmergencyCoordinationId, a.TenantId, a.AgencyName, a.Discipline,
        a.DocumentVersion, a.EffectiveFromUtc, a.RenewalDueAtUtc, a.RenewalStatus, a.EstablishedAtUtc, a.EstablishedBy
    FROM [SafetyEvents].[MutualAidAgreements] a
    INNER JOIN [SafetyEvents].[LocalEmergencyCoordinationCases] c ON c.LocalEmergencyCoordinationId = a.LocalEmergencyCoordinationId
    WHERE a.TenantId = @TenantId
      AND (@SiteId IS NULL OR c.SiteId = @SiteId)
    ORDER BY a.RenewalDueAtUtc ASC;
END
GO

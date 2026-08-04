-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyEvents.LocalEmergencyCoordination
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_LocalEmergencyCoordinationCases_GetById.sql
--  Created:        2026-08-04
--  SchemaVersion:  2026.1
--  Description:    Retrieves a single LocalEmergencyCoordination case, tenant-scoped, plus all
--                  four child entity collections as additional result sets (for full aggregate
--                  rehydration via Dapper QueryMultipleAsync). Result set order: case row;
--                  MutualAidAgreements; JointResponsePlans; ExternalResponderLiaisons;
--                  LiaisonLogEntries; JointDebriefRecords. Projects RowVersion as a string ETag on
--                  the case row. Mirrors usp_EmergencyManagementItems_GetById.sql, extended for
--                  child collections.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [SafetyEvents].[usp_LocalEmergencyCoordinationCases_GetById]
    @LocalEmergencyCoordinationId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        SELECT
            LocalEmergencyCoordinationId, TenantId, SiteId, IncidentId, CaseName, Status, CreatedAtUtc, CreatedBy, ClosedAtUtc, ArchivedAtUtc,
            CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
        FROM [SafetyEvents].[LocalEmergencyCoordinationCases]
        WHERE LocalEmergencyCoordinationId = @LocalEmergencyCoordinationId AND TenantId = @TenantId AND IsDeleted = 0;

        SELECT MutualAidAgreementId, LocalEmergencyCoordinationId, TenantId, AgencyName, Discipline, DocumentVersion, EffectiveFromUtc, RenewalDueAtUtc, RenewalStatus, EstablishedAtUtc, EstablishedBy
        FROM [SafetyEvents].[MutualAidAgreements]
        WHERE LocalEmergencyCoordinationId = @LocalEmergencyCoordinationId;

        SELECT JointResponsePlanId, LocalEmergencyCoordinationId, TenantId, IncidentId, PlanName, IsActive, ActivatedAtUtc, ActivatedBy, DeactivatedAtUtc, DeactivatedBy
        FROM [SafetyEvents].[JointResponsePlans]
        WHERE LocalEmergencyCoordinationId = @LocalEmergencyCoordinationId;

        SELECT ExternalResponderLiaisonId, LocalEmergencyCoordinationId, TenantId, IncidentId, AgencyName, LiaisonUserId, AssignedAtUtc, AssignedBy
        FROM [SafetyEvents].[ExternalResponderLiaisons]
        WHERE LocalEmergencyCoordinationId = @LocalEmergencyCoordinationId;

        SELECT LiaisonLogEntryId, ExternalResponderLiaisonId, LocalEmergencyCoordinationId, TenantId, IncidentId, AgencyName, Message, LoggedAtUtc, LoggedBy
        FROM [SafetyEvents].[LiaisonLogEntries]
        WHERE LocalEmergencyCoordinationId = @LocalEmergencyCoordinationId;

        SELECT JointDebriefRecordId, LocalEmergencyCoordinationId, TenantId, IncidentId, AgenciesInvolved, Summary, RecordedAtUtc, RecordedBy
        FROM [SafetyEvents].[JointDebriefRecords]
        WHERE LocalEmergencyCoordinationId = @LocalEmergencyCoordinationId;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'LocalEmergencyCoordinationId=', @LocalEmergencyCoordinationId, N'; TenantId=', @TenantId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO

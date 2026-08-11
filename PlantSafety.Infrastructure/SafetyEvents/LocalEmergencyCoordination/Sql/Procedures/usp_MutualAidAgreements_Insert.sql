-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyEvents.LocalEmergencyCoordination
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_MutualAidAgreements_Insert.sql
--  Created:        2026-08-04
--  SchemaVersion:  2026.1
--  Description:    Idempotently inserts a MutualAidAgreement child entity row (INSERT WHERE NOT
--                  EXISTS -- UpdateAsync re-submits the aggregate's full in-memory child
--                  collections on every call, so this proc treats an existing row for
--                  @MutualAidAgreementId as a no-op rather than erroring).
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [SafetyEvents].[usp_MutualAidAgreements_Insert]
    @MutualAidAgreementId UNIQUEIDENTIFIER,
    @LocalEmergencyCoordinationId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @AgencyName NVARCHAR(200),
    @Discipline NVARCHAR(50),
    @DocumentVersion INT,
    @EffectiveFromUtc DATETIME2(3),
    @RenewalDueAtUtc DATETIME2(3),
    @RenewalStatus NVARCHAR(30),
    @EstablishedAtUtc DATETIME2(3),
    @EstablishedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM [SafetyEvents].[MutualAidAgreements] WHERE MutualAidAgreementId = @MutualAidAgreementId)
        BEGIN
            INSERT INTO [SafetyEvents].[MutualAidAgreements]
            (MutualAidAgreementId, LocalEmergencyCoordinationId, TenantId, AgencyName, Discipline, DocumentVersion, EffectiveFromUtc, RenewalDueAtUtc, RenewalStatus, EstablishedAtUtc, EstablishedBy)
            VALUES
            (@MutualAidAgreementId, @LocalEmergencyCoordinationId, @TenantId, @AgencyName, @Discipline, @DocumentVersion, @EffectiveFromUtc, @RenewalDueAtUtc, @RenewalStatus, @EstablishedAtUtc, @EstablishedBy);
        END
        ELSE
        BEGIN
            UPDATE [SafetyEvents].[MutualAidAgreements]
            SET RenewalStatus = @RenewalStatus
            WHERE MutualAidAgreementId = @MutualAidAgreementId;
        END
        -- Canonical audit trail (Handbook section 2.11): recorded after the write, in its own
        -- TRY/CATCH so an audit failure can never fail the operation it describes.
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'MutualAidAgreements', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'MutualAidAgreementId=', @MutualAidAgreementId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'MutualAidAgreementId=', @MutualAidAgreementId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO

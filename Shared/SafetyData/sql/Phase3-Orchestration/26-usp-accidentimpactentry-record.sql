-- =============================================
-- Shared.SafetyData -- Phase3-Orchestration
-- Script:        26-usp-accidentimpactentry-record.sql
-- Purpose:       Creates Orchestration.usp_AccidentImpactEntry_Record against
--                [PlantSafety.CoreDB] -- mirrors
--                IAccidentImpactStore.Record(Guid eventId, AccidentImpactEntry
--                entry) / InMemoryAccidentImpactStore.Record
--                (Reporting.Infrastructure\EventBus\ReadModels\
--                InMemoryAccidentImpactStore.cs). AccidentImpactEntry wraps
--                the real AccidentId alongside the same
--                FacilityEventOrchestrationResult columns persisted by
--                20-usp-facilityeventorchestrationresult-record.sql.
--
-- IDEMPOTENCY RULE (replicated exactly from InMemoryAccidentImpactStore.
-- Record): idempotent on EventId -- if a row for @EventId already exists,
-- no-op (never overwrites).
--
-- Idempotent (script itself): CREATE OR ALTER. Safe to re-run.
-- Additive-only evolution.
-- =============================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [Orchestration].[usp_AccidentImpactEntry_Record]
    @EventId                             uniqueidentifier,
    @AccidentId                          uniqueidentifier,
    @TenantId                            nvarchar(100),
    @FacilityId                          uniqueidentifier,
    @SourceModule                        nvarchar(200),
    @EventType                           nvarchar(200),
    @OccurredAtUtc                       datetime2(3),
    @ComplianceRequiredElementsCount     int,
    @ComplianceCompletedElementsCount    int,
    @ComplianceMissingElementsCount      int,
    @CompliancePercentComplete           float,
    @ComplianceTier                      nvarchar(30),
    @ComplianceNotes                     nvarchar(400),
    @RiskHazardLevel                     nvarchar(20),
    @RiskProgramLevel                    nvarchar(20),
    @RiskRegulatorLevel                  nvarchar(20),
    @RiskEventLevel                      nvarchar(20),
    @RiskOverallLevel                    nvarchar(20),
    @Category                            nvarchar(20),
    @RequiresImmediateAttention          bit,
    @Notification                        nvarchar(1000)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @EventId IS NULL
            THROW 50001, N'EventId is required.', 1;
        IF @AccidentId IS NULL
            THROW 50001, N'AccidentId is required.', 1;
        IF @TenantId IS NULL OR LTRIM(RTRIM(@TenantId)) = N''
            THROW 50001, N'TenantId is required.', 1;

        IF NOT EXISTS (
            SELECT 1
            FROM [Orchestration].[AccidentImpactEntry] WITH (UPDLOCK, HOLDLOCK)
            WHERE [EventId] = @EventId
        )
        BEGIN
            INSERT INTO [Orchestration].[AccidentImpactEntry] (
                [EventId], [AccidentId], [TenantId], [FacilityId], [SourceModule], [EventType], [OccurredAtUtc],
                [ComplianceRequiredElementsCount], [ComplianceCompletedElementsCount], [ComplianceMissingElementsCount],
                [CompliancePercentComplete], [ComplianceTier], [ComplianceNotes],
                [RiskHazardLevel], [RiskProgramLevel], [RiskRegulatorLevel], [RiskEventLevel], [RiskOverallLevel],
                [Category], [RequiresImmediateAttention], [Notification], [CreatedUtc]
            )
            VALUES (
                @EventId, @AccidentId, @TenantId, @FacilityId, @SourceModule, @EventType, @OccurredAtUtc,
                @ComplianceRequiredElementsCount, @ComplianceCompletedElementsCount, @ComplianceMissingElementsCount,
                @CompliancePercentComplete, @ComplianceTier, @ComplianceNotes,
                @RiskHazardLevel, @RiskProgramLevel, @RiskRegulatorLevel, @RiskEventLevel, @RiskOverallLevel,
                @Category, @RequiresImmediateAttention, @Notification, SYSUTCDATETIME()
            );

            BEGIN TRY
                INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                    TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                    AfterSnapshot, ErrorContext, Severity, Module, ViewerAccessed
                )
                VALUES (
                    N'Orchestration.AccidentImpactEntry', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                    CONCAT(N'EventId=', @EventId, N'; AccidentId=', @AccidentId, N'; TenantId=', @TenantId),
                    NULL, N'INFO', OBJECT_NAME(@@PROCID), 0
                );
            END TRY
            BEGIN CATCH
            END CATCH;
        END
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContext26 nvarchar(200) = CONCAT(N'Params=EventId=', @EventId, N'; AccidentId=', @AccidentId, N'; User=', SYSTEM_USER);
        DECLARE @ErrorDetails26 nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
            N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
            N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());

        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
                ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
                ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
            )
            VALUES (
                ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                SYSUTCDATETIME(), @ErrorContext26, @ErrorDetails26, DB_NAME(), OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
        END CATCH;

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                BeforeSnapshot, AfterSnapshot, ErrorContext, Severity, Module, ViewerAccessed
            )
            VALUES (
                N'Orchestration.AccidentImpactEntry', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                NULL, NULL, @ErrorContext26, ERROR_SEVERITY(), OBJECT_NAME(@@PROCID), 0
            );
        END TRY
        BEGIN CATCH
        END CATCH;

        THROW;
    END CATCH
END
GO

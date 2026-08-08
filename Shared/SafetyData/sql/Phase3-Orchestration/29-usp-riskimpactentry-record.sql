-- =============================================
-- Shared.SafetyData -- Phase3-Orchestration
-- Script:        29-usp-riskimpactentry-record.sql
-- Purpose:       Creates Orchestration.usp_RiskImpactEntry_Record against
--                [PlantSafety.CoreDB] -- mirrors IRiskImpactStore.Record(Guid
--                eventId, RiskImpactEntry entry) / InMemoryRiskImpactStore.
--                Record (Reporting.Infrastructure\EventBus\ReadModels\
--                InMemoryRiskImpactStore.cs), which mirrors
--                InMemoryAccidentImpactStore's shape exactly per that file's
--                own header. RiskImpactEntry wraps the real RiskId alongside
--                the same FacilityEventOrchestrationResult columns.
--
-- IDEMPOTENCY RULE: idempotent on EventId -- if a row for @EventId already
-- exists, no-op (never overwrites), matching
-- InMemoryAccidentImpactStore.Record's rule (RiskImpactEntry's store
-- mirrors it exactly).
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

CREATE OR ALTER PROCEDURE [Orchestration].[usp_RiskImpactEntry_Record]
    @EventId                             uniqueidentifier,
    @RiskId                              uniqueidentifier,
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
        IF @RiskId IS NULL
            THROW 50001, N'RiskId is required.', 1;
        IF @TenantId IS NULL OR LTRIM(RTRIM(@TenantId)) = N''
            THROW 50001, N'TenantId is required.', 1;

        IF NOT EXISTS (
            SELECT 1
            FROM [Orchestration].[RiskImpactEntry] WITH (UPDLOCK, HOLDLOCK)
            WHERE [EventId] = @EventId
        )
        BEGIN
            INSERT INTO [Orchestration].[RiskImpactEntry] (
                [EventId], [RiskId], [TenantId], [FacilityId], [SourceModule], [EventType], [OccurredAtUtc],
                [ComplianceRequiredElementsCount], [ComplianceCompletedElementsCount], [ComplianceMissingElementsCount],
                [CompliancePercentComplete], [ComplianceTier], [ComplianceNotes],
                [RiskHazardLevel], [RiskProgramLevel], [RiskRegulatorLevel], [RiskEventLevel], [RiskOverallLevel],
                [Category], [RequiresImmediateAttention], [Notification], [CreatedUtc]
            )
            VALUES (
                @EventId, @RiskId, @TenantId, @FacilityId, @SourceModule, @EventType, @OccurredAtUtc,
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
                    N'Orchestration.RiskImpactEntry', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                    CONCAT(N'EventId=', @EventId, N'; RiskId=', @RiskId, N'; TenantId=', @TenantId),
                    NULL, N'INFO', OBJECT_NAME(@@PROCID), 0
                );
            END TRY
            BEGIN CATCH
            END CATCH;
        END
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContext29 nvarchar(200) = CONCAT(N'Params=EventId=', @EventId, N'; RiskId=', @RiskId, N'; User=', SYSTEM_USER);
        DECLARE @ErrorDetails29 nvarchar(max) = CONCAT(
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
                SYSUTCDATETIME(), @ErrorContext29, @ErrorDetails29, DB_NAME(), OBJECT_NAME(@@PROCID)
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
                N'Orchestration.RiskImpactEntry', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                NULL, NULL, @ErrorContext29, ERROR_SEVERITY(), OBJECT_NAME(@@PROCID), 0
            );
        END TRY
        BEGIN CATCH
        END CATCH;

        THROW;
    END CATCH
END
GO

-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyCulture (metric read-models)
--  Layer:          Infrastructure - SQL Scripts
--  File:           CultureMetricRecord.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Shared physical read-model table backing the 8 metric-scoped read
--                  repositories under CultureAssessment/<Metric>/Repositories
--                  (BehavioralObservationCompletion, LeadershipEngagementFrequency,
--                  NearMissReportingRate, PpeComplianceRate, SafetyMeetingParticipation,
--                  SafetySuggestionSubmissions, StopWorkActions, TrainingCompletionRate).
--                  Each metric's read repository queries this one table filtered by its own
--                  [MetricType] discriminator value via a dedicated usp_<Metric>_GetItems
--                  stored procedure -- see CultureMetricType smart enum for the canonical
--                  metric code list. Distinct from [Analytics] schema tables
--                  (Analytics.Culture module). Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CultureMetricRecord' AND schema_id = SCHEMA_ID('SafetyCulture'))
BEGIN
    CREATE TABLE [SafetyCulture].[CultureMetricRecord] (
        [ItemId]              UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [CultureAssessmentId] UNIQUEIDENTIFIER NOT NULL,
        [TenantId]            UNIQUEIDENTIFIER NOT NULL,
        [SiteId]              UNIQUEIDENTIFIER NULL,
        [MetricType]          NVARCHAR(100)    NOT NULL,
        [Title]               NVARCHAR(200)    NOT NULL,
        [Status]              NVARCHAR(20)     NOT NULL,
        [RecordedAtUtc]       DATETIME2(3)     NOT NULL,
        [IsDeleted]           BIT              NOT NULL CONSTRAINT DF_CultureMetricRecord_IsDeleted DEFAULT (0),
        CONSTRAINT FK_CultureMetricRecord_CultureAssessment
            FOREIGN KEY ([CultureAssessmentId]) REFERENCES [SafetyCulture].[CultureAssessment] ([CultureAssessmentId])
    );

    CREATE NONCLUSTERED INDEX [IX_CultureMetricRecord_MetricType_TenantId_RecordedAtUtc]
        ON [SafetyCulture].[CultureMetricRecord] ([MetricType], [TenantId], [RecordedAtUtc])
        INCLUDE ([SiteId], [Title], [Status], [CultureAssessmentId]);
END
GO

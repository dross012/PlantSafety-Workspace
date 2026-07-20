-- =============================================
-- Migration:     140-extended-properties-coredb-aggregates.sql
-- Author:        Documentation Agent
-- Create date:   2026-07-19
-- Description:   Adds MS_Description extended properties to the 22
--                CoreDB aggregate tables enriched in commit ebe6db4e
--                (Flesh out 22 stub Domain aggregates with real
--                business properties) and their new business columns.
--                Table-level and column-level descriptions are sourced
--                directly from the XML doc <summary> on the corresponding
--                PlantSafety.Domain aggregate class/property (no invented
--                business meaning -- see project_intelligence_design No-
--                Guessing Rule). Generic infra columns (Id/TenantId/
--                SiteId/CreatedUtc/CreatedBy/UpdatedUtc/UpdatedBy/
--                IsDeleted/ConcurrencyToken/Status) are intentionally
--                skipped -- restating the StrongId/audit-column
--                convention on every table would be noise, not
--                documentation.
--
-- Idempotency:   sp_updateextendedproperty when the property already
--                exists, sp_addextendedproperty otherwise. Documentation-
--                only; zero rows of application data touched.
-- =============================================
SET NOCOUNT ON;
GO
USE [PlantSafety.CoreDB];
GO

------------------------------------------------------------
-- Analytics.Culture
------------------------------------------------------------
IF EXISTS (SELECT 1 FROM sys.extended_properties WHERE major_id = OBJECT_ID(N'Analytics.Culture') AND minor_id = 0 AND name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Tracks a single safety-culture survey/assessment record for a tenant/site: survey instrument, participation, and computed culture-maturity score.', @level0type=N'SCHEMA', @level0name=N'Analytics', @level1type=N'TABLE', @level1name=N'Culture';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Tracks a single safety-culture survey/assessment record for a tenant/site: survey instrument, participation, and computed culture-maturity score.', @level0type=N'SCHEMA', @level0name=N'Analytics', @level1type=N'TABLE', @level1name=N'Culture';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Analytics.Culture') AND c.name = N'SurveyPeriod' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Survey period this measurement covers (e.g., "2026-Q3").', @level0type=N'SCHEMA', @level0name=N'Analytics', @level1type=N'TABLE', @level1name=N'Culture', @level2type=N'COLUMN', @level2name=N'SurveyPeriod';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Survey period this measurement covers (e.g., "2026-Q3").', @level0type=N'SCHEMA', @level0name=N'Analytics', @level1type=N'TABLE', @level1name=N'Culture', @level2type=N'COLUMN', @level2name=N'SurveyPeriod';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Analytics.Culture') AND c.name = N'CultureIndexScore' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Computed safety culture index score (0-100).', @level0type=N'SCHEMA', @level0name=N'Analytics', @level1type=N'TABLE', @level1name=N'Culture', @level2type=N'COLUMN', @level2name=N'CultureIndexScore';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Computed safety culture index score (0-100).', @level0type=N'SCHEMA', @level0name=N'Analytics', @level1type=N'TABLE', @level1name=N'Culture', @level2type=N'COLUMN', @level2name=N'CultureIndexScore';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Analytics.Culture') AND c.name = N'EngagementScore' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Computed engagement score (0-100).', @level0type=N'SCHEMA', @level0name=N'Analytics', @level1type=N'TABLE', @level1name=N'Culture', @level2type=N'COLUMN', @level2name=N'EngagementScore';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Computed engagement score (0-100).', @level0type=N'SCHEMA', @level0name=N'Analytics', @level1type=N'TABLE', @level1name=N'Culture', @level2type=N'COLUMN', @level2name=N'EngagementScore';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Analytics.Culture') AND c.name = N'ParticipationRate' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Survey participation rate (0-100).', @level0type=N'SCHEMA', @level0name=N'Analytics', @level1type=N'TABLE', @level1name=N'Culture', @level2type=N'COLUMN', @level2name=N'ParticipationRate';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Survey participation rate (0-100).', @level0type=N'SCHEMA', @level0name=N'Analytics', @level1type=N'TABLE', @level1name=N'Culture', @level2type=N'COLUMN', @level2name=N'ParticipationRate';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Analytics.Culture') AND c.name = N'TopRiskTheme' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Top risk theme identified from this survey period.', @level0type=N'SCHEMA', @level0name=N'Analytics', @level1type=N'TABLE', @level1name=N'Culture', @level2type=N'COLUMN', @level2name=N'TopRiskTheme';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Top risk theme identified from this survey period.', @level0type=N'SCHEMA', @level0name=N'Analytics', @level1type=N'TABLE', @level1name=N'Culture', @level2type=N'COLUMN', @level2name=N'TopRiskTheme';
GO

------------------------------------------------------------
-- Analytics.Dashboard
------------------------------------------------------------
IF EXISTS (SELECT 1 FROM sys.extended_properties WHERE major_id = OBJECT_ID(N'Analytics.Dashboard') AND minor_id = 0 AND name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Defines a configured analytics dashboard instance (layout/widget set) available to users within a tenant.', @level0type=N'SCHEMA', @level0name=N'Analytics', @level1type=N'TABLE', @level1name=N'Dashboard';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Defines a configured analytics dashboard instance (layout/widget set) available to users within a tenant.', @level0type=N'SCHEMA', @level0name=N'Analytics', @level1type=N'TABLE', @level1name=N'Dashboard';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Analytics.Dashboard') AND c.name = N'DashboardName' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Human-readable dashboard name.', @level0type=N'SCHEMA', @level0name=N'Analytics', @level1type=N'TABLE', @level1name=N'Dashboard', @level2type=N'COLUMN', @level2name=N'DashboardName';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Human-readable dashboard name.', @level0type=N'SCHEMA', @level0name=N'Analytics', @level1type=N'TABLE', @level1name=N'Dashboard', @level2type=N'COLUMN', @level2name=N'DashboardName';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Analytics.Dashboard') AND c.name = N'DashboardType' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Dashboard classification (e.g., Executive, Operator, SiteSafety, Compliance).', @level0type=N'SCHEMA', @level0name=N'Analytics', @level1type=N'TABLE', @level1name=N'Dashboard', @level2type=N'COLUMN', @level2name=N'DashboardType';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Dashboard classification (e.g., Executive, Operator, SiteSafety, Compliance).', @level0type=N'SCHEMA', @level0name=N'Analytics', @level1type=N'TABLE', @level1name=N'Dashboard', @level2type=N'COLUMN', @level2name=N'DashboardType';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Analytics.Dashboard') AND c.name = N'OwnerId' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Identity of the accountable dashboard owner.', @level0type=N'SCHEMA', @level0name=N'Analytics', @level1type=N'TABLE', @level1name=N'Dashboard', @level2type=N'COLUMN', @level2name=N'OwnerId';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Identity of the accountable dashboard owner.', @level0type=N'SCHEMA', @level0name=N'Analytics', @level1type=N'TABLE', @level1name=N'Dashboard', @level2type=N'COLUMN', @level2name=N'OwnerId';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Analytics.Dashboard') AND c.name = N'RefreshIntervalSeconds' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Refresh interval, in seconds, for live data widgets.', @level0type=N'SCHEMA', @level0name=N'Analytics', @level1type=N'TABLE', @level1name=N'Dashboard', @level2type=N'COLUMN', @level2name=N'RefreshIntervalSeconds';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Refresh interval, in seconds, for live data widgets.', @level0type=N'SCHEMA', @level0name=N'Analytics', @level1type=N'TABLE', @level1name=N'Dashboard', @level2type=N'COLUMN', @level2name=N'RefreshIntervalSeconds';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Analytics.Dashboard') AND c.name = N'WidgetCount' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Count of widgets configured on this dashboard.', @level0type=N'SCHEMA', @level0name=N'Analytics', @level1type=N'TABLE', @level1name=N'Dashboard', @level2type=N'COLUMN', @level2name=N'WidgetCount';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Count of widgets configured on this dashboard.', @level0type=N'SCHEMA', @level0name=N'Analytics', @level1type=N'TABLE', @level1name=N'Dashboard', @level2type=N'COLUMN', @level2name=N'WidgetCount';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Analytics.Dashboard') AND c.name = N'IsPublished' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Indicates whether this dashboard is published for general use.', @level0type=N'SCHEMA', @level0name=N'Analytics', @level1type=N'TABLE', @level1name=N'Dashboard', @level2type=N'COLUMN', @level2name=N'IsPublished';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Indicates whether this dashboard is published for general use.', @level0type=N'SCHEMA', @level0name=N'Analytics', @level1type=N'TABLE', @level1name=N'Dashboard', @level2type=N'COLUMN', @level2name=N'IsPublished';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Analytics.Dashboard') AND c.name = N'LastRefreshedAtUtc' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'UTC timestamp this dashboard''s data was last refreshed.', @level0type=N'SCHEMA', @level0name=N'Analytics', @level1type=N'TABLE', @level1name=N'Dashboard', @level2type=N'COLUMN', @level2name=N'LastRefreshedAtUtc';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'UTC timestamp this dashboard''s data was last refreshed.', @level0type=N'SCHEMA', @level0name=N'Analytics', @level1type=N'TABLE', @level1name=N'Dashboard', @level2type=N'COLUMN', @level2name=N'LastRefreshedAtUtc';
GO

------------------------------------------------------------
-- Analytics.Engagement
------------------------------------------------------------
IF EXISTS (SELECT 1 FROM sys.extended_properties WHERE major_id = OBJECT_ID(N'Analytics.Engagement') AND minor_id = 0 AND name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Tracks a worker safety-engagement initiative or campaign record and its participation/outcome metrics.', @level0type=N'SCHEMA', @level0name=N'Analytics', @level1type=N'TABLE', @level1name=N'Engagement';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Tracks a worker safety-engagement initiative or campaign record and its participation/outcome metrics.', @level0type=N'SCHEMA', @level0name=N'Analytics', @level1type=N'TABLE', @level1name=N'Engagement';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Analytics.Engagement') AND c.name = N'WorkerId' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Worker this engagement measurement relates to.', @level0type=N'SCHEMA', @level0name=N'Analytics', @level1type=N'TABLE', @level1name=N'Engagement', @level2type=N'COLUMN', @level2name=N'WorkerId';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Worker this engagement measurement relates to.', @level0type=N'SCHEMA', @level0name=N'Analytics', @level1type=N'TABLE', @level1name=N'Engagement', @level2type=N'COLUMN', @level2name=N'WorkerId';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Analytics.Engagement') AND c.name = N'EngagementScore' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Computed engagement score (0-100).', @level0type=N'SCHEMA', @level0name=N'Analytics', @level1type=N'TABLE', @level1name=N'Engagement', @level2type=N'COLUMN', @level2name=N'EngagementScore';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Computed engagement score (0-100).', @level0type=N'SCHEMA', @level0name=N'Analytics', @level1type=N'TABLE', @level1name=N'Engagement', @level2type=N'COLUMN', @level2name=N'EngagementScore';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Analytics.Engagement') AND c.name = N'SurveyPeriod' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Survey period this measurement covers (e.g., "2026-Q3").', @level0type=N'SCHEMA', @level0name=N'Analytics', @level1type=N'TABLE', @level1name=N'Engagement', @level2type=N'COLUMN', @level2name=N'SurveyPeriod';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Survey period this measurement covers (e.g., "2026-Q3").', @level0type=N'SCHEMA', @level0name=N'Analytics', @level1type=N'TABLE', @level1name=N'Engagement', @level2type=N'COLUMN', @level2name=N'SurveyPeriod';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Analytics.Engagement') AND c.name = N'ParticipationChannel' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Channel through which engagement was captured (e.g., MobileApp, Kiosk, Paper, Interview).', @level0type=N'SCHEMA', @level0name=N'Analytics', @level1type=N'TABLE', @level1name=N'Engagement', @level2type=N'COLUMN', @level2name=N'ParticipationChannel';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Channel through which engagement was captured (e.g., MobileApp, Kiosk, Paper, Interview).', @level0type=N'SCHEMA', @level0name=N'Analytics', @level1type=N'TABLE', @level1name=N'Engagement', @level2type=N'COLUMN', @level2name=N'ParticipationChannel';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Analytics.Engagement') AND c.name = N'ResponseCount' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Number of responses/interactions captured for this measurement.', @level0type=N'SCHEMA', @level0name=N'Analytics', @level1type=N'TABLE', @level1name=N'Engagement', @level2type=N'COLUMN', @level2name=N'ResponseCount';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Number of responses/interactions captured for this measurement.', @level0type=N'SCHEMA', @level0name=N'Analytics', @level1type=N'TABLE', @level1name=N'Engagement', @level2type=N'COLUMN', @level2name=N'ResponseCount';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Analytics.Engagement') AND c.name = N'LastEngagedAtUtc' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'UTC timestamp this worker last engaged with a safety touchpoint.', @level0type=N'SCHEMA', @level0name=N'Analytics', @level1type=N'TABLE', @level1name=N'Engagement', @level2type=N'COLUMN', @level2name=N'LastEngagedAtUtc';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'UTC timestamp this worker last engaged with a safety touchpoint.', @level0type=N'SCHEMA', @level0name=N'Analytics', @level1type=N'TABLE', @level1name=N'Engagement', @level2type=N'COLUMN', @level2name=N'LastEngagedAtUtc';
GO

------------------------------------------------------------
-- Analytics.PredictiveAnalytics
------------------------------------------------------------
IF EXISTS (SELECT 1 FROM sys.extended_properties WHERE major_id = OBJECT_ID(N'Analytics.PredictiveAnalytics') AND minor_id = 0 AND name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Stores a single predictive-analytics model run/prediction record (risk forecast) produced for a tenant/site.', @level0type=N'SCHEMA', @level0name=N'Analytics', @level1type=N'TABLE', @level1name=N'PredictiveAnalytics';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Stores a single predictive-analytics model run/prediction record (risk forecast) produced for a tenant/site.', @level0type=N'SCHEMA', @level0name=N'Analytics', @level1type=N'TABLE', @level1name=N'PredictiveAnalytics';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Analytics.PredictiveAnalytics') AND c.name = N'ModelName' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Name of the predictive model that generated this output.', @level0type=N'SCHEMA', @level0name=N'Analytics', @level1type=N'TABLE', @level1name=N'PredictiveAnalytics', @level2type=N'COLUMN', @level2name=N'ModelName';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Name of the predictive model that generated this output.', @level0type=N'SCHEMA', @level0name=N'Analytics', @level1type=N'TABLE', @level1name=N'PredictiveAnalytics', @level2type=N'COLUMN', @level2name=N'ModelName';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Analytics.PredictiveAnalytics') AND c.name = N'PredictionType' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Prediction classification (e.g., IncidentRisk, EquipmentFailure, ComplianceDrift).', @level0type=N'SCHEMA', @level0name=N'Analytics', @level1type=N'TABLE', @level1name=N'PredictiveAnalytics', @level2type=N'COLUMN', @level2name=N'PredictionType';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Prediction classification (e.g., IncidentRisk, EquipmentFailure, ComplianceDrift).', @level0type=N'SCHEMA', @level0name=N'Analytics', @level1type=N'TABLE', @level1name=N'PredictiveAnalytics', @level2type=N'COLUMN', @level2name=N'PredictionType';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Analytics.PredictiveAnalytics') AND c.name = N'TargetEntityId' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Entity this prediction targets (worker, equipment, site), when applicable.', @level0type=N'SCHEMA', @level0name=N'Analytics', @level1type=N'TABLE', @level1name=N'PredictiveAnalytics', @level2type=N'COLUMN', @level2name=N'TargetEntityId';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Entity this prediction targets (worker, equipment, site), when applicable.', @level0type=N'SCHEMA', @level0name=N'Analytics', @level1type=N'TABLE', @level1name=N'PredictiveAnalytics', @level2type=N'COLUMN', @level2name=N'TargetEntityId';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Analytics.PredictiveAnalytics') AND c.name = N'PredictedRiskScore' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Predicted risk score (0-100).', @level0type=N'SCHEMA', @level0name=N'Analytics', @level1type=N'TABLE', @level1name=N'PredictiveAnalytics', @level2type=N'COLUMN', @level2name=N'PredictedRiskScore';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Predicted risk score (0-100).', @level0type=N'SCHEMA', @level0name=N'Analytics', @level1type=N'TABLE', @level1name=N'PredictiveAnalytics', @level2type=N'COLUMN', @level2name=N'PredictedRiskScore';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Analytics.PredictiveAnalytics') AND c.name = N'ConfidenceLevel' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Model confidence level for this prediction (0-100).', @level0type=N'SCHEMA', @level0name=N'Analytics', @level1type=N'TABLE', @level1name=N'PredictiveAnalytics', @level2type=N'COLUMN', @level2name=N'ConfidenceLevel';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Model confidence level for this prediction (0-100).', @level0type=N'SCHEMA', @level0name=N'Analytics', @level1type=N'TABLE', @level1name=N'PredictiveAnalytics', @level2type=N'COLUMN', @level2name=N'ConfidenceLevel';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Analytics.PredictiveAnalytics') AND c.name = N'PredictionHorizonDays' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Prediction horizon, in days, this output forecasts over.', @level0type=N'SCHEMA', @level0name=N'Analytics', @level1type=N'TABLE', @level1name=N'PredictiveAnalytics', @level2type=N'COLUMN', @level2name=N'PredictionHorizonDays';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Prediction horizon, in days, this output forecasts over.', @level0type=N'SCHEMA', @level0name=N'Analytics', @level1type=N'TABLE', @level1name=N'PredictiveAnalytics', @level2type=N'COLUMN', @level2name=N'PredictionHorizonDays';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Analytics.PredictiveAnalytics') AND c.name = N'GeneratedAtUtc' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'UTC timestamp this prediction was generated.', @level0type=N'SCHEMA', @level0name=N'Analytics', @level1type=N'TABLE', @level1name=N'PredictiveAnalytics', @level2type=N'COLUMN', @level2name=N'GeneratedAtUtc';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'UTC timestamp this prediction was generated.', @level0type=N'SCHEMA', @level0name=N'Analytics', @level1type=N'TABLE', @level1name=N'PredictiveAnalytics', @level2type=N'COLUMN', @level2name=N'GeneratedAtUtc';
GO

------------------------------------------------------------
-- Analytics.ScoreCards
------------------------------------------------------------
IF EXISTS (SELECT 1 FROM sys.extended_properties WHERE major_id = OBJECT_ID(N'Analytics.ScoreCards') AND minor_id = 0 AND name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Stores a computed safety scorecard: a period-scoped rollup of KPI results for a tenant/site.', @level0type=N'SCHEMA', @level0name=N'Analytics', @level1type=N'TABLE', @level1name=N'ScoreCards';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Stores a computed safety scorecard: a period-scoped rollup of KPI results for a tenant/site.', @level0type=N'SCHEMA', @level0name=N'Analytics', @level1type=N'TABLE', @level1name=N'ScoreCards';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Analytics.ScoreCards') AND c.name = N'ScoreCardName' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Human-readable scorecard name.', @level0type=N'SCHEMA', @level0name=N'Analytics', @level1type=N'TABLE', @level1name=N'ScoreCards', @level2type=N'COLUMN', @level2name=N'ScoreCardName';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Human-readable scorecard name.', @level0type=N'SCHEMA', @level0name=N'Analytics', @level1type=N'TABLE', @level1name=N'ScoreCards', @level2type=N'COLUMN', @level2name=N'ScoreCardName';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Analytics.ScoreCards') AND c.name = N'ReportingPeriod' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Reporting period this scorecard covers (e.g., "2026-Q3").', @level0type=N'SCHEMA', @level0name=N'Analytics', @level1type=N'TABLE', @level1name=N'ScoreCards', @level2type=N'COLUMN', @level2name=N'ReportingPeriod';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Reporting period this scorecard covers (e.g., "2026-Q3").', @level0type=N'SCHEMA', @level0name=N'Analytics', @level1type=N'TABLE', @level1name=N'ScoreCards', @level2type=N'COLUMN', @level2name=N'ReportingPeriod';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Analytics.ScoreCards') AND c.name = N'KpiCategory' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'KPI category this scorecard reports on (e.g., Safety, Compliance, Operations).', @level0type=N'SCHEMA', @level0name=N'Analytics', @level1type=N'TABLE', @level1name=N'ScoreCards', @level2type=N'COLUMN', @level2name=N'KpiCategory';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'KPI category this scorecard reports on (e.g., Safety, Compliance, Operations).', @level0type=N'SCHEMA', @level0name=N'Analytics', @level1type=N'TABLE', @level1name=N'ScoreCards', @level2type=N'COLUMN', @level2name=N'KpiCategory';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Analytics.ScoreCards') AND c.name = N'PerformanceIndex' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Computed performance index (0-100).', @level0type=N'SCHEMA', @level0name=N'Analytics', @level1type=N'TABLE', @level1name=N'ScoreCards', @level2type=N'COLUMN', @level2name=N'PerformanceIndex';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Computed performance index (0-100).', @level0type=N'SCHEMA', @level0name=N'Analytics', @level1type=N'TABLE', @level1name=N'ScoreCards', @level2type=N'COLUMN', @level2name=N'PerformanceIndex';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Analytics.ScoreCards') AND c.name = N'GeneratedForSiteId' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Site this scorecard was generated for, if site-scoped. Null means enterprise-wide.', @level0type=N'SCHEMA', @level0name=N'Analytics', @level1type=N'TABLE', @level1name=N'ScoreCards', @level2type=N'COLUMN', @level2name=N'GeneratedForSiteId';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Site this scorecard was generated for, if site-scoped. Null means enterprise-wide.', @level0type=N'SCHEMA', @level0name=N'Analytics', @level1type=N'TABLE', @level1name=N'ScoreCards', @level2type=N'COLUMN', @level2name=N'GeneratedForSiteId';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Analytics.ScoreCards') AND c.name = N'GeneratedAtUtc' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'UTC timestamp this scorecard was generated.', @level0type=N'SCHEMA', @level0name=N'Analytics', @level1type=N'TABLE', @level1name=N'ScoreCards', @level2type=N'COLUMN', @level2name=N'GeneratedAtUtc';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'UTC timestamp this scorecard was generated.', @level0type=N'SCHEMA', @level0name=N'Analytics', @level1type=N'TABLE', @level1name=N'ScoreCards', @level2type=N'COLUMN', @level2name=N'GeneratedAtUtc';
GO

------------------------------------------------------------
-- Compliance.Auditing
------------------------------------------------------------
IF EXISTS (SELECT 1 FROM sys.extended_properties WHERE major_id = OBJECT_ID(N'Compliance.Auditing') AND minor_id = 0 AND name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Aggregate root for the Auditing bounded context. Tracks a single audit program/execution record: type, scope, assigned auditor, schedule, findings volume, and computed compliance score.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Auditing';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Aggregate root for the Auditing bounded context. Tracks a single audit program/execution record: type, scope, assigned auditor, schedule, findings volume, and computed compliance score.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Auditing';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Compliance.Auditing') AND c.name = N'AuditTitle' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Human-readable audit program/execution title.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Auditing', @level2type=N'COLUMN', @level2name=N'AuditTitle';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Human-readable audit program/execution title.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Auditing', @level2type=N'COLUMN', @level2name=N'AuditTitle';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Compliance.Auditing') AND c.name = N'AuditType' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Audit classification: Internal, External, Regulatory, Supplier.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Auditing', @level2type=N'COLUMN', @level2name=N'AuditType';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Audit classification: Internal, External, Regulatory, Supplier.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Auditing', @level2type=N'COLUMN', @level2name=N'AuditType';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Compliance.Auditing') AND c.name = N'Scope' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Narrative scope of the audit (areas, processes, sites covered).', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Auditing', @level2type=N'COLUMN', @level2name=N'Scope';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Narrative scope of the audit (areas, processes, sites covered).', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Auditing', @level2type=N'COLUMN', @level2name=N'Scope';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Compliance.Auditing') AND c.name = N'AuditorId' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Identity of the lead auditor assigned to this audit.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Auditing', @level2type=N'COLUMN', @level2name=N'AuditorId';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Identity of the lead auditor assigned to this audit.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Auditing', @level2type=N'COLUMN', @level2name=N'AuditorId';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Compliance.Auditing') AND c.name = N'ScheduledAtUtc' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Scheduled execution date/time for the audit, if planned.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Auditing', @level2type=N'COLUMN', @level2name=N'ScheduledAtUtc';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Scheduled execution date/time for the audit, if planned.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Auditing', @level2type=N'COLUMN', @level2name=N'ScheduledAtUtc';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Compliance.Auditing') AND c.name = N'CompletedAtUtc' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'UTC timestamp the audit was completed, if finished.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Auditing', @level2type=N'COLUMN', @level2name=N'CompletedAtUtc';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'UTC timestamp the audit was completed, if finished.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Auditing', @level2type=N'COLUMN', @level2name=N'CompletedAtUtc';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Compliance.Auditing') AND c.name = N'FindingsCount' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Count of findings raised during this audit.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Auditing', @level2type=N'COLUMN', @level2name=N'FindingsCount';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Count of findings raised during this audit.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Auditing', @level2type=N'COLUMN', @level2name=N'FindingsCount';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Compliance.Auditing') AND c.name = N'Score' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Computed compliance score (0-100) for this audit, when available.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Auditing', @level2type=N'COLUMN', @level2name=N'Score';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Computed compliance score (0-100) for this audit, when available.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Auditing', @level2type=N'COLUMN', @level2name=N'Score';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Compliance.Auditing') AND c.name = N'ComplianceRating' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Overall compliance rating classification: NotRated, Satisfactory, NeedsImprovement, Unsatisfactory.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Auditing', @level2type=N'COLUMN', @level2name=N'ComplianceRating';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Overall compliance rating classification: NotRated, Satisfactory, NeedsImprovement, Unsatisfactory.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Auditing', @level2type=N'COLUMN', @level2name=N'ComplianceRating';
GO

------------------------------------------------------------
-- Compliance.Certification
------------------------------------------------------------
IF EXISTS (SELECT 1 FROM sys.extended_properties WHERE major_id = OBJECT_ID(N'Compliance.Certification') AND minor_id = 0 AND name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Tracks a single worker/site certification record: certification type, issuing body, issue/expiry dates, and status.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Certification';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Tracks a single worker/site certification record: certification type, issuing body, issue/expiry dates, and status.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Certification';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Compliance.Certification') AND c.name = N'HolderWorkerId' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Worker who holds this certification.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Certification', @level2type=N'COLUMN', @level2name=N'HolderWorkerId';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Worker who holds this certification.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Certification', @level2type=N'COLUMN', @level2name=N'HolderWorkerId';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Compliance.Certification') AND c.name = N'CertificationName' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Name of the certification (e.g., Confined Space Entry Supervisor).', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Certification', @level2type=N'COLUMN', @level2name=N'CertificationName';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Name of the certification (e.g., Confined Space Entry Supervisor).', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Certification', @level2type=N'COLUMN', @level2name=N'CertificationName';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Compliance.Certification') AND c.name = N'CertificationBody' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Issuing/certifying body (e.g., OSHA, NEBOSH).', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Certification', @level2type=N'COLUMN', @level2name=N'CertificationBody';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Issuing/certifying body (e.g., OSHA, NEBOSH).', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Certification', @level2type=N'COLUMN', @level2name=N'CertificationBody';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Compliance.Certification') AND c.name = N'CertificationNumber' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Certificate/credential number as issued by the certifying body.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Certification', @level2type=N'COLUMN', @level2name=N'CertificationNumber';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Certificate/credential number as issued by the certifying body.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Certification', @level2type=N'COLUMN', @level2name=N'CertificationNumber';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Compliance.Certification') AND c.name = N'IssuedAtUtc' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'UTC timestamp the certification was issued.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Certification', @level2type=N'COLUMN', @level2name=N'IssuedAtUtc';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'UTC timestamp the certification was issued.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Certification', @level2type=N'COLUMN', @level2name=N'IssuedAtUtc';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Compliance.Certification') AND c.name = N'ExpiresAtUtc' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'UTC timestamp the certification expires, if it has a validity window.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Certification', @level2type=N'COLUMN', @level2name=N'ExpiresAtUtc';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'UTC timestamp the certification expires, if it has a validity window.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Certification', @level2type=N'COLUMN', @level2name=N'ExpiresAtUtc';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Compliance.Certification') AND c.name = N'RenewalRequired' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Indicates whether this certification requires periodic renewal.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Certification', @level2type=N'COLUMN', @level2name=N'RenewalRequired';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Indicates whether this certification requires periodic renewal.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Certification', @level2type=N'COLUMN', @level2name=N'RenewalRequired';
GO

------------------------------------------------------------
-- Compliance.Compliance
------------------------------------------------------------
IF EXISTS (SELECT 1 FROM sys.extended_properties WHERE major_id = OBJECT_ID(N'Compliance.Compliance') AND minor_id = 0 AND name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Tracks a single regulatory-compliance obligation record and its current fulfillment status for a tenant/site.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Compliance';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Tracks a single regulatory-compliance obligation record and its current fulfillment status for a tenant/site.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Compliance';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Compliance.Compliance') AND c.name = N'RegulationReference' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Regulation/standard reference this record evaluates (e.g., "29 CFR 1910.147").', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Compliance', @level2type=N'COLUMN', @level2name=N'RegulationReference';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Regulation/standard reference this record evaluates (e.g., "29 CFR 1910.147").', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Compliance', @level2type=N'COLUMN', @level2name=N'RegulationReference';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Compliance.Compliance') AND c.name = N'RuleDescription' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Narrative description of the rule being evaluated.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Compliance', @level2type=N'COLUMN', @level2name=N'RuleDescription';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Narrative description of the rule being evaluated.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Compliance', @level2type=N'COLUMN', @level2name=N'RuleDescription';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Compliance.Compliance') AND c.name = N'ComplianceScore' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Computed compliance score (0-100) for this rule/regulation.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Compliance', @level2type=N'COLUMN', @level2name=N'ComplianceScore';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Computed compliance score (0-100) for this rule/regulation.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Compliance', @level2type=N'COLUMN', @level2name=N'ComplianceScore';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Compliance.Compliance') AND c.name = N'NonConformanceCount' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Count of open non-conformances tied to this compliance record.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Compliance', @level2type=N'COLUMN', @level2name=N'NonConformanceCount';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Count of open non-conformances tied to this compliance record.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Compliance', @level2type=N'COLUMN', @level2name=N'NonConformanceCount';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Compliance.Compliance') AND c.name = N'ResponsiblePartyId' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Identity of the party accountable for maintaining compliance.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Compliance', @level2type=N'COLUMN', @level2name=N'ResponsiblePartyId';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Identity of the party accountable for maintaining compliance.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Compliance', @level2type=N'COLUMN', @level2name=N'ResponsiblePartyId';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Compliance.Compliance') AND c.name = N'LastEvaluatedAtUtc' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'UTC timestamp this compliance record was last evaluated.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Compliance', @level2type=N'COLUMN', @level2name=N'LastEvaluatedAtUtc';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'UTC timestamp this compliance record was last evaluated.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Compliance', @level2type=N'COLUMN', @level2name=N'LastEvaluatedAtUtc';
GO

------------------------------------------------------------
-- Compliance.ManagementOfChange
------------------------------------------------------------
IF EXISTS (SELECT 1 FROM sys.extended_properties WHERE major_id = OBJECT_ID(N'Compliance.ManagementOfChange') AND minor_id = 0 AND name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Tracks a single Management of Change (MOC) request: the proposed change, risk assessment, approval workflow, and implementation status.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'ManagementOfChange';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Tracks a single Management of Change (MOC) request: the proposed change, risk assessment, approval workflow, and implementation status.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'ManagementOfChange';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Compliance.ManagementOfChange') AND c.name = N'ChangeTitle' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Human-readable title of the proposed change.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'ManagementOfChange', @level2type=N'COLUMN', @level2name=N'ChangeTitle';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Human-readable title of the proposed change.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'ManagementOfChange', @level2type=N'COLUMN', @level2name=N'ChangeTitle';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Compliance.ManagementOfChange') AND c.name = N'ChangeType' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Change classification: Process, Equipment, Personnel, Procedure, Software.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'ManagementOfChange', @level2type=N'COLUMN', @level2name=N'ChangeType';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Change classification: Process, Equipment, Personnel, Procedure, Software.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'ManagementOfChange', @level2type=N'COLUMN', @level2name=N'ChangeType';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Compliance.ManagementOfChange') AND c.name = N'Justification' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Business/safety justification for the change.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'ManagementOfChange', @level2type=N'COLUMN', @level2name=N'Justification';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Business/safety justification for the change.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'ManagementOfChange', @level2type=N'COLUMN', @level2name=N'Justification';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Compliance.ManagementOfChange') AND c.name = N'RiskLevel' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Risk level of the change: Low, Medium, High, Critical.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'ManagementOfChange', @level2type=N'COLUMN', @level2name=N'RiskLevel';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Risk level of the change: Low, Medium, High, Critical.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'ManagementOfChange', @level2type=N'COLUMN', @level2name=N'RiskLevel';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Compliance.ManagementOfChange') AND c.name = N'RequestedByUserId' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Identity of the user who requested the change.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'ManagementOfChange', @level2type=N'COLUMN', @level2name=N'RequestedByUserId';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Identity of the user who requested the change.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'ManagementOfChange', @level2type=N'COLUMN', @level2name=N'RequestedByUserId';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Compliance.ManagementOfChange') AND c.name = N'ApprovedByUserId' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Identity of the user who approved the change, if approved.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'ManagementOfChange', @level2type=N'COLUMN', @level2name=N'ApprovedByUserId';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Identity of the user who approved the change, if approved.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'ManagementOfChange', @level2type=N'COLUMN', @level2name=N'ApprovedByUserId';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Compliance.ManagementOfChange') AND c.name = N'ApprovalStatus' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Approval status: Pending, Approved, Rejected, Deferred.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'ManagementOfChange', @level2type=N'COLUMN', @level2name=N'ApprovalStatus';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Approval status: Pending, Approved, Rejected, Deferred.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'ManagementOfChange', @level2type=N'COLUMN', @level2name=N'ApprovalStatus';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Compliance.ManagementOfChange') AND c.name = N'ImplementationAtUtc' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Planned/actual implementation date for the change.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'ManagementOfChange', @level2type=N'COLUMN', @level2name=N'ImplementationAtUtc';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Planned/actual implementation date for the change.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'ManagementOfChange', @level2type=N'COLUMN', @level2name=N'ImplementationAtUtc';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Compliance.ManagementOfChange') AND c.name = N'ReviewDueAtUtc' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Scheduled post-implementation review date.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'ManagementOfChange', @level2type=N'COLUMN', @level2name=N'ReviewDueAtUtc';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Scheduled post-implementation review date.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'ManagementOfChange', @level2type=N'COLUMN', @level2name=N'ReviewDueAtUtc';
GO

------------------------------------------------------------
-- Compliance.Permits
------------------------------------------------------------
IF EXISTS (SELECT 1 FROM sys.extended_properties WHERE major_id = OBJECT_ID(N'Compliance.Permits') AND minor_id = 0 AND name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Tracks a single work permit (e.g., hot work, confined space) record: type, requestor, approval, validity window, and status.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Permits';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Tracks a single work permit (e.g., hot work, confined space) record: type, requestor, approval, validity window, and status.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Permits';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Compliance.Permits') AND c.name = N'PermitType' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Permit classification (e.g., HotWork, ConfinedSpace, Excavation, LOTO).', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Permits', @level2type=N'COLUMN', @level2name=N'PermitType';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Permit classification (e.g., HotWork, ConfinedSpace, Excavation, LOTO).', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Permits', @level2type=N'COLUMN', @level2name=N'PermitType';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Compliance.Permits') AND c.name = N'PermitNumber' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Human-readable permit number/reference.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Permits', @level2type=N'COLUMN', @level2name=N'PermitNumber';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Human-readable permit number/reference.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Permits', @level2type=N'COLUMN', @level2name=N'PermitNumber';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Compliance.Permits') AND c.name = N'WorkDescription' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Description of the work covered by this permit.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Permits', @level2type=N'COLUMN', @level2name=N'WorkDescription';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Description of the work covered by this permit.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Permits', @level2type=N'COLUMN', @level2name=N'WorkDescription';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Compliance.Permits') AND c.name = N'Location' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Physical location the permit applies to.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Permits', @level2type=N'COLUMN', @level2name=N'Location';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Physical location the permit applies to.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Permits', @level2type=N'COLUMN', @level2name=N'Location';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Compliance.Permits') AND c.name = N'HazardsIdentified' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Hazards identified for this permitted work.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Permits', @level2type=N'COLUMN', @level2name=N'HazardsIdentified';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Hazards identified for this permitted work.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Permits', @level2type=N'COLUMN', @level2name=N'HazardsIdentified';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Compliance.Permits') AND c.name = N'RequestedByUserId' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Identity of the user who requested the permit.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Permits', @level2type=N'COLUMN', @level2name=N'RequestedByUserId';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Identity of the user who requested the permit.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Permits', @level2type=N'COLUMN', @level2name=N'RequestedByUserId';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Compliance.Permits') AND c.name = N'ApprovedByUserId' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Identity of the user who approved/issued the permit, if issued.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Permits', @level2type=N'COLUMN', @level2name=N'ApprovedByUserId';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Identity of the user who approved/issued the permit, if issued.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Permits', @level2type=N'COLUMN', @level2name=N'ApprovedByUserId';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Compliance.Permits') AND c.name = N'IssuedAtUtc' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'UTC timestamp the permit was issued.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Permits', @level2type=N'COLUMN', @level2name=N'IssuedAtUtc';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'UTC timestamp the permit was issued.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Permits', @level2type=N'COLUMN', @level2name=N'IssuedAtUtc';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Compliance.Permits') AND c.name = N'ExpiresAtUtc' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'UTC timestamp the permit expires.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Permits', @level2type=N'COLUMN', @level2name=N'ExpiresAtUtc';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'UTC timestamp the permit expires.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Permits', @level2type=N'COLUMN', @level2name=N'ExpiresAtUtc';
GO

------------------------------------------------------------
-- Compliance.Policy
------------------------------------------------------------
IF EXISTS (SELECT 1 FROM sys.extended_properties WHERE major_id = OBJECT_ID(N'Compliance.Policy') AND minor_id = 0 AND name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Tracks a single governance policy document record: title, version, effective dates, and approval status.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Policy';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Tracks a single governance policy document record: title, version, effective dates, and approval status.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Policy';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Compliance.Policy') AND c.name = N'PolicyTitle' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Human-readable policy title.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Policy', @level2type=N'COLUMN', @level2name=N'PolicyTitle';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Human-readable policy title.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Policy', @level2type=N'COLUMN', @level2name=N'PolicyTitle';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Compliance.Policy') AND c.name = N'PolicyVersion' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Monotonically increasing policy revision number, starting at 1.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Policy', @level2type=N'COLUMN', @level2name=N'PolicyVersion';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Monotonically increasing policy revision number, starting at 1.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Policy', @level2type=N'COLUMN', @level2name=N'PolicyVersion';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Compliance.Policy') AND c.name = N'Category' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Policy category (e.g., PPE, HotWork, EmergencyResponse, GeneralSafety).', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Policy', @level2type=N'COLUMN', @level2name=N'Category';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Policy category (e.g., PPE, HotWork, EmergencyResponse, GeneralSafety).', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Policy', @level2type=N'COLUMN', @level2name=N'Category';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Compliance.Policy') AND c.name = N'OwnerId' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Identity of the accountable policy owner.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Policy', @level2type=N'COLUMN', @level2name=N'OwnerId';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Identity of the accountable policy owner.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Policy', @level2type=N'COLUMN', @level2name=N'OwnerId';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Compliance.Policy') AND c.name = N'EffectiveAtUtc' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'UTC timestamp the policy becomes effective.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Policy', @level2type=N'COLUMN', @level2name=N'EffectiveAtUtc';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'UTC timestamp the policy becomes effective.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Policy', @level2type=N'COLUMN', @level2name=N'EffectiveAtUtc';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Compliance.Policy') AND c.name = N'AcknowledgmentRequired' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Indicates whether personnel acknowledgment is required for this policy.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Policy', @level2type=N'COLUMN', @level2name=N'AcknowledgmentRequired';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Indicates whether personnel acknowledgment is required for this policy.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Policy', @level2type=N'COLUMN', @level2name=N'AcknowledgmentRequired';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Compliance.Policy') AND c.name = N'AcknowledgmentCount' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Count of personnel who have acknowledged this policy.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Policy', @level2type=N'COLUMN', @level2name=N'AcknowledgmentCount';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Count of personnel who have acknowledged this policy.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Policy', @level2type=N'COLUMN', @level2name=N'AcknowledgmentCount';
GO

------------------------------------------------------------
-- Compliance.Procedures
------------------------------------------------------------
IF EXISTS (SELECT 1 FROM sys.extended_properties WHERE major_id = OBJECT_ID(N'Compliance.Procedures') AND minor_id = 0 AND name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Tracks a single standard operating procedure (SOP) document record: title, version, and review/approval status.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Procedures';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Tracks a single standard operating procedure (SOP) document record: title, version, and review/approval status.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Procedures';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Compliance.Procedures') AND c.name = N'ProcedureTitle' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Human-readable procedure title (e.g., "Confined Space Entry SOP").', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Procedures', @level2type=N'COLUMN', @level2name=N'ProcedureTitle';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Human-readable procedure title (e.g., "Confined Space Entry SOP").', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Procedures', @level2type=N'COLUMN', @level2name=N'ProcedureTitle';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Compliance.Procedures') AND c.name = N'ProcedureNumber' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Procedure document number/reference.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Procedures', @level2type=N'COLUMN', @level2name=N'ProcedureNumber';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Procedure document number/reference.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Procedures', @level2type=N'COLUMN', @level2name=N'ProcedureNumber';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Compliance.Procedures') AND c.name = N'Category' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Procedure category (e.g., LOTO, HotWork, ConfinedSpace, EmergencyResponse).', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Procedures', @level2type=N'COLUMN', @level2name=N'Category';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Procedure category (e.g., LOTO, HotWork, ConfinedSpace, EmergencyResponse).', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Procedures', @level2type=N'COLUMN', @level2name=N'Category';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Compliance.Procedures') AND c.name = N'RevisionNumber' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Monotonically increasing revision number, starting at 1.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Procedures', @level2type=N'COLUMN', @level2name=N'RevisionNumber';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Monotonically increasing revision number, starting at 1.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Procedures', @level2type=N'COLUMN', @level2name=N'RevisionNumber';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Compliance.Procedures') AND c.name = N'OwnerId' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Identity of the accountable procedure owner.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Procedures', @level2type=N'COLUMN', @level2name=N'OwnerId';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Identity of the accountable procedure owner.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Procedures', @level2type=N'COLUMN', @level2name=N'OwnerId';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Compliance.Procedures') AND c.name = N'EffectiveAtUtc' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'UTC timestamp the procedure becomes effective.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Procedures', @level2type=N'COLUMN', @level2name=N'EffectiveAtUtc';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'UTC timestamp the procedure becomes effective.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Procedures', @level2type=N'COLUMN', @level2name=N'EffectiveAtUtc';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Compliance.Procedures') AND c.name = N'ReviewDueAtUtc' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'UTC timestamp the procedure is next due for review.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Procedures', @level2type=N'COLUMN', @level2name=N'ReviewDueAtUtc';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'UTC timestamp the procedure is next due for review.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'Procedures', @level2type=N'COLUMN', @level2name=N'ReviewDueAtUtc';
GO

------------------------------------------------------------
-- Compliance.RegulatoryCalendar
------------------------------------------------------------
IF EXISTS (SELECT 1 FROM sys.extended_properties WHERE major_id = OBJECT_ID(N'Compliance.RegulatoryCalendar') AND minor_id = 0 AND name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Tracks a single regulatory compliance deadline/event entry on the compliance calendar for a tenant/site.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'RegulatoryCalendar';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Tracks a single regulatory compliance deadline/event entry on the compliance calendar for a tenant/site.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'RegulatoryCalendar';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Compliance.RegulatoryCalendar') AND c.name = N'EventTitle' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Human-readable title of the regulatory event/obligation.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'RegulatoryCalendar', @level2type=N'COLUMN', @level2name=N'EventTitle';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Human-readable title of the regulatory event/obligation.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'RegulatoryCalendar', @level2type=N'COLUMN', @level2name=N'EventTitle';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Compliance.RegulatoryCalendar') AND c.name = N'RegulatoryBody' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Governing regulatory body (e.g., "OSHA", "EPA", "State DOL").', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'RegulatoryCalendar', @level2type=N'COLUMN', @level2name=N'RegulatoryBody';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Governing regulatory body (e.g., "OSHA", "EPA", "State DOL").', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'RegulatoryCalendar', @level2type=N'COLUMN', @level2name=N'RegulatoryBody';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Compliance.RegulatoryCalendar') AND c.name = N'JurisdictionCode' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Jurisdiction code the obligation applies to (e.g., "US-CA", "US-FED").', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'RegulatoryCalendar', @level2type=N'COLUMN', @level2name=N'JurisdictionCode';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Jurisdiction code the obligation applies to (e.g., "US-CA", "US-FED").', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'RegulatoryCalendar', @level2type=N'COLUMN', @level2name=N'JurisdictionCode';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Compliance.RegulatoryCalendar') AND c.name = N'ComplianceCategory' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Compliance category (e.g., Reporting, Inspection, Renewal, Filing).', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'RegulatoryCalendar', @level2type=N'COLUMN', @level2name=N'ComplianceCategory';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Compliance category (e.g., Reporting, Inspection, Renewal, Filing).', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'RegulatoryCalendar', @level2type=N'COLUMN', @level2name=N'ComplianceCategory';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Compliance.RegulatoryCalendar') AND c.name = N'ResponsiblePartyId' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Identity of the party accountable for meeting this obligation.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'RegulatoryCalendar', @level2type=N'COLUMN', @level2name=N'ResponsiblePartyId';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Identity of the party accountable for meeting this obligation.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'RegulatoryCalendar', @level2type=N'COLUMN', @level2name=N'ResponsiblePartyId';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Compliance.RegulatoryCalendar') AND c.name = N'DueAtUtc' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'UTC timestamp the obligation is due.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'RegulatoryCalendar', @level2type=N'COLUMN', @level2name=N'DueAtUtc';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'UTC timestamp the obligation is due.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'RegulatoryCalendar', @level2type=N'COLUMN', @level2name=N'DueAtUtc';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Compliance.RegulatoryCalendar') AND c.name = N'ReminderLeadDays' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Number of days before DueAtUtc that reminders should fire.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'RegulatoryCalendar', @level2type=N'COLUMN', @level2name=N'ReminderLeadDays';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Number of days before DueAtUtc that reminders should fire.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'TABLE', @level1name=N'RegulatoryCalendar', @level2type=N'COLUMN', @level2name=N'ReminderLeadDays';
GO

------------------------------------------------------------
-- Operational.Checklists
------------------------------------------------------------
IF EXISTS (SELECT 1 FROM sys.extended_properties WHERE major_id = OBJECT_ID(N'Operational.Checklists') AND minor_id = 0 AND name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Tracks a single operational safety checklist template or completed-checklist instance record.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'Checklists';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Tracks a single operational safety checklist template or completed-checklist instance record.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'Checklists';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Operational.Checklists') AND c.name = N'ChecklistTitle' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Human-readable checklist title.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'Checklists', @level2type=N'COLUMN', @level2name=N'ChecklistTitle';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Human-readable checklist title.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'Checklists', @level2type=N'COLUMN', @level2name=N'ChecklistTitle';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Operational.Checklists') AND c.name = N'ChecklistType' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Checklist classification (e.g., PreShift, EquipmentInspection, SafetyWalkdown).', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'Checklists', @level2type=N'COLUMN', @level2name=N'ChecklistType';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Checklist classification (e.g., PreShift, EquipmentInspection, SafetyWalkdown).', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'Checklists', @level2type=N'COLUMN', @level2name=N'ChecklistType';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Operational.Checklists') AND c.name = N'Location' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Physical location the checklist was executed at.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'Checklists', @level2type=N'COLUMN', @level2name=N'Location';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Physical location the checklist was executed at.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'Checklists', @level2type=N'COLUMN', @level2name=N'Location';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Operational.Checklists') AND c.name = N'TotalItems' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Total number of items on this checklist.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'Checklists', @level2type=N'COLUMN', @level2name=N'TotalItems';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Total number of items on this checklist.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'Checklists', @level2type=N'COLUMN', @level2name=N'TotalItems';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Operational.Checklists') AND c.name = N'CompletedItems' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Number of items completed so far.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'Checklists', @level2type=N'COLUMN', @level2name=N'CompletedItems';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Number of items completed so far.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'Checklists', @level2type=N'COLUMN', @level2name=N'CompletedItems';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Operational.Checklists') AND c.name = N'ExceptionCount' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Count of exceptions/failures raised during execution.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'Checklists', @level2type=N'COLUMN', @level2name=N'ExceptionCount';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Count of exceptions/failures raised during execution.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'Checklists', @level2type=N'COLUMN', @level2name=N'ExceptionCount';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Operational.Checklists') AND c.name = N'CompletedByUserId' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Identity of the user who completed the checklist, if completed.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'Checklists', @level2type=N'COLUMN', @level2name=N'CompletedByUserId';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Identity of the user who completed the checklist, if completed.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'Checklists', @level2type=N'COLUMN', @level2name=N'CompletedByUserId';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Operational.Checklists') AND c.name = N'CompletedAtUtc' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'UTC timestamp the checklist was completed.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'Checklists', @level2type=N'COLUMN', @level2name=N'CompletedAtUtc';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'UTC timestamp the checklist was completed.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'Checklists', @level2type=N'COLUMN', @level2name=N'CompletedAtUtc';
GO

------------------------------------------------------------
-- Operational.Contractor
------------------------------------------------------------
IF EXISTS (SELECT 1 FROM sys.extended_properties WHERE major_id = OBJECT_ID(N'Operational.Contractor') AND minor_id = 0 AND name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Tracks a single contractor/visitor safety-qualification and site-access record.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'Contractor';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Tracks a single contractor/visitor safety-qualification and site-access record.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'Contractor';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Operational.Contractor') AND c.name = N'ContractorCompany' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Contractor company/organization name.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'Contractor', @level2type=N'COLUMN', @level2name=N'ContractorCompany';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Contractor company/organization name.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'Contractor', @level2type=N'COLUMN', @level2name=N'ContractorCompany';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Operational.Contractor') AND c.name = N'ContractorName' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Individual contractor/worker name, if this record represents an individual.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'Contractor', @level2type=N'COLUMN', @level2name=N'ContractorName';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Individual contractor/worker name, if this record represents an individual.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'Contractor', @level2type=N'COLUMN', @level2name=N'ContractorName';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Operational.Contractor') AND c.name = N'QualificationStatus' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Qualification status: Pending, Qualified, Expired, Disqualified.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'Contractor', @level2type=N'COLUMN', @level2name=N'QualificationStatus';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Qualification status: Pending, Qualified, Expired, Disqualified.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'Contractor', @level2type=N'COLUMN', @level2name=N'QualificationStatus';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Operational.Contractor') AND c.name = N'ReadinessScore' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Computed contractor readiness/compliance score (0-100).', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'Contractor', @level2type=N'COLUMN', @level2name=N'ReadinessScore';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Computed contractor readiness/compliance score (0-100).', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'Contractor', @level2type=N'COLUMN', @level2name=N'ReadinessScore';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Operational.Contractor') AND c.name = N'PrimaryContactId' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Identity of the primary contact for this contractor engagement.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'Contractor', @level2type=N'COLUMN', @level2name=N'PrimaryContactId';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Identity of the primary contact for this contractor engagement.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'Contractor', @level2type=N'COLUMN', @level2name=N'PrimaryContactId';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Operational.Contractor') AND c.name = N'InsuranceExpiresAtUtc' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'UTC timestamp the contractor''s insurance coverage expires.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'Contractor', @level2type=N'COLUMN', @level2name=N'InsuranceExpiresAtUtc';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'UTC timestamp the contractor''s insurance coverage expires.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'Contractor', @level2type=N'COLUMN', @level2name=N'InsuranceExpiresAtUtc';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Operational.Contractor') AND c.name = N'OnboardedAtUtc' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'UTC timestamp the contractor completed onboarding.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'Contractor', @level2type=N'COLUMN', @level2name=N'OnboardedAtUtc';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'UTC timestamp the contractor completed onboarding.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'Contractor', @level2type=N'COLUMN', @level2name=N'OnboardedAtUtc';
GO

------------------------------------------------------------
-- Operational.PPEManagement
------------------------------------------------------------
IF EXISTS (SELECT 1 FROM sys.extended_properties WHERE major_id = OBJECT_ID(N'Operational.PPEManagement') AND minor_id = 0 AND name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Tracks a single Personal Protective Equipment (PPE) issuance/inventory record for a worker or site.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'PPEManagement';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Tracks a single Personal Protective Equipment (PPE) issuance/inventory record for a worker or site.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'PPEManagement';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Operational.PPEManagement') AND c.name = N'WorkerId' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Worker this PPE record is issued to.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'PPEManagement', @level2type=N'COLUMN', @level2name=N'WorkerId';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Worker this PPE record is issued to.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'PPEManagement', @level2type=N'COLUMN', @level2name=N'WorkerId';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Operational.PPEManagement') AND c.name = N'PpeType' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'PPE type classification (e.g., HeadProtection, RespiratoryProtection, FallProtection).', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'PPEManagement', @level2type=N'COLUMN', @level2name=N'PpeType';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'PPE type classification (e.g., HeadProtection, RespiratoryProtection, FallProtection).', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'PPEManagement', @level2type=N'COLUMN', @level2name=N'PpeType';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Operational.PPEManagement') AND c.name = N'PpeItem' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Specific PPE item description/model.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'PPEManagement', @level2type=N'COLUMN', @level2name=N'PpeItem';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Specific PPE item description/model.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'PPEManagement', @level2type=N'COLUMN', @level2name=N'PpeItem';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Operational.PPEManagement') AND c.name = N'ComplianceStatus' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Compliance status: Compliant, Overdue, Expired, NonCompliant.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'PPEManagement', @level2type=N'COLUMN', @level2name=N'ComplianceStatus';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Compliance status: Compliant, Overdue, Expired, NonCompliant.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'PPEManagement', @level2type=N'COLUMN', @level2name=N'ComplianceStatus';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Operational.PPEManagement') AND c.name = N'IssuedAtUtc' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'UTC timestamp the PPE was issued.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'PPEManagement', @level2type=N'COLUMN', @level2name=N'IssuedAtUtc';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'UTC timestamp the PPE was issued.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'PPEManagement', @level2type=N'COLUMN', @level2name=N'IssuedAtUtc';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Operational.PPEManagement') AND c.name = N'ReturnedAtUtc' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'UTC timestamp the PPE was returned, if applicable.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'PPEManagement', @level2type=N'COLUMN', @level2name=N'ReturnedAtUtc';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'UTC timestamp the PPE was returned, if applicable.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'PPEManagement', @level2type=N'COLUMN', @level2name=N'ReturnedAtUtc';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Operational.PPEManagement') AND c.name = N'InspectionDueAtUtc' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'UTC timestamp the PPE is next due for inspection.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'PPEManagement', @level2type=N'COLUMN', @level2name=N'InspectionDueAtUtc';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'UTC timestamp the PPE is next due for inspection.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'PPEManagement', @level2type=N'COLUMN', @level2name=N'InspectionDueAtUtc';
GO

------------------------------------------------------------
-- Operational.RiskManagement
------------------------------------------------------------
IF EXISTS (SELECT 1 FROM sys.extended_properties WHERE major_id = OBJECT_ID(N'Operational.RiskManagement') AND minor_id = 0 AND name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Tracks a single identified operational risk record: hazard description, likelihood/severity assessment, and mitigation status.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'RiskManagement';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Tracks a single identified operational risk record: hazard description, likelihood/severity assessment, and mitigation status.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'RiskManagement';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Operational.RiskManagement') AND c.name = N'HazardDescription' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Description of the identified hazard.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'RiskManagement', @level2type=N'COLUMN', @level2name=N'HazardDescription';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Description of the identified hazard.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'RiskManagement', @level2type=N'COLUMN', @level2name=N'HazardDescription';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Operational.RiskManagement') AND c.name = N'RiskCategory' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Risk category (e.g., Mechanical, Chemical, Electrical, Ergonomic, Fire).', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'RiskManagement', @level2type=N'COLUMN', @level2name=N'RiskCategory';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Risk category (e.g., Mechanical, Chemical, Electrical, Ergonomic, Fire).', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'RiskManagement', @level2type=N'COLUMN', @level2name=N'RiskCategory';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Operational.RiskManagement') AND c.name = N'LikelihoodRating' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Likelihood rating on a 1-5 scale.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'RiskManagement', @level2type=N'COLUMN', @level2name=N'LikelihoodRating';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Likelihood rating on a 1-5 scale.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'RiskManagement', @level2type=N'COLUMN', @level2name=N'LikelihoodRating';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Operational.RiskManagement') AND c.name = N'SeverityRating' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Severity rating on a 1-5 scale.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'RiskManagement', @level2type=N'COLUMN', @level2name=N'SeverityRating';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Severity rating on a 1-5 scale.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'RiskManagement', @level2type=N'COLUMN', @level2name=N'SeverityRating';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Operational.RiskManagement') AND c.name = N'ControlMeasures' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Computed risk score (LikelihoodRating * SeverityRating).</summary> public int RiskScore => LikelihoodRating * SeverityRating; /// <summary>Control measures identified to mitigate this risk.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'RiskManagement', @level2type=N'COLUMN', @level2name=N'ControlMeasures';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Computed risk score (LikelihoodRating * SeverityRating).</summary> public int RiskScore => LikelihoodRating * SeverityRating; /// <summary>Control measures identified to mitigate this risk.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'RiskManagement', @level2type=N'COLUMN', @level2name=N'ControlMeasures';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Operational.RiskManagement') AND c.name = N'ResponsiblePartyId' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Identity of the party accountable for risk control verification.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'RiskManagement', @level2type=N'COLUMN', @level2name=N'ResponsiblePartyId';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Identity of the party accountable for risk control verification.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'RiskManagement', @level2type=N'COLUMN', @level2name=N'ResponsiblePartyId';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Operational.RiskManagement') AND c.name = N'ReviewDueAtUtc' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'UTC timestamp this risk assessment is next due for review.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'RiskManagement', @level2type=N'COLUMN', @level2name=N'ReviewDueAtUtc';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'UTC timestamp this risk assessment is next due for review.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'RiskManagement', @level2type=N'COLUMN', @level2name=N'ReviewDueAtUtc';
GO

------------------------------------------------------------
-- Operational.Schedules
------------------------------------------------------------
IF EXISTS (SELECT 1 FROM sys.extended_properties WHERE major_id = OBJECT_ID(N'Operational.Schedules') AND minor_id = 0 AND name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Tracks a single recurring or one-time operational safety task/schedule assignment record.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'Schedules';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Tracks a single recurring or one-time operational safety task/schedule assignment record.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'Schedules';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Operational.Schedules') AND c.name = N'TaskName' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Human-readable task name.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'Schedules', @level2type=N'COLUMN', @level2name=N'TaskName';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Human-readable task name.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'Schedules', @level2type=N'COLUMN', @level2name=N'TaskName';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Operational.Schedules') AND c.name = N'RecurrencePattern' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Recurrence pattern (e.g., Daily, Weekly, Monthly, Quarterly, OneTime).', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'Schedules', @level2type=N'COLUMN', @level2name=N'RecurrencePattern';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Recurrence pattern (e.g., Daily, Weekly, Monthly, Quarterly, OneTime).', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'Schedules', @level2type=N'COLUMN', @level2name=N'RecurrencePattern';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Operational.Schedules') AND c.name = N'AssignedToUserId' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Identity of the user assigned to complete this task.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'Schedules', @level2type=N'COLUMN', @level2name=N'AssignedToUserId';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Identity of the user assigned to complete this task.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'Schedules', @level2type=N'COLUMN', @level2name=N'AssignedToUserId';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Operational.Schedules') AND c.name = N'DueAtUtc' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'UTC timestamp this task instance is due.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'Schedules', @level2type=N'COLUMN', @level2name=N'DueAtUtc';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'UTC timestamp this task instance is due.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'Schedules', @level2type=N'COLUMN', @level2name=N'DueAtUtc';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Operational.Schedules') AND c.name = N'SlaWindowMinutes' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'SLA window, in minutes, allowed for task completion once due.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'Schedules', @level2type=N'COLUMN', @level2name=N'SlaWindowMinutes';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'SLA window, in minutes, allowed for task completion once due.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'Schedules', @level2type=N'COLUMN', @level2name=N'SlaWindowMinutes';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Operational.Schedules') AND c.name = N'CompletedAtUtc' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'UTC timestamp this task instance was completed.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'Schedules', @level2type=N'COLUMN', @level2name=N'CompletedAtUtc';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'UTC timestamp this task instance was completed.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'Schedules', @level2type=N'COLUMN', @level2name=N'CompletedAtUtc';
GO

------------------------------------------------------------
-- Operational.Training
------------------------------------------------------------
IF EXISTS (SELECT 1 FROM sys.extended_properties WHERE major_id = OBJECT_ID(N'Operational.Training') AND minor_id = 0 AND name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Tracks a single worker safety-training assignment/completion record: course, due date, and completion status.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'Training';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Tracks a single worker safety-training assignment/completion record: course, due date, and completion status.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'Training';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Operational.Training') AND c.name = N'CourseName' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Human-readable course name.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'Training', @level2type=N'COLUMN', @level2name=N'CourseName';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Human-readable course name.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'Training', @level2type=N'COLUMN', @level2name=N'CourseName';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Operational.Training') AND c.name = N'CourseCode' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Course catalog code/reference.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'Training', @level2type=N'COLUMN', @level2name=N'CourseCode';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Course catalog code/reference.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'Training', @level2type=N'COLUMN', @level2name=N'CourseCode';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Operational.Training') AND c.name = N'WorkerId' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Worker this training is assigned to.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'Training', @level2type=N'COLUMN', @level2name=N'WorkerId';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Worker this training is assigned to.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'Training', @level2type=N'COLUMN', @level2name=N'WorkerId';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Operational.Training') AND c.name = N'ComplianceCategory' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Compliance category (e.g., Mandatory, Refresher, Elective).', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'Training', @level2type=N'COLUMN', @level2name=N'ComplianceCategory';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Compliance category (e.g., Mandatory, Refresher, Elective).', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'Training', @level2type=N'COLUMN', @level2name=N'ComplianceCategory';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Operational.Training') AND c.name = N'AssignedAtUtc' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'UTC timestamp the training was assigned.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'Training', @level2type=N'COLUMN', @level2name=N'AssignedAtUtc';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'UTC timestamp the training was assigned.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'Training', @level2type=N'COLUMN', @level2name=N'AssignedAtUtc';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Operational.Training') AND c.name = N'DueAtUtc' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'UTC timestamp the training is due for completion.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'Training', @level2type=N'COLUMN', @level2name=N'DueAtUtc';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'UTC timestamp the training is due for completion.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'Training', @level2type=N'COLUMN', @level2name=N'DueAtUtc';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Operational.Training') AND c.name = N'CompletedAtUtc' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'UTC timestamp the training was completed.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'Training', @level2type=N'COLUMN', @level2name=N'CompletedAtUtc';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'UTC timestamp the training was completed.', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'Training', @level2type=N'COLUMN', @level2name=N'CompletedAtUtc';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'Operational.Training') AND c.name = N'Score' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Assessment score achieved, if the course includes an assessment (0-100).', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'Training', @level2type=N'COLUMN', @level2name=N'Score';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Assessment score achieved, if the course includes an assessment (0-100).', @level0type=N'SCHEMA', @level0name=N'Operational', @level1type=N'TABLE', @level1name=N'Training', @level2type=N'COLUMN', @level2name=N'Score';
GO

------------------------------------------------------------
-- SafetyEvents.EmergencyManagement
------------------------------------------------------------
IF EXISTS (SELECT 1 FROM sys.extended_properties WHERE major_id = OBJECT_ID(N'SafetyEvents.EmergencyManagement') AND minor_id = 0 AND name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Tracks a single emergency-event or drill record: type, response actions, and resolution status.', @level0type=N'SCHEMA', @level0name=N'SafetyEvents', @level1type=N'TABLE', @level1name=N'EmergencyManagement';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Tracks a single emergency-event or drill record: type, response actions, and resolution status.', @level0type=N'SCHEMA', @level0name=N'SafetyEvents', @level1type=N'TABLE', @level1name=N'EmergencyManagement';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'SafetyEvents.EmergencyManagement') AND c.name = N'EmergencyType' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Emergency classification (e.g., Fire, ChemicalRelease, MedicalEmergency, Evacuation, SevereWeather).', @level0type=N'SCHEMA', @level0name=N'SafetyEvents', @level1type=N'TABLE', @level1name=N'EmergencyManagement', @level2type=N'COLUMN', @level2name=N'EmergencyType';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Emergency classification (e.g., Fire, ChemicalRelease, MedicalEmergency, Evacuation, SevereWeather).', @level0type=N'SCHEMA', @level0name=N'SafetyEvents', @level1type=N'TABLE', @level1name=N'EmergencyManagement', @level2type=N'COLUMN', @level2name=N'EmergencyType';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'SafetyEvents.EmergencyManagement') AND c.name = N'Severity' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Emergency severity: Minor, Moderate, Major, Catastrophic.', @level0type=N'SCHEMA', @level0name=N'SafetyEvents', @level1type=N'TABLE', @level1name=N'EmergencyManagement', @level2type=N'COLUMN', @level2name=N'Severity';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Emergency severity: Minor, Moderate, Major, Catastrophic.', @level0type=N'SCHEMA', @level0name=N'SafetyEvents', @level1type=N'TABLE', @level1name=N'EmergencyManagement', @level2type=N'COLUMN', @level2name=N'Severity';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'SafetyEvents.EmergencyManagement') AND c.name = N'Location' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Physical location the emergency was declared at.', @level0type=N'SCHEMA', @level0name=N'SafetyEvents', @level1type=N'TABLE', @level1name=N'EmergencyManagement', @level2type=N'COLUMN', @level2name=N'Location';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Physical location the emergency was declared at.', @level0type=N'SCHEMA', @level0name=N'SafetyEvents', @level1type=N'TABLE', @level1name=N'EmergencyManagement', @level2type=N'COLUMN', @level2name=N'Location';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'SafetyEvents.EmergencyManagement') AND c.name = N'CasualtyCount' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Count of casualties/injuries associated with this emergency.', @level0type=N'SCHEMA', @level0name=N'SafetyEvents', @level1type=N'TABLE', @level1name=N'EmergencyManagement', @level2type=N'COLUMN', @level2name=N'CasualtyCount';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Count of casualties/injuries associated with this emergency.', @level0type=N'SCHEMA', @level0name=N'SafetyEvents', @level1type=N'TABLE', @level1name=N'EmergencyManagement', @level2type=N'COLUMN', @level2name=N'CasualtyCount';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'SafetyEvents.EmergencyManagement') AND c.name = N'ResponseTeamNotified' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Indicates whether the emergency response team has been notified.', @level0type=N'SCHEMA', @level0name=N'SafetyEvents', @level1type=N'TABLE', @level1name=N'EmergencyManagement', @level2type=N'COLUMN', @level2name=N'ResponseTeamNotified';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Indicates whether the emergency response team has been notified.', @level0type=N'SCHEMA', @level0name=N'SafetyEvents', @level1type=N'TABLE', @level1name=N'EmergencyManagement', @level2type=N'COLUMN', @level2name=N'ResponseTeamNotified';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'SafetyEvents.EmergencyManagement') AND c.name = N'DeclaredAtUtc' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'UTC timestamp the emergency was declared.', @level0type=N'SCHEMA', @level0name=N'SafetyEvents', @level1type=N'TABLE', @level1name=N'EmergencyManagement', @level2type=N'COLUMN', @level2name=N'DeclaredAtUtc';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'UTC timestamp the emergency was declared.', @level0type=N'SCHEMA', @level0name=N'SafetyEvents', @level1type=N'TABLE', @level1name=N'EmergencyManagement', @level2type=N'COLUMN', @level2name=N'DeclaredAtUtc';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'SafetyEvents.EmergencyManagement') AND c.name = N'StabilizedAtUtc' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'UTC timestamp the emergency was stabilized.', @level0type=N'SCHEMA', @level0name=N'SafetyEvents', @level1type=N'TABLE', @level1name=N'EmergencyManagement', @level2type=N'COLUMN', @level2name=N'StabilizedAtUtc';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'UTC timestamp the emergency was stabilized.', @level0type=N'SCHEMA', @level0name=N'SafetyEvents', @level1type=N'TABLE', @level1name=N'EmergencyManagement', @level2type=N'COLUMN', @level2name=N'StabilizedAtUtc';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'SafetyEvents.EmergencyManagement') AND c.name = N'ResolvedAtUtc' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'UTC timestamp the emergency was resolved/closed.', @level0type=N'SCHEMA', @level0name=N'SafetyEvents', @level1type=N'TABLE', @level1name=N'EmergencyManagement', @level2type=N'COLUMN', @level2name=N'ResolvedAtUtc';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'UTC timestamp the emergency was resolved/closed.', @level0type=N'SCHEMA', @level0name=N'SafetyEvents', @level1type=N'TABLE', @level1name=N'EmergencyManagement', @level2type=N'COLUMN', @level2name=N'ResolvedAtUtc';
GO

------------------------------------------------------------
-- SafetyEvents.IncidentManagement
------------------------------------------------------------
IF EXISTS (SELECT 1 FROM sys.extended_properties WHERE major_id = OBJECT_ID(N'SafetyEvents.IncidentManagement') AND minor_id = 0 AND name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Aggregate root for the Incident Management bounded context. Tracks a single safety incident record: classification, severity, location, injured parties, investigation, and corrective-action status.', @level0type=N'SCHEMA', @level0name=N'SafetyEvents', @level1type=N'TABLE', @level1name=N'IncidentManagement';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Aggregate root for the Incident Management bounded context. Tracks a single safety incident record: classification, severity, location, injured parties, investigation, and corrective-action status.', @level0type=N'SCHEMA', @level0name=N'SafetyEvents', @level1type=N'TABLE', @level1name=N'IncidentManagement';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'SafetyEvents.IncidentManagement') AND c.name = N'Title' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Human-readable incident title/summary.', @level0type=N'SCHEMA', @level0name=N'SafetyEvents', @level1type=N'TABLE', @level1name=N'IncidentManagement', @level2type=N'COLUMN', @level2name=N'Title';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Human-readable incident title/summary.', @level0type=N'SCHEMA', @level0name=N'SafetyEvents', @level1type=N'TABLE', @level1name=N'IncidentManagement', @level2type=N'COLUMN', @level2name=N'Title';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'SafetyEvents.IncidentManagement') AND c.name = N'Description' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Narrative description of what happened.', @level0type=N'SCHEMA', @level0name=N'SafetyEvents', @level1type=N'TABLE', @level1name=N'IncidentManagement', @level2type=N'COLUMN', @level2name=N'Description';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Narrative description of what happened.', @level0type=N'SCHEMA', @level0name=N'SafetyEvents', @level1type=N'TABLE', @level1name=N'IncidentManagement', @level2type=N'COLUMN', @level2name=N'Description';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'SafetyEvents.IncidentManagement') AND c.name = N'Severity' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Incident severity: Low, Medium, High, Critical.', @level0type=N'SCHEMA', @level0name=N'SafetyEvents', @level1type=N'TABLE', @level1name=N'IncidentManagement', @level2type=N'COLUMN', @level2name=N'Severity';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Incident severity: Low, Medium, High, Critical.', @level0type=N'SCHEMA', @level0name=N'SafetyEvents', @level1type=N'TABLE', @level1name=N'IncidentManagement', @level2type=N'COLUMN', @level2name=N'Severity';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'SafetyEvents.IncidentManagement') AND c.name = N'Location' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Physical location/equipment where the incident occurred.', @level0type=N'SCHEMA', @level0name=N'SafetyEvents', @level1type=N'TABLE', @level1name=N'IncidentManagement', @level2type=N'COLUMN', @level2name=N'Location';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Physical location/equipment where the incident occurred.', @level0type=N'SCHEMA', @level0name=N'SafetyEvents', @level1type=N'TABLE', @level1name=N'IncidentManagement', @level2type=N'COLUMN', @level2name=N'Location';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'SafetyEvents.IncidentManagement') AND c.name = N'IncidentOccurredAtUtc' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'UTC timestamp the incident actually occurred (may differ from CreatedAtUtc).', @level0type=N'SCHEMA', @level0name=N'SafetyEvents', @level1type=N'TABLE', @level1name=N'IncidentManagement', @level2type=N'COLUMN', @level2name=N'IncidentOccurredAtUtc';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'UTC timestamp the incident actually occurred (may differ from CreatedAtUtc).', @level0type=N'SCHEMA', @level0name=N'SafetyEvents', @level1type=N'TABLE', @level1name=N'IncidentManagement', @level2type=N'COLUMN', @level2name=N'IncidentOccurredAtUtc';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'SafetyEvents.IncidentManagement') AND c.name = N'InjuryType' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Injury classification, when personnel injury occurred (e.g., None, FirstAid, MedicalTreatment, LostTime, Fatality).', @level0type=N'SCHEMA', @level0name=N'SafetyEvents', @level1type=N'TABLE', @level1name=N'IncidentManagement', @level2type=N'COLUMN', @level2name=N'InjuryType';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Injury classification, when personnel injury occurred (e.g., None, FirstAid, MedicalTreatment, LostTime, Fatality).', @level0type=N'SCHEMA', @level0name=N'SafetyEvents', @level1type=N'TABLE', @level1name=N'IncidentManagement', @level2type=N'COLUMN', @level2name=N'InjuryType';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'SafetyEvents.IncidentManagement') AND c.name = N'ReportedByWorkerId' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Identity of the worker who reported the incident.', @level0type=N'SCHEMA', @level0name=N'SafetyEvents', @level1type=N'TABLE', @level1name=N'IncidentManagement', @level2type=N'COLUMN', @level2name=N'ReportedByWorkerId';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Identity of the worker who reported the incident.', @level0type=N'SCHEMA', @level0name=N'SafetyEvents', @level1type=N'TABLE', @level1name=N'IncidentManagement', @level2type=N'COLUMN', @level2name=N'ReportedByWorkerId';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'SafetyEvents.IncidentManagement') AND c.name = N'AssignedInvestigatorId' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Identity of the investigator assigned to this incident, if assigned.', @level0type=N'SCHEMA', @level0name=N'SafetyEvents', @level1type=N'TABLE', @level1name=N'IncidentManagement', @level2type=N'COLUMN', @level2name=N'AssignedInvestigatorId';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Identity of the investigator assigned to this incident, if assigned.', @level0type=N'SCHEMA', @level0name=N'SafetyEvents', @level1type=N'TABLE', @level1name=N'IncidentManagement', @level2type=N'COLUMN', @level2name=N'AssignedInvestigatorId';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'SafetyEvents.IncidentManagement') AND c.name = N'RootCause' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Root cause identified during investigation/RCA, once determined.', @level0type=N'SCHEMA', @level0name=N'SafetyEvents', @level1type=N'TABLE', @level1name=N'IncidentManagement', @level2type=N'COLUMN', @level2name=N'RootCause';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Root cause identified during investigation/RCA, once determined.', @level0type=N'SCHEMA', @level0name=N'SafetyEvents', @level1type=N'TABLE', @level1name=N'IncidentManagement', @level2type=N'COLUMN', @level2name=N'RootCause';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'SafetyEvents.IncidentManagement') AND c.name = N'CorrectiveActions' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Corrective actions identified/implemented as a result of this incident.', @level0type=N'SCHEMA', @level0name=N'SafetyEvents', @level1type=N'TABLE', @level1name=N'IncidentManagement', @level2type=N'COLUMN', @level2name=N'CorrectiveActions';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Corrective actions identified/implemented as a result of this incident.', @level0type=N'SCHEMA', @level0name=N'SafetyEvents', @level1type=N'TABLE', @level1name=N'IncidentManagement', @level2type=N'COLUMN', @level2name=N'CorrectiveActions';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'SafetyEvents.IncidentManagement') AND c.name = N'ClosedAtUtc' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'UTC timestamp the incident was closed, once resolved.', @level0type=N'SCHEMA', @level0name=N'SafetyEvents', @level1type=N'TABLE', @level1name=N'IncidentManagement', @level2type=N'COLUMN', @level2name=N'ClosedAtUtc';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'UTC timestamp the incident was closed, once resolved.', @level0type=N'SCHEMA', @level0name=N'SafetyEvents', @level1type=N'TABLE', @level1name=N'IncidentManagement', @level2type=N'COLUMN', @level2name=N'ClosedAtUtc';
GO

------------------------------------------------------------
-- SafetyEvents.Observation
------------------------------------------------------------
IF EXISTS (SELECT 1 FROM sys.extended_properties WHERE major_id = OBJECT_ID(N'SafetyEvents.Observation') AND minor_id = 0 AND name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Tracks a single safety observation (behavior-based safety) record submitted by a worker/observer.', @level0type=N'SCHEMA', @level0name=N'SafetyEvents', @level1type=N'TABLE', @level1name=N'Observation';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Tracks a single safety observation (behavior-based safety) record submitted by a worker/observer.', @level0type=N'SCHEMA', @level0name=N'SafetyEvents', @level1type=N'TABLE', @level1name=N'Observation';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'SafetyEvents.Observation') AND c.name = N'ObserverId' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Identity of the worker/observer who captured this observation.', @level0type=N'SCHEMA', @level0name=N'SafetyEvents', @level1type=N'TABLE', @level1name=N'Observation', @level2type=N'COLUMN', @level2name=N'ObserverId';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Identity of the worker/observer who captured this observation.', @level0type=N'SCHEMA', @level0name=N'SafetyEvents', @level1type=N'TABLE', @level1name=N'Observation', @level2type=N'COLUMN', @level2name=N'ObserverId';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'SafetyEvents.Observation') AND c.name = N'ObservationType' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Observation classification: SafeBehavior, AtRiskBehavior, HazardCondition.', @level0type=N'SCHEMA', @level0name=N'SafetyEvents', @level1type=N'TABLE', @level1name=N'Observation', @level2type=N'COLUMN', @level2name=N'ObservationType';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Observation classification: SafeBehavior, AtRiskBehavior, HazardCondition.', @level0type=N'SCHEMA', @level0name=N'SafetyEvents', @level1type=N'TABLE', @level1name=N'Observation', @level2type=N'COLUMN', @level2name=N'ObservationType';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'SafetyEvents.Observation') AND c.name = N'HazardCategory' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Hazard category, when the observation identifies a hazard (e.g., Slip/Trip, Ergonomic, PPE).', @level0type=N'SCHEMA', @level0name=N'SafetyEvents', @level1type=N'TABLE', @level1name=N'Observation', @level2type=N'COLUMN', @level2name=N'HazardCategory';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Hazard category, when the observation identifies a hazard (e.g., Slip/Trip, Ergonomic, PPE).', @level0type=N'SCHEMA', @level0name=N'SafetyEvents', @level1type=N'TABLE', @level1name=N'Observation', @level2type=N'COLUMN', @level2name=N'HazardCategory';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'SafetyEvents.Observation') AND c.name = N'Location' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Physical location the observation was made at.', @level0type=N'SCHEMA', @level0name=N'SafetyEvents', @level1type=N'TABLE', @level1name=N'Observation', @level2type=N'COLUMN', @level2name=N'Location';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Physical location the observation was made at.', @level0type=N'SCHEMA', @level0name=N'SafetyEvents', @level1type=N'TABLE', @level1name=N'Observation', @level2type=N'COLUMN', @level2name=N'Location';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'SafetyEvents.Observation') AND c.name = N'Description' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Narrative description of what was observed.', @level0type=N'SCHEMA', @level0name=N'SafetyEvents', @level1type=N'TABLE', @level1name=N'Observation', @level2type=N'COLUMN', @level2name=N'Description';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Narrative description of what was observed.', @level0type=N'SCHEMA', @level0name=N'SafetyEvents', @level1type=N'TABLE', @level1name=N'Observation', @level2type=N'COLUMN', @level2name=N'Description';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'SafetyEvents.Observation') AND c.name = N'CorrectiveActionRequired' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Indicates whether corrective action is required as a result of this observation.', @level0type=N'SCHEMA', @level0name=N'SafetyEvents', @level1type=N'TABLE', @level1name=N'Observation', @level2type=N'COLUMN', @level2name=N'CorrectiveActionRequired';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Indicates whether corrective action is required as a result of this observation.', @level0type=N'SCHEMA', @level0name=N'SafetyEvents', @level1type=N'TABLE', @level1name=N'Observation', @level2type=N'COLUMN', @level2name=N'CorrectiveActionRequired';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'SafetyEvents.Observation') AND c.name = N'FollowUpDueAtUtc' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'UTC timestamp any required follow-up is due.', @level0type=N'SCHEMA', @level0name=N'SafetyEvents', @level1type=N'TABLE', @level1name=N'Observation', @level2type=N'COLUMN', @level2name=N'FollowUpDueAtUtc';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'UTC timestamp any required follow-up is due.', @level0type=N'SCHEMA', @level0name=N'SafetyEvents', @level1type=N'TABLE', @level1name=N'Observation', @level2type=N'COLUMN', @level2name=N'FollowUpDueAtUtc';
GO

------------------------------------------------------------
-- Verification
------------------------------------------------------------
SELECT s.name AS SchemaName, t.name AS TableName, COUNT(ep.name) AS DescCount
FROM sys.tables t
JOIN sys.schemas s ON t.schema_id = s.schema_id
LEFT JOIN sys.extended_properties ep ON ep.major_id = t.object_id AND ep.name = N'MS_Description'
WHERE s.name IN ('Analytics','Compliance','Operational','SafetyEvents')
GROUP BY s.name, t.name
ORDER BY SchemaName, TableName;
GO


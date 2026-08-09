// ============================================================================
//  Project:        PlantSafety.UI
//  Module:         Routes
//  File:           AppRoutes.cs
//  Created:        2026-08-09
//  Description:    Centralized, strongly typed route constants for every
//                  module (Section 8). Deliberately consolidated into one
//                  file with one static class per module rather than 20
//                  separate "<ModuleName>Routes.cs" files -- a documented,
//                  scale-driven deviation from the literal one-file-per-
//                  module convention given this program's module count;
//                  the per-module static-class structure and constant
//                  naming/shape are otherwise unchanged.
// ============================================================================
namespace PlantSafety.UI.Routes;

/// <summary>Route constants for the Facility module.</summary>
public static class FacilityRoutes
{
    public const string List = "/facility/list";
    public const string Detail = "/facility/detail/{Id}";
    public const string Create = "/facility/create";
}

/// <summary>Route constants for the Employee module.</summary>
public static class EmployeeRoutes
{
    public const string List = "/employee/list";
    public const string Detail = "/employee/detail/{Id}";
    public const string Create = "/employee/create";
}

/// <summary>Route constants for the Machine module.</summary>
public static class MachineRoutes
{
    public const string List = "/machine/list";
    public const string Detail = "/machine/detail/{Id}";
    public const string Create = "/machine/create";
}

/// <summary>Route constants for the Workflow Engine module.</summary>
public static class WorkflowEngineRoutes
{
    public const string Evaluate = "/workflow-engine/evaluate";
}

/// <summary>Route constants for the Predictive Models module.</summary>
public static class PredictiveModelsRoutes
{
    public const string Console = "/predictive-models/console";
}

/// <summary>Route constants for the Data Lake module.</summary>
public static class DataLakeRoutes
{
    public const string Dashboard = "/datalake/dashboard";
}

/// <summary>Route constants for the Enterprise Graph module.</summary>
public static class EnterpriseGraphRoutes
{
    public const string Console = "/enterprise-graph/console";
}

/// <summary>Route constants for the Enterprise Dashboard module.</summary>
public static class EnterpriseDashboardRoutes
{
    public const string Dashboard = "/enterprise-dashboard/dashboard";
}

/// <summary>Route constants for the Rules Engine module.</summary>
public static class RulesEngineRoutes
{
    public const string Console = "/rules-engine/console";
}

/// <summary>Route constants for the Workflow Orchestration module.</summary>
public static class WorkflowOrchestrationRoutes
{
    public const string Console = "/workflow-orchestration/console";
}

/// <summary>Route constants for the Digital Twin module.</summary>
public static class DigitalTwinRoutes
{
    public const string Console = "/digital-twin/console";
}

/// <summary>Route constants for the Predictive Modeling module.</summary>
public static class PredictiveModelingRoutes
{
    public const string Console = "/predictive-modeling/console";
}

/// <summary>Route constants for the Integration module.</summary>
public static class IntegrationRoutes
{
    public const string Console = "/integration/console";
}

/// <summary>Route constants for the Tenant Facility Registry module.</summary>
public static class TenantFacilityRegistryRoutes
{
    public const string List = "/tenant-facility-registry/list";
}

/// <summary>Route constants for the AI Reasoning module.</summary>
public static class AiReasoningRoutes
{
    public const string Console = "/ai-reasoning/console";
}

/// <summary>Route constants for the AI Hazard Understanding module.</summary>
public static class AiHazardUnderstandingRoutes
{
    public const string Console = "/ai-hazard-understanding/console";
}

/// <summary>Route constants for the AI Workflow Automation module.</summary>
public static class AiWorkflowAutomationRoutes
{
    public const string Console = "/ai-workflow-automation/console";
}

/// <summary>Route constants for the AI Regulatory Interpretation module.</summary>
public static class AiRegulatoryInterpretationRoutes
{
    public const string Console = "/ai-regulatory-interpretation/console";
}

/// <summary>Route constants for the AI Severity Assessment module.</summary>
public static class AiSeverityAssessmentRoutes
{
    public const string Console = "/ai-severity-assessment/console";
}

/// <summary>Route constants for the AI Predictive Optimization module.</summary>
public static class AiPredictiveOptimizationRoutes
{
    public const string Console = "/ai-predictive-optimization/console";
}

/// <summary>Route constants for the AI Digital Twin Learning module.</summary>
public static class AiDigitalTwinLearningRoutes
{
    public const string Console = "/ai-digital-twin-learning/console";
}

/// <summary>Route constants for the AI Safety Copilot module.</summary>
public static class AiSafetyCopilotRoutes
{
    public const string Dashboard = "/ai-safety-copilot/dashboard";
}

/// <summary>Route constants for the AI Enterprise Orchestration module.</summary>
public static class AiEnterpriseOrchestrationRoutes
{
    public const string Console = "/ai-enterprise-orchestration/console";
}

/// <summary>Route constants for the home/landing page.</summary>
public static class HomeRoutes
{
    public const string Home = "/";
}

namespace PlantClientInstaller.Core.Logs;

public sealed class LogPolicy
{
    public bool IncludeTrace { get; }
    public bool IncludeContext { get; }
    public bool IncludeExceptionDetails { get; }

    public LogPolicy(bool includeTrace = false, bool includeContext = true, bool includeExceptionDetails = true)
    {
        IncludeTrace = includeTrace;
        IncludeContext = includeContext;
        IncludeExceptionDetails = includeExceptionDetails;
    }
}

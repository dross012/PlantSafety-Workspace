namespace PlantSafety.Integration.Pusher.Abstractions;

/// <summary>
/// Pushes a source module's data into its PlantSafety-side integration schema.
/// Deliberately a direct, synchronous, call-and-log abstraction -- NOT an event-bus publisher.
/// No message bus, event envelope, or routing-key dispatcher exists anywhere in this program
/// (see EmergencyMgt-Integration/README.md's "Build-repair note"); this interface is the
/// honest, buildable substitute: a scheduled or manually-invoked job that reads the source
/// module's database directly and calls the target integration schema's own
/// usp_*_Upsert/usp_*_Record procedures (already deployed, already proven live via sqlcmd).
/// </summary>
public interface IIntegrationPusher
{
    /// <summary>Runs one full push cycle: reads every feed from the source, upserts each into the target integration schema, and returns the outcome.</summary>
    Task<IIntegrationPushResult> PushAsync(CancellationToken cancellationToken);
}

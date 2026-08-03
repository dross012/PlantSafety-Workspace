namespace PlantClientInstaller.Core.Logs;

public static class LogExtensions
{
    public static void LogInstallerStart(this ILogWriter writer, string installerId, string machine) =>
        writer.Info($"Installer started", new LogContext(installerId, machine));

    public static void LogInstallerComplete(this ILogWriter writer, string installerId, string machine) =>
        writer.Info($"Installer completed", new LogContext(installerId, machine));

    public static void LogInstallerError(this ILogWriter writer, string installerId, string machine, Exception ex) =>
        writer.Error($"Installer error", ex, new LogContext(installerId, machine));
}

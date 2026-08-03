namespace PlantClientInstaller.Bootstrapper.Bootstrapper
{
    public static class CommandLineParser
    {
        public static string Parse(string[] args)
        {
            if (args.Length == 0) return "install";
            return args[0].ToLowerInvariant();
        }
    }
}

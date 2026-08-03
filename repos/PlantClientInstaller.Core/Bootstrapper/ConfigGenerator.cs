using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PlantClientInstaller.Core.Bootstrapper
{
    /// <summary>
    /// Generates deterministic configuration files based on template inputs,
    /// installer context, and enterprise architectural rules.
    /// </summary>
    public sealed class ConfigGenerator(InstallerContext context, ILogger logger)
    {
        private readonly InstallerContext _context = context ?? throw new ArgumentNullException(nameof(context));
        private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        /// <summary>
        /// Executes the full configuration generation pipeline.
        /// </summary>
        public void Run()
        {
            _logger.Info("Starting configuration generation pipeline.");

            ValidatePaths();
            var templates = LoadTemplates();
            var resolvedConfigs = ResolveTokens(templates);
            WriteConfigs(resolvedConfigs);

            _logger.Info("Configuration generation pipeline completed successfully.");
        }

        // ------------------------------------------------------------
        // 1. VALIDATION
        // ------------------------------------------------------------

        private void ValidatePaths()
        {
            _logger.Info("Validating configuration paths.");

            if (!Directory.Exists(_context.ConfigTemplatePath))
            {
                throw new DirectoryNotFoundException(
                    $"Config template directory not found: {_context.ConfigTemplatePath}");
            }

            if (!Directory.Exists(_context.OutputConfigPath))
            {
                Directory.CreateDirectory(_context.OutputConfigPath);
                _logger.Info($"Created configuration output directory: {_context.OutputConfigPath}");
            }
        }

        // ------------------------------------------------------------
        // 2. LOAD TEMPLATES
        // ------------------------------------------------------------

        private Dictionary<string, string> LoadTemplates()
        {
            _logger.Info("Loading configuration templates.");

            var templates = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var file in Directory.GetFiles(_context.ConfigTemplatePath, "*.template.json"))
            {
                var name = Path.GetFileNameWithoutExtension(file).Replace(".template", "");
                var content = File.ReadAllText(file);

                templates[name] = content;
                _logger.Info($"Loaded template: {name}");
            }

            if (templates.Count == 0)
            {
                throw new InvalidOperationException("No configuration templates found.");
            }

            return templates;
        }

        // ------------------------------------------------------------
        // 3. TOKEN RESOLUTION
        // ------------------------------------------------------------

        private Dictionary<string, string> ResolveTokens(Dictionary<string, string> templates)
        {
            _logger.Info("Resolving configuration tokens.");

            var resolved = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var kvp in templates)
            {
                var output = kvp.Value;

                foreach (var token in _context.Tokens)
                {
                    var placeholder = $"{{{{{token.Key}}}}}";
                    output = output.Replace(placeholder, token.Value, StringComparison.OrdinalIgnoreCase);
                }

                resolved[kvp.Key] = output;
                _logger.Info($"Resolved tokens for: {kvp.Key}");
            }

            return resolved;
        }

        // ------------------------------------------------------------
        // 4. WRITE FINAL CONFIG FILES
        // ------------------------------------------------------------

        private void WriteConfigs(Dictionary<string, string> resolvedConfigs)
        {
            _logger.Info("Writing final configuration files.");

            foreach (var kvp in resolvedConfigs)
            {
                var outputFile = Path.Combine(_context.OutputConfigPath, $"{kvp.Key}.json");

                File.WriteAllText(outputFile, kvp.Value);
                _logger.Info($"Wrote configuration file: {outputFile}");
            }
        }
    }

    // ------------------------------------------------------------
    // SUPPORTING TYPES
    // ------------------------------------------------------------

    // ILogger is defined here for the Core project and used across the
    // bootstrapper components. It exposes common log levels used by the
    // various pipeline classes.
    public interface ILogger
    {
        void Info(string message);
        void Warning(string message);
        void Error(string message, Exception? ex = null);
        void Fatal(string message, Exception? ex = null);
    }
}

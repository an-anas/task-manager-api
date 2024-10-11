using Microsoft.Extensions.Configuration;

namespace TaskManager.Common.Helpers
{
    public class ConfigurationHelper(IConfiguration configuration) : IConfigurationHelper
    {
        private IConfiguration Configuration { get; } = configuration ?? throw new ArgumentNullException(nameof(configuration));

        // Retrieves a single configuration value, prioritizing environment variables
        public string GetConfigValue(string configKey)
        {
            // Replace colons with double underscores for environment variable lookup
            var envVarKey = configKey.Replace(":", "__");

            // Check environment variable first, then fall back to appsettings.json
            var value = Environment.GetEnvironmentVariable(envVarKey) ?? Configuration[configKey];

            if (string.IsNullOrEmpty(value))
            {
                throw new InvalidOperationException($"Configuration value for '{configKey}' is missing.");
            }

            return value;
        }
    }
}
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace KanbanRestService.Helpers
{
    public static class JwtKeyProvider
    {
        public static string GetKey(IConfiguration config, IHostEnvironment environment)
        {
            if (environment.IsEnvironment("IntegrationTest") || environment.IsEnvironment("UnitTest"))
            {
                var testingKey = config["TestingJwtKey"];
                _throwExceptionIfStringIsNull(testingKey, environment.EnvironmentName);
                return testingKey;
            }

            var key = config["SuperSecretJwtKey"];

            _throwExceptionIfStringIsNull(key, environment.EnvironmentName);

            return key;
        }

        private static void _throwExceptionIfStringIsNull(string? key, string environmentName)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new InvalidOperationException($"JwtKeyProvider: key is missing in config for environment '{environmentName}'.");
            }
        }
    }
}

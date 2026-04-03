namespace KanbanRestService.Helpers
{
    public static class JwtKeyProvider
    {
        public static string GetKey(IConfiguration config, IHostEnvironment environment)
        {
            if (environment.IsEnvironment("IntegrationTest") || environment.IsEnvironment("UnitTest"))
            {
                return "TEST_KEY_1234567890123456789012345";
            }

            var key = config["SuperSecretJwtKey"];          

            if (string.IsNullOrWhiteSpace(key))
            {
                throw new InvalidOperationException("SuperSecretJwtKey is missing.");
            }
            
            return key;
        }
    }
}

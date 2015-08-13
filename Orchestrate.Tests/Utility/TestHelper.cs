using System;

public static class TestHelper
{
    public static string ApiKey
    {
        get
        {
            return Environment.GetEnvironmentVariable("OrchestrateApiKey");
        }
    }
}
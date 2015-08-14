using System;

public static class TestUtility
{
    public static string ApplicationKey
    {
        get
        {
            return Environment.GetEnvironmentVariable("OrchestrateApiKey");
        }
    }
}
using System;

namespace Orchestrate.Io
{
    public static class EnvironmentHelper
    {
        public static string ApiKey(string envVariable)
        {
            string result = Environment.GetEnvironmentVariable(envVariable);
            if (result == null)
                return string.Empty;

            return result;
        }
    }
}

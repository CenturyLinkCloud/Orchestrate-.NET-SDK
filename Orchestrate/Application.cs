using System;

namespace Orchestrate.Io
{
    public class Application : IApplication
    {
        string applicationKey;
        string host;  

        public Application(string applicationKey, string host = "https://api.orchestrate.io/")
        {
            this.applicationKey = applicationKey;
            this.host = host;
        }

        public string Key
        {
            get
            {
                string result = Environment.GetEnvironmentVariable(applicationKey);
                if (result == null)
                    return string.Empty;

                return result;
            }
        }

        public string HostUrl
        {
            get { return host + "v0/"; }
        }
    }
}

using System;

namespace Orchestrate.Io
{
    public class Application : IApplication
    {
        string applicationKey;
        string apiUrl;  

        public Application(string applicationKey, string apiUrl = "https://api.orchestrate.io/")
        {
            this.applicationKey = applicationKey;
            this.apiUrl = apiUrl;
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

        public string V0ApiUrl
        {
            get { return apiUrl + "v0/"; }
        }
    }
}

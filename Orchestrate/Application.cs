using System;

namespace Orchestrate.Io
{
    /// <summary>
    /// This is a utility class provided as an implementation of the 
    /// <see cref="IApplication"/> interface that looks for the Orchestrate 
    /// Application Key in a specified environment variable. It also provides 
    /// a default URL for the Orchestrate API. 
    /// </summary>
    public class Application : IApplication
    {
        string envVariable;
        string host;  

        /// <summary>
        /// This class is constructed with an environment variable and  thedefault API URL. 
        /// You can optionally provide a differenet URL which allows access to Orchestrate in 
        /// mutiple data centers. 
        /// </summary>
        /// <param name="emvironmentVariable">The environment variable that holds the Orchestrate Application Key</param>
        /// <param name="host">The API Host URL</param>
        public Application(string envVariable, string host = "https://api.orchestrate.io/")
        {
            this.envVariable = envVariable;
            this.host = host;
        }

        /// <summary>
        /// The Key will lookup the passed in named environment variable for the 
        /// Orchestrate API key. If the environment variable is not present 
        /// it will return null. 
        /// </summary>
        public string Key
        {
            get
            {
                string result = Environment.GetEnvironmentVariable(envVariable);
                if (result == null)
                    return string.Empty;

                return result;
            }
        }

        /// <inheritdoc/>
        public string HostUrl
        {
            get { return host + "v0/"; }
        }
    }
}

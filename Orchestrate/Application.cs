namespace Orchestrate.Io
{
    /// <summary>
    /// This is a utility class provided as an implementation of the 
    /// <see cref="IApplication"/> interface that associates the API Key with the 
    /// specific URL for the data center that hosts your database. 
    /// </summary>
    public class Application : IApplication
    {
        /// <summary>
        /// This is the API key that you retrieve from the Orchestrate Website for your specific application. 
        /// </summary>
        public string Key { get; private set; }

        string host;  

        /// <summary>
        /// This class is constructed with an environment variable and  thedefault API URL. 
        /// You can optionally provide a differenet URL which allows access to Orchestrate in 
        /// mutiple data centers. 
        /// </summary>
        /// <param name="apiKey">The environment variable that holds the Orchestrate Application Key</param>
        /// <param name="host">The API Host URL</param>
        public Application(string apiKey, string host = "https://api.orchestrate.io/")
        {
            Key = apiKey;
            this.host = host;
        }

        /// <inheritdoc/>
        public string HostUrl
        {
            get { return host + "v0/"; }
        }
    }
}

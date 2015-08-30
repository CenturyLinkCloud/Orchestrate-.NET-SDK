namespace Orchestrate.Io
{
    /// <summary>
    /// The IApplication interface is used by the Client class to provide access to
    /// API Key and the URL of the Orchestrate Service. 
    /// </summary>
    public interface IApplication
    {
        /// <summary>
        /// The string representing the API Key. You get this when you define an
        /// application on the Orchestrate website. 
        /// </summary>
        string Key { get; }
        
        /// <summary>
        /// The URL that will be used to communicate from your application to the 
        /// Orchestrate service. 
        /// </summary>
        string HostUrl { get; }
    }
}
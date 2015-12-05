using System.Net.Http;
using System.Reflection;
using Orchestrate.Io;
using Xunit;

public class HttpMessageExtensionsTests
{
    [Fact]
    public void UserAgent()
    {
        HttpRequestMessage message = new HttpRequestMessage();
        message.AddUserAgent();

        var productVersion = typeof(KvMetadata).GetTypeInfo().Assembly.GetName().Version;
        string expectedUserAgent = string.Format("OrchestrateDotNetClient/{0}", productVersion);
        Assert.Contains(expectedUserAgent, message.Headers.UserAgent.ToString());
    }
}

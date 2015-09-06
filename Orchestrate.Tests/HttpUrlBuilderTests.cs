using System;
using Orchestrate.Io;
using Xunit;

public class HttpUrlBuilderTests
{
    public class Constructor
    {
        [Fact]
        public void InitializesUrlFromString()
        {
            var builder = new HttpUrlBuilder("http://localhost/api/foo");

            Assert.Equal(new Uri("http://localhost/api/foo"), builder.ToUri());
        }

        [Fact]
        public void InitializesUrlFromRelativeString()
        {
            var builder = new HttpUrlBuilder("/api/foo", UriKind.Relative);

            Assert.Equal(new Uri("/api/foo", UriKind.Relative), builder.ToUri());
        }

        [Fact]
        public void InitializesUrlFromAbsoluteUri()
        {
            var builder = new HttpUrlBuilder(new Uri("http://localhost/api/foo", UriKind.Absolute));

            Assert.Equal(new Uri("http://localhost/api/foo", UriKind.Absolute), builder.ToUri());
        }

        [Fact]
        public void InitializesUrlFromRelativeUri()
        {
            var builder = new HttpUrlBuilder(new Uri("/api/foo", UriKind.Relative));

            Assert.Equal(new Uri("/api/foo", UriKind.Relative), builder.ToUri());
        }

        [Fact]
        public void InitializesUrlFromComponents()
        {
            var builder = new HttpUrlBuilder("localhost", port: 1234, scheme: "https");

            Assert.Equal(new Uri("https://localhost:1234"), builder.ToUri());
        }
    }

    public class AddRawQuery
    {
        [Fact]
        public void AddsValueToUrl()
        {
            var builder = new HttpUrlBuilder("http://localhost");

            builder.AddRawQuery("name=value");

            Assert.Equal("?name=value", builder.ToUri().Query);
        }
    }

    public class AddQuery
    {
        [Fact]
        public void AddsValueToUrl()
        {
            var builder = new HttpUrlBuilder("http://localhost");

            builder.AddQuery("name", "value");

            Assert.Equal("?name=value", builder.ToUri().Query);
        }

        [Fact]
        public void AddsMultipleValuesWithSameNameToUrl()
        {
            var builder = new HttpUrlBuilder("http://localhost");

            builder.AddQuery("name", "value1");
            builder.AddQuery("name", "value2");

            Assert.Equal("?name=value1&name=value2", builder.ToUri().Query);
        }

        [Fact]
        public void WhenInitialUrlHasQueryString_AppendsQueryToExisting()
        {
            var builder = new HttpUrlBuilder("http://localhost?name1=value1");

            builder.AddQuery("name2", "value2");

            Assert.Equal("?name1=value1&name2=value2", builder.ToUri().Query);
        }

        [Fact]
        public void EncodesName()
        {
            var builder = new HttpUrlBuilder("http://localhost");

            builder.AddQuery("na/me", "value");

            Assert.Equal("?na%2fme=value", builder.ToUri().Query);
        }

        [Fact]
        public void EncodesValue()
        {
            var builder = new HttpUrlBuilder("http://localhost");

            builder.AddQuery("name", "va/lue");

            Assert.Equal("?name=va%2flue", builder.ToUri().Query);
        }
    }

    public class AppendPath
    {
        [Theory]
        [InlineData("http://localhost/existing/path")]
        [InlineData("http://localhost/existing/path/")]
        public void AppendsPathToExisting(string url)
        {
            var builder = new HttpUrlBuilder(url);

            builder.AppendPath("foo/bar");

            Assert.Equal("/existing/path/foo/bar", builder.ToUri().AbsolutePath);
        }

        [Theory]
        [InlineData("http://localhost")]
        [InlineData("http://localhost/")]
        public void WhenNoExistingPath_SetsAbsolutePath(string url)
        {
            var builder = new HttpUrlBuilder(url);

            builder.AppendPath("foo/bar");

            Assert.Equal("/foo/bar", builder.ToUri().AbsolutePath);
        }

        [Theory]
        [InlineData("foo#bar", "/foo%23bar")]
        [InlineData("foo?bar", "/foo%3Fbar")]
        public void EncodesPath(string path, string expectedPath)
        {
            var builder = new HttpUrlBuilder("http://localhost");

            builder.AppendPath(path);

            Assert.Equal(expectedPath, builder.ToUri().AbsolutePath);
        }

        [Fact]
        public void EncodesPathArguments()
        {
            var builder = new HttpUrlBuilder("http://localhost");

            builder.AppendPath("foo/{0}", "bar#bar");

            Assert.Equal("/foo/bar%23bar", builder.ToUri().AbsolutePath);
        }

        [Fact]
        public void DoesNotEncodeThePathSeperator()
        {
            var builder = new HttpUrlBuilder("http://localhost");

            builder.AppendPath("foo/bar");

            Assert.Equal("/foo/bar", builder.ToUri().AbsolutePath);
        }

        [Fact]
        public void FormatsPath()
        {
            var builder = new HttpUrlBuilder("http://localhost");

            builder.AppendPath("foo/{0}", "bar");

            Assert.Equal("/foo/bar", builder.ToUri().AbsolutePath);
        }
    }

    public class AppendFragment
    {
        [Theory]
        [InlineData("http://localhost#/existing/path")]
        [InlineData("http://localhost#/existing/path/")]
        public void AppendsFragmentToExisting(string url)
        {
            var builder = new HttpUrlBuilder(url);

            builder.AppendFragment("foo/bar");

            Assert.Equal("#/existing/path/foo/bar", builder.ToUri().Fragment);
        }

        [Theory]
        [InlineData("http://localhost")]
        [InlineData("http://localhost/")]
        public void WhenNoExistingFragment_SetsFragment(string url)
        {
            var builder = new HttpUrlBuilder(url);

            builder.AppendFragment("foo/bar");

            Assert.Equal("#/foo/bar", builder.ToUri().Fragment);
        }

        [Theory]
        [InlineData("foo#bar", "#/foo%23bar")]
        public void EncodesFragment(string path, string expectedPath)
        {
            var builder = new HttpUrlBuilder("http://localhost");

            builder.AppendFragment(path);

            Assert.Equal(expectedPath, builder.ToUri().Fragment);
        }

        [Fact]
        public void EncodesFragmentArguments()
        {
            var builder = new HttpUrlBuilder("http://localhost");

            builder.AppendFragment("foo/{0}", "bar#bar");

            Assert.Equal("#/foo/bar%23bar", builder.ToUri().Fragment);
        }

        [Fact]
        public void DoesNotEncodeThePathSeperator()
        {
            var builder = new HttpUrlBuilder("http://localhost");

            builder.AppendFragment("foo/bar");

            Assert.Equal("#/foo/bar", builder.ToUri().Fragment);
        }

        [Fact]
        public void FormatsFragment()
        {
            var builder = new HttpUrlBuilder("http://localhost");

            builder.AppendFragment("foo/{0}", "bar");

            Assert.Equal("#/foo/bar", builder.ToUri().Fragment);
        }
    }

    public class WithHost
    {
        [Fact]
        public void ChangesHostName()
        {
            var builder = new HttpUrlBuilder("http://host1:1337/path");

            builder.WithHost("host2");

            Assert.Equal("http://host2:1337/path", builder.ToString());
        }
    }

    public class WithPath
    {
        [Theory]
        [InlineData("http://localhost")]
        [InlineData("http://localhost/")]
        public void SetsAbsolutePath(string url)
        {
            var builder = new HttpUrlBuilder(url);

            builder.WithPath("foo/bar");

            Assert.Equal("/foo/bar", builder.ToUri().AbsolutePath);
        }

        [Theory]
        [InlineData("http://localhost/existing/path")]
        [InlineData("http://localhost/existing/path/")]
        public void OverwritesExistingPath(string url)
        {
            var builder = new HttpUrlBuilder(url);

            builder.WithPath("foo/bar");

            Assert.Equal("/foo/bar", builder.ToUri().AbsolutePath);
        }

        [Theory]
        [InlineData("foo#bar", "/foo%23bar")]
        [InlineData("foo?bar", "/foo%3Fbar")]
        public void EncodesPath(string path, string expectedPath)
        {
            var builder = new HttpUrlBuilder("http://localhost");

            builder.WithPath(path);

            Assert.Equal(expectedPath, builder.ToUri().AbsolutePath);
        }

        [Fact]
        public void EncodesPathArguments()
        {
            var builder = new HttpUrlBuilder("http://localhost");

            builder.WithPath("foo/{0}", "bar#bar");

            Assert.Equal("/foo/bar%23bar", builder.ToUri().AbsolutePath);
        }

        [Fact]
        public void DoesNotEncodeThePathSeperator()
        {
            var builder = new HttpUrlBuilder("http://localhost");

            builder.WithPath("foo/bar");

            Assert.Equal("/foo/bar", builder.ToUri().AbsolutePath);
        }

        [Fact]
        public void FormatsPath()
        {
            var builder = new HttpUrlBuilder("http://localhost");

            builder.WithPath("foo/{0}", "bar");

            Assert.Equal("/foo/bar", builder.ToUri().AbsolutePath);
        }
    }

    public class WithPort
    {
        [Fact]
        public void ChangesPost()
        {
            var builder = new HttpUrlBuilder("http://host1:1337/path");

            builder.WithPort(2112);

            Assert.Equal("http://host1:2112/path", builder.ToString());
        }
    }

    public class WithScheme
    {
        [Fact]
        public void ChangesScheme()
        {
            var builder = new HttpUrlBuilder("http://host1:1337/path");

            builder.WithScheme("https");

            Assert.Equal("https://host1:1337/path", builder.ToString());
        }
    }

    public class ToStringTests
    {
        [Fact]
        public void ConvertsToString()
        {
            var builder = new HttpUrlBuilder("http://localhost/api/foo");

            Assert.Equal("http://localhost/api/foo", builder.ToString());
        }

        [Fact]
        public void ConvertsRelativeUrlToString()
        {
            var builder = new HttpUrlBuilder("/api/foo", UriKind.Relative);

            Assert.Equal("/api/foo", builder.ToString());
        }

        [Fact]
        public void ConvertsAbsoluteUrlToString()
        {
            var builder = new HttpUrlBuilder("http://localhost/api/foo", UriKind.Absolute);

            Assert.Equal("http://localhost/api/foo", builder.ToString());
        }
    }

    public class ToUri
    {
        [Fact]
        public void ConvertsToUri()
        {
            var builder = new HttpUrlBuilder("http://localhost/api/foo");

            Assert.Equal(new Uri("http://localhost/api/foo"), builder.ToUri());
        }

        [Fact]
        public void ConvertsRelativeToUri()
        {
            var builder = new HttpUrlBuilder("/api/foo", UriKind.Relative);

            Assert.Equal(new Uri("/api/foo", UriKind.Relative), builder.ToUri());
        }

        [Fact]
        public void ConvertsAbsoluteToUri()
        {
            var builder = new HttpUrlBuilder("http://localhost/api/foo", UriKind.Absolute);

            Assert.Equal(new Uri("http://localhost/api/foo", UriKind.Absolute), builder.ToUri());
        }
    }
}
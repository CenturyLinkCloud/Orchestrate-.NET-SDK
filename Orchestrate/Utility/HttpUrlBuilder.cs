using System;
using System.Collections.Generic;
using System.Web;

namespace Orchestrate.Io
{
    public class HttpUrlBuilder
    {
        readonly UriBuilder builder;
        readonly bool isRelative;

        public HttpUrlBuilder(string baseUrl)
            : this(baseUrl, UriKind.RelativeOrAbsolute)
        {}

        public HttpUrlBuilder(string baseUrl, UriKind kind)
            : this(new Uri(baseUrl, kind))
        {}

        public HttpUrlBuilder(Uri baseUrl)
        {
            if (baseUrl.IsAbsoluteUri)
                builder = new UriBuilder(baseUrl);
            else
            {
                builder = new UriBuilder("http", "localhost", 80);

                string path;
                string query;
                if (!TryParseRelativeUri(baseUrl, out path, out query))
                    throw new InvalidOperationException("Unable to parse relative Uri into path and query.");

                builder.Path = path;
                builder.Query = query;
                isRelative = true;
            }
        }

        public HttpUrlBuilder(string hostname, int port = 80, string scheme = "http")
        {
            builder = new UriBuilder(scheme, hostname, port);
        }

        public string Host { get { return builder.Host; } }

        public int Port { get { return builder.Port; } }

        public string Scheme { get { return builder.Scheme; } }

        public HttpUrlBuilder AddRawQuery(string query)
        {
            if (String.IsNullOrWhiteSpace(builder.Query))
                builder.Query = query;
            else
            {
                // Stupid UriBuilder.Query returns the '?' at the beginning of the query string,
                // so we need to strip it off each time we append...
                builder.Query = String.Format("{0}&{1}", builder.Query.Remove(0, 1), query);
            }

            return this;
        }

        public HttpUrlBuilder AddQuery(string name, string value)
        {
            var query = String.Join("=", HttpUtility.UrlEncode(name), HttpUtility.UrlEncode(value));

            return AddRawQuery(query);
        }

        public HttpUrlBuilder AddQuery(IEnumerable<KeyValuePair<string, string>> keyValuePairs)
        {
            foreach (var keyValuePair in keyValuePairs)
                AddQuery(keyValuePair.Key, keyValuePair.Value);

            return this;
        }

        public HttpUrlBuilder AddQuery(IEnumerable<Tuple<string, string>> keyValuePairs)
        {
            foreach (var keyValuePair in keyValuePairs)
                AddQuery(keyValuePair.Item1, keyValuePair.Item2);

            return this;
        }

        public HttpUrlBuilder AppendFragment(string fragmentFormat, params string[] args)
        {
            var fragment = HttpUtility.UrlPathEncode(String.Format(fragmentFormat, args));
            fragment = CombinePaths(builder.Fragment, fragment);

            if (fragment.StartsWith("#"))
                fragment = fragment.Remove(0, 1);

            fragment = fragment.Replace("#", "%23");

            builder.Fragment = fragment;
            return this;
        }

        public HttpUrlBuilder AppendPath(string pathFormat, params string[] args)
        {
            var path = HttpUtility.UrlPathEncode(String.Format(pathFormat, args));
            builder.Path = CombinePaths(builder.Path, path);
            return this;
        }

        static string CombinePaths(string path1, string path2)
        {
            // Normalize leading/trailing slashes
            if (path1.StartsWith("/"))
                path1 = path1.Substring(1);

            if (!path1.EndsWith("/"))
                path1 += "/";

            if (path2.StartsWith("/"))
                path2 = path2.Substring(1);

            return path1 + path2;
        }

        public HttpUrlBuilder WithHost(string host)
        {
            builder.Host = host;
            return this;
        }

        public HttpUrlBuilder WithPath(string pathFormat, params string[] args)
        {
            var path = HttpUtility.UrlPathEncode(String.Format(pathFormat, args));
            builder.Path = path;
            return this;
        }

        public HttpUrlBuilder WithPort(int port)
        {
            builder.Port = port;
            return this;
        }

        public HttpUrlBuilder WithScheme(string scheme)
        {
            builder.Scheme = scheme;
            return this;
        }

        public override string ToString()
        {
            if (isRelative)
                return builder.Uri.PathAndQuery;
            else
                return builder.Uri.ToString();
        }

        public Uri ToUri()
        {
            if (isRelative)
                return new Uri(builder.Uri.PathAndQuery, UriKind.Relative);
            else
                return builder.Uri;
        }

        static bool TryParseRelativeUri(Uri uri, out string path, out string query)
        {
            var value = uri.OriginalString;
            if (!String.IsNullOrWhiteSpace(value))
            {
                var values = value.Split('?');
                if (values.Length == 1)
                {
                    path = values[0];
                    query = null;
                    return true;
                }
                else if (values.Length == 2)
                {
                    path = values[0];
                    query = values[1];
                    return true;
                }
            }

            path = null;
            query = null;
            return false;
        }

        public static explicit operator string(HttpUrlBuilder httpUrl)
        {
            return httpUrl.ToString();
        }

        public static implicit operator Uri(HttpUrlBuilder httpUrl)
        {
            return httpUrl.ToUri();
        }
    }
}

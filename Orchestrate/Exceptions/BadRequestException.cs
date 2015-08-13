using System.Net;

namespace Orchestrate.Io
{
    public class BadRequestException : RequestException
    {
        public BadRequestException(HttpStatusCode code, string rawJson, string requestId)
            : base(code, rawJson, requestId)
        {}
    }
}

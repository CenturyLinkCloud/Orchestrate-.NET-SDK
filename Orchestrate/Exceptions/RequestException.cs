using System;
using System.Net;
using Newtonsoft.Json.Linq;

namespace Orchestrate.Io
{
    public class RequestException : ApplicationException
    {
        string rawJson;
        dynamic jsonData;

        public HttpStatusCode StatusCode { get; private set; }
        public string RequestId { get; private set; }

        public RequestException(HttpStatusCode code, string rawJson, string requestId)
        {
            StatusCode = code;
            this.rawJson = rawJson;
            RequestId = requestId;
            if (!string.IsNullOrEmpty(rawJson))
                jsonData = JObject.Parse(rawJson);
        }

        public override string Message
        {
            get
            {
                if(jsonData != null && jsonData.message != null)
                    return jsonData.message;

                return rawJson;
            }
        }
    }
}

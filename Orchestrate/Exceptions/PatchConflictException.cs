using System.Net;
using Newtonsoft.Json;

namespace Orchestrate.Io
{
    public class PatchConflictException : RequestException
    {
        public PatchConflict PatchConflict { get; private set; }

        public PatchConflictException(HttpStatusCode code, string rawJson, string requestId)
            : base(code, rawJson, requestId)
        {
            PatchConflict = JsonConvert.DeserializeObject<PatchConflict>(rawJson);
        }
    }
}

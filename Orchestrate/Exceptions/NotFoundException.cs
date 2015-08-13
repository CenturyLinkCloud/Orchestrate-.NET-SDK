using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Orchestrate.Io
{
    public class NotFoundException : RequestException
    {
        public NotFoundData NotFoundData { get; private set; }

        public NotFoundException(HttpStatusCode code, string rawJson, string requestId)
            : base(code, rawJson, requestId)
        {
            NotFoundData = JsonConvert.DeserializeObject<NotFoundData>(rawJson);
        }

        public override string Message
        {
            get
            {
                return String.Format("Key: {0} was not found in collection: {1}", 
                                     NotFoundData.Details.Items[0].Key,
                                     NotFoundData.Details.Items[0].Collection);
            }
        }
    }
}


using System.Collections.Generic;
using System.Linq;

namespace Orchestrate.Io
{
    public sealed class DataCenter
    {
        public static readonly DataCenter AmazonUsEast =
            new DataCenter("https://api.aws-us-east-1.orchestrate.io/");
        public static readonly DataCenter AmazonEUWest =
            new DataCenter("https://api.aws-eu-west-1.orchestrate.io/");
        public static readonly DataCenter CtlVA1 =
            new DataCenter("https://api.ctl-va1-a.orchestrate.io/");
        public static readonly DataCenter CtlUC1 =
            new DataCenter("https://api.ctl-uc1-a.orchestrate.io/");
        public static readonly DataCenter CTLGB3 =
            new DataCenter("https://api.ctl-gb3-a.orchestrate.io/");
        public static readonly DataCenter CtlSG1 =
            new DataCenter("https://api.ctl-sg1-a.orchestrate.io/");

        private DataCenter(string apiUrl)
        {
            ApiUrl = apiUrl;
        }

        public string ApiUrl { get; private set; }
    }
}
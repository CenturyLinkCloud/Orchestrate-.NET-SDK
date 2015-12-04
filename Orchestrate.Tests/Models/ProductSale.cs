using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Orchestrate.Tests.Models
{
    public class ProductSale
    {
        [JsonProperty("amount")]
        public Decimal Amount { get; set; }

        [JsonProperty("notes")]
        public string Notes { get; set; }
    }
}

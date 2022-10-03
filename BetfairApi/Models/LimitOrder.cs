using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BetfairApi.Models
{
    public class LimitOrder
    {
        [JsonProperty(PropertyName = "size")]
        public double Size { get; set; }

        [JsonProperty(PropertyName = "price")]
        public double Price { get; set; }

        [JsonProperty(PropertyName = "persistenceType")]
        public PersistenceType PersistenceType { get; set; }

        [JsonProperty(PropertyName = "timeInForce", NullValueHandling = NullValueHandling.Ignore)]
        public TimeInForce? TimeInForce { get; set; }
        
        [JsonProperty(PropertyName = "minFillSize", NullValueHandling = NullValueHandling.Ignore)]
        public double? MinFillSize { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder()
                        .AppendFormat("Size={0}", Size)
                        .AppendFormat(" : Price={0}", Price)
                        .AppendFormat(" : PersistenceType={0}", PersistenceType);
            if (TimeInForce != null)
            {
                sb.AppendFormat(" : TimeInForce={0}", TimeInForce)
                    .AppendFormat(" : MinFillSize={0}", MinFillSize);
            }

            return sb.ToString();
        }
    }
    
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TimeInForce
    {
        FILL_OR_KILL
    }
}

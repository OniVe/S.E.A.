using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEA.P.Web.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Command
    {
        private const char KEY_SEPARATOR = '\u23A5';

        [JsonProperty("id")]
        public uint Id { get; set; }
        [JsonProperty("value")]
        public string Value { get; set; }

        public Command( uint key, string value )
        {
            Id = key;
            Value = value;
        }
        public StringBuilder Stringify()
        {
            var value = new StringBuilder();
            return value.Append(Id).Append(KEY_SEPARATOR).Append(Value);
        }
    }
}

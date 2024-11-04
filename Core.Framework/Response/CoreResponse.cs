using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Core.Framework.Response
{
    [JsonObject]
    public class CoreResponse<T>
    {
        [JsonProperty]
        [DataMember()]
        public CoreResponseCode ResponseCode { get; set; }

        [JsonProperty]
        [DataMember()]
        public string? Message { get; set; } = null!;
        [JsonProperty]
        [DataMember()]
        public List<string>? ErrorMessages { get; set; } = null!;

        [JsonProperty]
        [DataMember()]
        public T? Data { get; set; }
    }
}

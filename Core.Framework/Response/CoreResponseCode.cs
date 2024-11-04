using Newtonsoft.Json.Converters;
using Newtonsoft.Json;


namespace Core.Framework.Response
{
    [JsonConverter(typeof(StringEnumConverter))]

    public enum CoreResponseCode
    {
        [JsonProperty]
        None,
        [JsonProperty]
        Success,
        [JsonProperty]
        Fail,
        [JsonProperty]
        NoData,
        [JsonProperty]
        Info,
        [JsonProperty]
        Unauthorized,
        [JsonProperty]
        InvalidToken
    }
}

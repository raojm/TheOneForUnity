using Newtonsoft.Json;

namespace TheOneUnity.SolanaApi.Models
{
    public class CloudFunctionResult<T>
    {
        [JsonProperty("result")]
        public T Result { get; set; }
    }
}

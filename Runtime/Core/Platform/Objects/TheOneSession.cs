

namespace TheOneUnity.Platform.Objects
{
    public class TheOneSession : TheOneObject
    {
        public TheOneSession() : base("_Session") { }

       	// [JsonProperty("sessionToken")]
        public new string sessionToken { get; set; }
    }
}

using TheOneUnity.Platform;
using TheOneUnity.Platform.Utilities;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using TheOneUnity.Platform.Abstractions;
using TheOneUnity.Platform.Objects;
using TheOneUnity.Platform.Services;

namespace TheOneUnity.Platform
{
    public class TheOneServiceHub : ServiceHub<TheOneUser>
    {
        public TheOneServiceHub (HttpClient httpClient, IServerConnectionData connectionData, IJsonSerializer jsonSerializer) : base(connectionData, jsonSerializer, httpClient) { }
    }
}

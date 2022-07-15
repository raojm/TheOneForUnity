using System;
using System.Collections.Generic;
using System.Threading;
using TheOneUnity.Platform.Utilities;
using Cysharp.Threading.Tasks;
using System.Net;
using TheOneUnity.Platform.Abstractions;
using TheOneUnity.Platform.Services.Models;

namespace TheOneUnity.Platform.Services.ClientServices
{
    /// <summary>
    /// This service enables an application to call TheOne Cloud Functions.
    /// </summary>
    public class TheOneCloudFunctionService : ICloudFunctionService
    {
        ITheOneCommandRunner CommandRunner { get; }

        IServerConnectionData ServerConnectionData { get; }

        IJsonSerializer JsonSerializer { get; }

        public TheOneCloudFunctionService(ITheOneCommandRunner commandRunner, IServerConnectionData serverConnectionData, IJsonSerializer jsonSerializer) => (CommandRunner, ServerConnectionData, JsonSerializer) = (commandRunner, serverConnectionData, jsonSerializer);

        /// <summary>
        /// Calls TheOne cloud function specified by 'name'.
        /// </summary>
        /// <typeparam name="T">Expected result type</typeparam>
        /// <param name="name">Name of cloud function</param>
        /// <param name="parameters">Parameters that will be passed to the cloud function</param>
        /// <param name="sessionToken">current user's session token</param>
        /// <param name="cancellationToken">Threading cancellation token</param>
        /// <returns>T - result of cloud function call.</returns>
        public async UniTask<T> CallFunctionAsync<T>(string name, IDictionary<string, object> parameters, string sessionToken, System.Threading.CancellationToken cancellationToken = default)
        {
            TheOneCommand command = new TheOneCommand($"server/functions/{Uri.EscapeDataString(name)}", method: "POST", sessionToken: sessionToken, data: parameters is { } && parameters.Count > 0 ? JsonSerializer.Serialize(parameters) : "{}");

            Tuple<HttpStatusCode, string> cmdResult = await CommandRunner.RunCommandAsync(command, cancellationToken: cancellationToken);
            
            T resp = default;

            if ((int)cmdResult.Item1 < 400)
            {
                resp = (T)JsonSerializer.Deserialize<T>(cmdResult.Item2);
            }

            return resp;
         
        }
    }
}

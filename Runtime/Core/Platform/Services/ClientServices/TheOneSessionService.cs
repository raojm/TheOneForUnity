using System.Threading;
using TheOneUnity.Platform.Utilities;
using Cysharp.Threading.Tasks;
using System.Net;
using System;
using TheOneUnity.Platform.Abstractions;
using TheOneUnity.Platform.Objects;
using TheOneUnity.Platform.Services.Models;

namespace TheOneUnity.Platform.Services.ClientServices
{
    public class TheOneSessionService<TUser> : ISessionService<TUser> where TUser : TheOneUser
    {
        ITheOneCommandRunner CommandRunner { get; }

        IJsonSerializer JsonSerializer { get; }

        public TheOneSessionService(ITheOneCommandRunner commandRunner, IJsonSerializer jsonSerializer) => (CommandRunner, JsonSerializer) = (commandRunner, jsonSerializer);

        public async UniTask<TheOneSession> GetSessionAsync(string sessionToken, IServiceHub<TUser> serviceHub, CancellationToken cancellationToken = default)
        {
            Tuple<HttpStatusCode, string> cmdResp = await CommandRunner.RunCommandAsync(new TheOneCommand("sessions/me", method: "GET", sessionToken: sessionToken, data: null), cancellationToken: cancellationToken);
         
            TheOneSession session = default;

            if ((int)cmdResp.Item1 < 300)
            {
                session = JsonSerializer.Deserialize<TheOneSession>(cmdResp.Item2);
            }

            return session;
        }

        public async UniTask RevokeAsync(string sessionToken, CancellationToken cancellationToken = default)
        {
            await CommandRunner.RunCommandAsync(new TheOneCommand("logout", method: "POST", sessionToken: sessionToken, data: "{}"), cancellationToken: cancellationToken);
        }

        public async UniTask<TheOneSession> UpgradeToRevocableSessionAsync(string sessionToken, IServiceHub<TUser> serviceHub, CancellationToken cancellationToken = default)
        {
            Tuple<HttpStatusCode, string> cmdResp = await CommandRunner.RunCommandAsync(new TheOneCommand("upgradeToRevocableSession", method: "POST", sessionToken: sessionToken, data: "{}"), cancellationToken: cancellationToken);
              
            TheOneSession session = default;

            if ((int)cmdResp.Item1 < 300)
            {
                session = JsonSerializer.Deserialize<TheOneSession>(cmdResp.Item2);
            }

            return session;
        }

        public bool IsRevocableSessionToken(string sessionToken) => sessionToken.Contains("r:");
    }
}

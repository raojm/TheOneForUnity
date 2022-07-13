using System.Collections.Generic;
using System.Threading;
using TheOneUnity.Platform.Utilities;
using Cysharp.Threading.Tasks;
using System.Net;
using System;
using TheOneUnity.Platform.Abstractions;
using TheOneUnity.Platform.Objects;
using TheOneUnity.Platform.Services.Models;
using Newtonsoft.Json;
using UnityEngine;

namespace TheOneUnity.Platform.Services.ClientServices
{
    public class TheOneUserService<TUser> : IUserService<TUser> where TUser : TheOneUser
    {
        ITheOneCommandRunner CommandRunner { get; }

        IJsonSerializer JsonSerializer { get; }

        IObjectService ObjectService { get; }

        public bool RevocableSessionEnabled { get; set; }

        public object RevocableSessionEnabledMutex { get; } = new object { };

        public TheOneUserService(ITheOneCommandRunner commandRunner, IObjectService objectService, IJsonSerializer jsonSerializer) => (CommandRunner, ObjectService, JsonSerializer) = (commandRunner, objectService, jsonSerializer);

        public async UniTask<TUser> SignUpAsync(IObjectState state, IDictionary<string, ITheOneFieldOperation> operations, CancellationToken cancellationToken = default)
        {
            Tuple<HttpStatusCode, string> cmdResp = await CommandRunner.RunCommandAsync(new TheOneCommand("server/classes/_User", method: "POST", data: JsonSerializer.Serialize(operations)), cancellationToken: cancellationToken);

            TUser resp = default;

            if ((int)cmdResp.Item1 < 300)
            {
                resp = (TUser)JsonSerializer.Deserialize<TUser>(cmdResp.Item2);

                resp.ObjectService = this.ObjectService;
            }
            else
            {
                Debug.LogError($"SignUpAsync failed: {cmdResp.Item2}");
            }

            return resp;
        }

        public async UniTask<TUser> LogInAsync(string username, string password, IServiceHub<TUser> serviceHub, CancellationToken cancellationToken = default)
        {
            TUser result = default;
            
            string signinData = JsonConvert.SerializeObject(new
            {
                username,
                password,
            });

            Tuple<HttpStatusCode, string> cmdResp =
                await CommandRunner.RunCommandAsync(
                    new TheOneCommand($"server/login", method: "POST", data: signinData), cancellationToken: cancellationToken);

            if ((int)cmdResp.Item1 < 300)
            {
                result = JsonSerializer.Deserialize<TUser>(cmdResp.Item2.ToString());

                result.ObjectService = this.ObjectService;
                result.ACL = new TheOneAcl(result);
                await result.SaveAsync();
            }
            else
            {
                Debug.LogError($"LogInAsync failed: {cmdResp.Item2}");
            }

            return result;
        }

        public async UniTask<TUser> LogInAsync(string authType, IDictionary<string, object> data, IServiceHub<TUser> serviceHub, CancellationToken cancellationToken = default)
        {
            TUser user = default;

            Dictionary<string, object> authData = new Dictionary<string, object>
            {
                [authType] = data
            };

            TheOneCommand cmd = new TheOneCommand("server/users", method: "POST", data: JsonSerializer.Serialize(new Dictionary<string, object> { [nameof(authData)] = authData }));
            Tuple<HttpStatusCode, string> cmdResp = await CommandRunner.RunCommandAsync(cmd, cancellationToken: cancellationToken);

            if ((int)cmdResp.Item1 < 300)
            {
                user = JsonSerializer.Deserialize<TUser>(cmdResp.Item2.ToString());

                user.ObjectService = this.ObjectService;

                user.ACL = new TheOneAcl(user);
                user.ethAddress = data["id"].ToString();
                user.accounts = new string[1];
                user.accounts[0] = user.ethAddress;

                await user.SaveAsync();
            }
            else
            {
                Debug.Log($"LogInAsync failed: {cmdResp.Item2}");
            }

            return user;
        }

        public async UniTask<TUser> GetUserAsync(string sessionToken, IServiceHub<TUser> serviceHub, CancellationToken cancellationToken = default)
        {
            TUser user = default;
            Tuple<HttpStatusCode, string> cmdResp = await CommandRunner.RunCommandAsync(new TheOneCommand("server/users/me", method: "GET", sessionToken: sessionToken, data: default), cancellationToken: cancellationToken);
            if ((int)cmdResp.Item1 < 300)
            {
                user = JsonSerializer.Deserialize<TUser>(cmdResp.Item2.ToString());

                user.ObjectService = this.ObjectService;
            }

            return user;
        }

        public async UniTask RequestPasswordResetAsync(string email, CancellationToken cancellationToken = default)
        {
            Tuple<HttpStatusCode, string> cmdResp = await CommandRunner.RunCommandAsync(new TheOneCommand("server/requestPasswordReset", method: "POST", data: JsonSerializer.Serialize(new Dictionary<string, object> { [nameof(email)] = email })), cancellationToken: cancellationToken);

            if((int)cmdResp.Item1 >= 400)
            {
                Debug.LogError($"RequestPasswordResetAsync failed: {cmdResp.Item2}");
            }
        }
    }
}

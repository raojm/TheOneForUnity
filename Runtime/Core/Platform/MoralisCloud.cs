using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TheOneUnity.Platform;
using TheOneUnity.Platform.Exceptions;
using TheOneUnity.Platform.Queries;
using TheOneUnity.Platform.Services;
using TheOneUnity.Platform.Services.ClientServices;
using TheOneUnity.Platform.Utilities;
using TheOneUnity.Platform.Abstractions;
using TheOneUnity.Platform.Objects;

namespace TheOneUnity.Platform
{
    public class TheOneCloud<TUser> where TUser : TheOneUser
    {
        public IServiceHub<TUser> ServiceHub;

        public TheOneCloud(IServiceHub<TUser> serviceHub) => (ServiceHub) = (serviceHub);
        public async UniTask<T> RunAsync<T>(string name, IDictionary<string, object> parameters)
        {
            TheOneUser user = await this.ServiceHub.GetCurrentUserAsync();

            T result = await this.ServiceHub.CloudFunctionService.CallFunctionAsync<T>(name, parameters, user is { } ? user.sessionToken : "");

            return result;
        }
    }
}

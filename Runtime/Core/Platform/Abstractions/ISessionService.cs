using System.Threading;
using TheOneUnity.Platform.Objects;
using Cysharp.Threading.Tasks;

namespace TheOneUnity.Platform.Abstractions
{
    public interface ISessionService<TUser> where TUser : TheOneUser
    {
        UniTask<TheOneSession> GetSessionAsync(string sessionToken, IServiceHub<TUser> serviceHub, CancellationToken cancellationToken = default);

        UniTask RevokeAsync(string sessionToken, CancellationToken cancellationToken = default);

        UniTask<TheOneSession> UpgradeToRevocableSessionAsync(string sessionToken, IServiceHub<TUser> serviceHub, CancellationToken cancellationToken = default);

        bool IsRevocableSessionToken(string sessionToken);
    }
}

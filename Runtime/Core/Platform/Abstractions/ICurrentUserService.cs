using System.Threading;
using Cysharp.Threading.Tasks;
using TheOneUnity.Platform.Objects;

namespace TheOneUnity.Platform.Abstractions
{
    public interface ICurrentUserService<TUser> : ICurrentObjectService<TUser, TUser> where TUser : TheOneUser
    {
        TUser CurrentUser { get; set; }

        UniTask<string> GetCurrentSessionTokenAsync(IServiceHub<TUser> serviceHub, CancellationToken cancellationToken = default);

        UniTask LogOutAsync(IServiceHub<TUser> serviceHub, CancellationToken cancellationToken = default);
    }
}

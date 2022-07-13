
using System.Threading;
using TheOneUnity.Platform.Utilities;
using Cysharp.Threading.Tasks;
using TheOneUnity.Platform.Abstractions;
using TheOneUnity.Platform.Objects;
using TheOneUnity.Platform.Queries;

namespace TheOneUnity.Platform.Services.ClientServices
{
    public static class SessionServiceExtensions
    {
        /// <summary>
        /// Constructs a <see cref="ParseQuery{ParseSession}"/> for ParseSession.
        /// </summary>
        public static TheOneQuery<TheOneSession> GetSessionQuery<TUser>(this IServiceHub<TUser> serviceHub) where TUser : TheOneUser => serviceHub.GetQuery<TheOneSession, TUser>();

        /// <summary>
        /// Gets the current <see cref="ParseSession"/> object related to the current user.
        /// </summary>
        public static async UniTask<TheOneSession> GetCurrentSessionAsync<TUser>(this IServiceHub<TUser> serviceHub) where TUser : TheOneUser
        {
            return await GetCurrentSessionAsync(serviceHub, CancellationToken.None);
        }

        /// <summary>
        /// Gets the current <see cref="ParseSession"/> object related to the current user.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token</param>
        public static async UniTask<TheOneSession> GetCurrentSessionAsync<TUser>(this IServiceHub<TUser> serviceHub, CancellationToken cancellationToken) where TUser : TheOneUser
        {
            TheOneSession ms = await serviceHub.GetCurrentSessionAsync<TUser>(cancellationToken);

            if (ms == null) return default;

            string token = ms.GetCurrentSessionToken();

            if (token == null) return default;

            return await serviceHub.SessionService.GetSessionAsync(token, serviceHub, cancellationToken);
        }

        public static async UniTask RevokeSessionAsync<TUser>(this IServiceHub<TUser> serviceHub, string sessionToken, CancellationToken cancellationToken) where TUser : TheOneUser
        {
            if (sessionToken != null && serviceHub.SessionService.IsRevocableSessionToken(sessionToken))
                await serviceHub.SessionService.RevokeAsync(sessionToken, cancellationToken);
        }

        public static async UniTask<string> UpgradeToRevocableSessionAsync<TUser>(this IServiceHub<TUser> serviceHub, string sessionToken, CancellationToken cancellationToken) where TUser : TheOneUser
        {
            if (sessionToken is null || serviceHub.SessionService.IsRevocableSessionToken(sessionToken))
                return sessionToken;
            else
            {
                TheOneSession ms = await serviceHub.SessionService.UpgradeToRevocableSessionAsync(sessionToken, serviceHub, cancellationToken);

                return ms.sessionToken;
            }
        }
    }
}

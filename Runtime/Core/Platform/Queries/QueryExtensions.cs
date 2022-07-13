using TheOneUnity.Platform.Abstractions;
using TheOneUnity.Platform.Objects;
using TheOneUnity.Platform.Services.ClientServices;

namespace TheOneUnity.Platform.Queries
{
    public static class QueryExtensions
    {
        public static TheOneLiveQueryClient<T> Subscribe<T>(this TheOneQuery<T> query, ILiveQueryCallbacks<T> callbacks = null) where T : TheOneObject
        {
            if (!(callbacks is { }))
            {
                callbacks = new LiveQueryCallbacks<T>();
            }

            string sessionToken = query.SessionToken;
            string installationId = query.InstallationService.InstallationId?.ToString();
            
            TheOneLiveQueryClient<T>  subscription = 
                TheOneLiveQueryManager.CreateSubscription<T>(query, query.ServerConnectionData, callbacks, query.JsonSerializer, sessionToken, installationId);

            return subscription;
        }
    }
}
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TheOneUnity.Platform.Abstractions;
using TheOneUnity.Platform.Objects;
using TheOneUnity.Platform.Queries;

namespace TheOneUnity.Platform.Services.ClientServices
{
    public class TheOneLiveQueryManager : ILiveQueryService
    {
        private List<ILiveQueryClient> clients = new List<ILiveQueryClient>();

        private static TheOneLiveQueryManager instance = new TheOneLiveQueryManager();

        private TheOneLiveQueryManager() { }
        /// <summary>
        /// Adds a live query subscription to the service
        /// </summary>
        /// <param name="client"></param>
        public void AddSubscription(ILiveQueryClient client)
        {
            client.Subscribe();
            clients.Add(client);
        }

        public void Dispose()
        {
            UnsubscribeAll();

            foreach (ILiveQueryClient c in clients)
            {
                c.Dispose();
            }
        }

        /// <summary>
        /// Returns all active subscriptions.
        /// </summary>
        /// <returns>IEnumerable<ILiveQueryClient></returns>
        public IEnumerable<ILiveQueryClient> Subscriptions()
        {
            return clients;
        }

        /// <summary>
        /// Issues unsubscribe message to all subscriptions that are currently subscribed.
        /// </summary>
        public void UnsubscribeAll()
        {
            List<UniTask> tasks = new List<UniTask>();

            foreach (ILiveQueryClient c in clients)
            {
                tasks.Add(UniTask.Run(() =>
                {
                    long ticks = DateTime.Now.Ticks;
                    c.Unsubscribe();
                    while ((DateTime.Now.Ticks - ticks) > 2000 || 
                        c.ClientStatus != Queries.Live.LiveQueryClientStatusTypes.Closed);
                }));
            }

            UniTask.WhenAll(tasks.ToArray());
        }

        #if UNITY_WEBGL
        /// <summary>
        /// For WebGL Only should be called on Unity Update.
        /// </summary>
        public void HandleUpdateEvent()
        {
            foreach (ILiveQueryClient c in clients)
            {
                c.HandleUpdateEvent();
            }
        }
        #endif

        /// <summary>
        /// Create and return a subscription to the specified query.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="connectionData"></param>
        /// <param name="callbacks"></param>
        /// <param name="sessionToken"></param>
        /// <param name="installationId"></param>
        /// <returns>TheOneLiveQueryClient<T</returns>
        public static TheOneLiveQueryClient<T> CreateSubscription<T>(TheOneQuery<T> query, IServerConnectionData connectionData, ILiveQueryCallbacks<T> callbacks, IJsonSerializer jsonSerializer, string sessionToken = null, string installationId = null) where T : TheOneObject
        {
            TheOneLiveQueryClient<T> client = new TheOneLiveQueryClient<T>(query, connectionData, callbacks, sessionToken, installationId);
            instance.AddSubscription(client);

            return client;
        }

        public static void UnsubscribeToAllAsync()
        {
            instance.UnsubscribeAll();
        }

        public static void DisposeService()
        {
            instance.Dispose();
        }

        #if UNITY_WEBGL
        public static void UpdateWebSockets()
        {
            instance.HandleUpdateEvent();
        }
        #endif
    }
}
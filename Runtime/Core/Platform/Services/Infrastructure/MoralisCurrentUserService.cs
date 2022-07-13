using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TheOneUnity.Platform.Abstractions;
using TheOneUnity.Platform.Objects;
using TheOneUnity.Platform.Utilities;

namespace TheOneUnity.Platform.Services.Infrastructure
{
    public class TheOneCurrentUserService<TUser> : ICurrentUserService<TUser> where TUser : TheOneUser
    {
        TUser currentUser;

        object Mutex { get; } = new object { };

        ICacheService StorageController { get; }

        IJsonSerializer JsonSerializer { get; }

        public TheOneCurrentUserService(ICacheService storageController, IJsonSerializer jsonSerializer) => (StorageController, JsonSerializer) = (storageController, jsonSerializer);

       
        public TUser CurrentUser
        {
            get
            {
                lock (Mutex)
                    return currentUser;
            }
            set
            {
                lock (Mutex)
                    currentUser = value;
            }
        }

        public async UniTask SetAsync(TUser user, CancellationToken cancellationToken)
        {
            if (user is null)
            {
                IDataCache<string, object> loadResp = await StorageController.LoadAsync();
                await loadResp.RemoveAsync(nameof(CurrentUser));
            }
            else
            {
                string data = JsonSerializer.Serialize(user);

                IDataCache<string, object> loadResp = await StorageController.LoadAsync();
                await loadResp.AddAsync(nameof(CurrentUser), data);
            }

            CurrentUser = user;
        }

        public async UniTask<TUser> GetAsync(IServiceHub<TUser> serviceHub, CancellationToken cancellationToken = default) 
        {
            TUser cachedCurrent = null;

            lock (Mutex)
                cachedCurrent = CurrentUser;

            if (cachedCurrent == null)
            {
                object data = default;
                IDataCache<string, object> loadResp = await StorageController.LoadAsync();
                loadResp.TryGetValue(nameof(CurrentUser), out data);
                TUser user = default;

                if (data is string)
                {
                    user = (TUser)JsonSerializer.Deserialize<TUser>(data.ToString());
                    CurrentUser = user;
                    cachedCurrent = user;
                }
            }

            return cachedCurrent;
        }

        public async UniTask<bool> ExistsAsync(CancellationToken cancellationToken)
        {
            bool result = true;

            if (CurrentUser == null)
            {
                IDataCache<string, object> loadResp = await StorageController.LoadAsync();
                result = loadResp.ContainsKey(nameof(CurrentUser));
            }

            return result;
        }

        public bool IsCurrent(TUser user)
        {
            lock (Mutex)
                return CurrentUser == user;
        }

        public void ClearFromMemory() => CurrentUser = default;

        public async UniTask ClearFromDiskAsync()
        {
            lock (Mutex)
            {
                ClearFromMemory();
            }

            IDataCache<string, object> loadResp = await StorageController.LoadAsync();
            await loadResp.RemoveAsync(nameof(CurrentUser));
        }

        public async UniTask<string> GetCurrentSessionTokenAsync(IServiceHub<TUser> serviceHub, CancellationToken cancellationToken = default)
        {
            string result = null;
            TUser user = await GetAsync(serviceHub, cancellationToken);
            result = user?.sessionToken;

            return result;
        }

        public async UniTask LogOutAsync(IServiceHub<TUser> serviceHub, CancellationToken cancellationToken = default)
        {
            TUser user = await GetAsync(serviceHub, cancellationToken);
            await ClearFromDiskAsync();
        }

    }
}

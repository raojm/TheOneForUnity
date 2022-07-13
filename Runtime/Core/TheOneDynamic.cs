using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using TheOneUnity.Platform;
using TheOneUnity.Platform.Abstractions;
using TheOneUnity.Platform.Objects;
using TheOneUnity.Platform.Queries;
using TheOneUnity.Platform.Services;
using TheOneUnity.Platform.Services.ClientServices;
using TheOneUnity.Platform.Services.Infrastructure;
using TheOneUnity.SolanaApi.Interfaces;
using TheOneUnity.Web3Api.Interfaces;

namespace TheOneUnity
{
    public class TheOneClient<TUser> where TUser : TheOneUser
    {
        string serverAuthToken = String.Empty;

        public TheOneClient(ServerConnectionData connectionData, IWeb3Api web3Api = null, ISolanaApi solanaApi = null, IJsonSerializer jsonSerializer = null)
        {
            if (jsonSerializer == null)
            {
                throw new ArgumentException("jsonSerializer cannot be null.");
            }

            connectionData.Key = connectionData.Key != null ? connectionData.Key : "";

            theoneService = new TheOneService<TUser>(connectionData.ApplicationID, connectionData.ServerURI, connectionData.Key, jsonSerializer);
            theoneService.ServerConnectionData.Key = connectionData.Key;
            theoneService.ServerConnectionData.ServerURI = connectionData.ServerURI;
            theoneService.ServerConnectionData.ApplicationID = connectionData.ApplicationID; theoneService.ServerConnectionData.LocalStoragePath = connectionData.LocalStoragePath;

            // Make sure local folder for Unity apps is used if defined.
            TheOneCacheService<TUser>.BaseFilePath = connectionData.LocalStoragePath;

            // Make sure singleton instance is available.
            theoneService.Publicize();

            this.Web3Api = web3Api;

            if (this.Web3Api is { })
            {
                if (connectionData.ApiKey is { })
                {
                    this.Web3Api.Initialize();
                }
                else
                {
                    this.Web3Api.Initialize(connectionData.ServerURI);
                }
            }

            this.SolanaApi = solanaApi;

            if (this.SolanaApi is { })
            {
                if (connectionData.ApiKey is { })
                {
                    this.SolanaApi.Initialize();
                }
                else
                {
                    this.SolanaApi.Initialize(connectionData.ServerURI);
                }
            }
        }


        public string EthAddress { get; }

        public IServiceHub<TUser> ServiceHub => theoneService.Services;

        TheOneService<TUser> theoneService;

        public void SetLocalDatastoreController()
        {
            throw new NotImplementedException();
        }

        public string ApplicationId
        {
            get => theoneService.ServerConnectionData.ApplicationID;
            set
            {
                theoneService.ServerConnectionData.ApplicationID = value;
            }
        }

        public string Key
        {
            get => theoneService.ServerConnectionData.Key;
            set
            {
                theoneService.ServerConnectionData.Key = value;
            }
        }

        public string MasterKey
        {
            get => theoneService.ServerConnectionData.MasterKey;
            set
            {
                theoneService.ServerConnectionData.MasterKey = value;
            }
        }

        public string ServerUrl
        {
            get => theoneService.ServerConnectionData.ServerURI;
            set
            {
                theoneService.ServerConnectionData.ServerURI = value;
            }
        }

        public void SetServerAuthToken(string value)
        {
            serverAuthToken = value;
        }

        public string GetServerAuthToken()
        {
            return serverAuthToken;
        }

        public void SetServerAuthType(string value)
        {
            serverAuthToken = value;
        }

        public string GetServerAuthType()
        {
            return serverAuthToken;
        }

        public void SetLiveQueryServerURL(string value)
        {
            throw new NotImplementedException();
        }

        public string GetLiveQueryServerURL()
        {
            throw new NotImplementedException();
        }

        public void SetEncryptedUser(string value)
        {
            throw new NotImplementedException();
        }

        public string GetEncryptedUser()
        {
            throw new NotImplementedException();
        }

        public void SetSecret(string value)
        {
            throw new NotImplementedException();
        }

        public string GetSecret()
        {
            throw new NotImplementedException();
        }

        public void SetIdempotency(string value)
        {
            throw new NotImplementedException();
        }

        public string GetIdempotency()
        {
            throw new NotImplementedException();
        }

        public IFileService File => theoneService.FileService;

        public IInstallationService InstallationService => theoneService.InstallationService;
        public IQueryService QueryService => theoneService.QueryService;
        public ISessionService<TUser> Session => theoneService.SessionService;
        public IUserService<TUser> UserService => theoneService.UserService;

        public TheOneCloud<TUser> Cloud => theoneService.Cloud;


        public async UniTask<Guid?> GetInstallationIdAsync() => await InstallationService.GetAsync();

        public async UniTask<TheOneQuery<T>> Query<T>() where T : TheOneObject
        {
            TUser user = await GetCurrentUserAsync();
            return new TheOneQuery<T>(this.QueryService, InstallationService, theoneService.ServerConnectionData, theoneService.JsonSerializer, user.sessionToken); //, logger);
        }

        public T Create<T>(object[] parameters = null) where T : TheOneObject
        {
            return this.ServiceHub.Create<T>(parameters);
        }

        public async UniTask<TUser> GetCurrentUserAsync() => await this.ServiceHub.GetCurrentUserAsync();


        public void Dispose()
        {
#if UNITY_WEBGL
            TheOneLiveQueryManager.DisposeService();
#endif
        }

        /// <summary>
        /// Retrieve user object by sesion token.
        /// </summary>
        /// <param name="sessionToken"></param>
        /// <returns>Task<TheOneUser</returns>
        public UniTask<TUser> UserFromSession(string sessionToken)
        {
            return this.ServiceHub.BecomeAsync<TUser>(sessionToken);
        }

        /// <summary>
        /// Provid async user login.
        /// data: 
        ///     id: Address
        ///     signature: Signature from wallet
        ///     data: Message that was signed
        /// </summary>
        /// <param name="data">Authentication data</param>
        /// <returns>Task<TUser></returns>
        public async UniTask<TUser> LogInAsync(IDictionary<string, object> data)
        {
            return await this.LogInAsync(data, CancellationToken.None);
        }

        /// Provid async user login.
        /// data: 
        ///     id: Address
        ///     signature: Signature from wallet
        ///     data: Message that was signed
        /// </summary>
        /// <param name="data">Authentication data</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Task<TUser></returns>
        public async UniTask<TUser> LogInAsync(IDictionary<string, object> data, CancellationToken cancellationToken)
        {
            return await this.ServiceHub.LogInWithAsync("moralisEth", data, cancellationToken);
        }

        /// <summary>
        /// Logs out the current user.
        /// </summary>
        /// <returns></returns>
        public async UniTask LogOutAsync()
        {
            await this.ServiceHub.LogOutAsync<TUser>();
        }

        /// <summary>
        /// Constructs a query that is the and of the given queries.
        /// </summary>
        /// <typeparam name="T">The type of TheOneObject being queried.</typeparam>
        /// <param name="serviceHub"></param>
        /// <param name="source">An initial query to 'and' with additional queries.</param>
        /// <param name="queries">The list of TheOneQueries to 'and' together.</param>
        /// <returns>A query that is the and of the given queries.</returns>
        public TheOneQuery<T> BuildAndQuery<T>(TheOneQuery<T> source, params TheOneQuery<T>[] queries) where T : TheOneObject
        {
            return ServiceHub.ConstructAndQuery<T, TUser>(source, queries);
        }

        /// <summary>
        /// Construct a query that is the and of two or more queries.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serviceHub"></param>
        /// <param name="queries">The list of TheOneQueries to 'and' together.</param>
        /// <returns>A TheOneQquery that is the 'and' of the passed in queries.</returns>
        public TheOneQuery<T> BuildAndQuery<T>(IEnumerable<TheOneQuery<T>> queries) where T : TheOneObject
        {
            return ServiceHub.ConstructAndQuery<T, TUser>(queries);
        }

        /// <summary>
        /// Constructs a query that is the or of the given queries.
        /// </summary>
        /// <typeparam name="T">The type of TheOneObject being queried.</typeparam>
        /// <param name="source">An initial query to 'or' with additional queries.</param>
        /// <param name="queries">The list of TheOneQueries to 'or' together.</param>
        /// <returns>A query that is the or of the given queries.</returns>
        public TheOneQuery<T> BuildOrQuery<T>(TheOneQuery<T> source, params TheOneQuery<T>[] queries) where T : TheOneObject
        {
            return ServiceHub.ConstructOrQuery<T, TUser>(source, queries);
        }

        /// <summary>
        /// Construct a query that is the 'or' of two or more queries.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serviceHub"></param>
        /// <param name="queries">The list of TheOneQueries to 'and' together.</param>
        /// <returns>A TheOneQquery that is the 'or' of the passed in queries.</returns>
        public TheOneQuery<T> BuildOrQuery<T>(IEnumerable<TheOneQuery<T>> queries) where T : TheOneObject
        {
            return ServiceHub.ConstructOrQuery<T, TUser>(queries);
        }

        /// <summary>
        /// Constructs a query that is the nor of the given queries.
        /// </summary>
        /// <typeparam name="T">The type of TheOneObject being queried.</typeparam>
        /// <param name="source">An initial query to 'or' with additional queries.</param>
        /// <param name="queries">The list of TheOneQueries to 'or' together.</param>
        /// <returns>A query that is the nor of the given queries.</returns>
        public TheOneQuery<T> BuildNorQuery<T>(TheOneQuery<T> source, params TheOneQuery<T>[] queries) where T : TheOneObject
        {
            return ServiceHub.ConstructNorQuery<T, TUser>(source, queries);
        }

        /// <summary>
        /// Construct a query that is the 'nor' of two or more queries.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serviceHub"></param>
        /// <param name="queries">The list of TheOneQueries to 'and' together.</param>
        /// <returns>A TheOneQquery that is the 'nor' of the passed in queries.</returns>
        public TheOneQuery<T> BuildNorQuery<T>(IEnumerable<TheOneQuery<T>> queries) where T : TheOneObject
        {
            return ServiceHub.ConstructNorQuery<T, TUser>(queries);
        }

        /// <summary>
        /// Deletes target object from the TheOne database.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <returns></returns>
        public UniTask DeleteAsync<T>(T target) where T : TheOneObject
        {
            return target.DeleteAsync();
        }

        /// <summary>
        /// Provide an object hook for Web3Api incase developer supplies a
        /// web3api client at initialize
        /// </summary>
        public IWeb3Api Web3Api { get; private set; }

        /// <summary>
        /// Provide an object hook for SolanaApi
        /// </summary>
        public ISolanaApi SolanaApi { get; private set; }

        /// <summary>
        /// Included so that this can be set prior to initialization for systems
        /// (Unity, Xamarin, etc.) that may not have Assembly Attributes available.
        /// </summary>
        public static HostManifestData ManifestData
        {
            get => ServiceHub<TUser>.ManifestData;
            set
            {
                ServiceHub<TUser>.ManifestData = value;
                MutableServiceHub<TUser>.ManifestData = value;
            }
        }
    }


}

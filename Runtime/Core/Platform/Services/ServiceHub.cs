﻿using System;
using System.Net.Http;
using TheOneUnity.Platform.Abstractions;
using TheOneUnity.Platform.Objects;
using TheOneUnity.Platform.Services.ClientServices;
using TheOneUnity.Platform.Services.Infrastructure;
using TheOneUnity.Platform.Utilities;

namespace TheOneUnity.Platform.Services
{
    /// <summary>
    /// A service hub that uses late initialization to efficiently provide controllers and other dependencies to internal TheOne SDK systems.
    /// </summary>
    public class ServiceHub<TUser> : IServiceHub<TUser> where TUser : TheOneUser
    {
        private static ServiceHub<TUser> instance;

        #region Instance
        public bool AlwaysSave { get; set; }
        LateInitializer LateInitializer { get; } = new LateInitializer { };
        private UniversalWebClient webClient;

        public ServiceHub()
        {
            webClient = new UniversalWebClient { };
        }

        public ServiceHub(IServerConnectionData connectionData, IJsonSerializer jsonSerializer, HttpClient httpClient = null)
        {
            //httpClient = httpClient is { } ? httpClient : new HttpClient();
            webClient = new UniversalWebClient();
            httpClient.DefaultRequestHeaders.Remove("IfModifiedSince");
            ServerConnectionData ??= connectionData;
            JsonSerializer = jsonSerializer is { } ? jsonSerializer : throw new ArgumentException("jsonSerializer cannot be null.");

            AlwaysSave = true;
        }

        /// <summary>
        /// Included so that this can be set prior to initialization for systems
        /// (Unity, Xamarin, etc.) that may not have Assembly Attributes available.
        /// </summary>
        public static HostManifestData ManifestData { get; set; }

        public IServerConnectionData ServerConnectionData { get; set; }
        /// <summary>
        /// Provides Serialization / Deserialization services.
        /// </summary>
        public IJsonSerializer JsonSerializer { get; set; }
        public IMetadataService MetadataService => LateInitializer.GetValue(() => new MetadataService { HostManifestData = ManifestData ?? HostManifestData.Inferred, EnvironmentData = EnvironmentData.Inferred });

        public IWebClient WebClient => LateInitializer.GetValue(() => webClient);
        public ICacheService CacheService => LateInitializer.GetValue(() => new TheOneCacheService<TUser> (TheOneCacheService<TUser>.DefineRelativeFilePath("TheOne\\theone.cachefile")));
        public IInstallationService InstallationService => LateInitializer.GetValue(() => new InstallationService(CacheService));
        public ITheOneCommandRunner CommandRunner => LateInitializer.GetValue(() => new TheOneCommandRunner<TUser>(WebClient, InstallationService, MetadataService, ServerConnectionData, new Lazy<IUserService<TUser>>(() => UserService)));
        public IUserService<TUser> UserService => LateInitializer.GetValue(() => new TheOneUserService<TUser>(CommandRunner, ObjectService, JsonSerializer));
        public ICurrentUserService<TUser> CurrentUserService => LateInitializer.GetValue(() => new TheOneCurrentUserService<TUser>(CacheService, JsonSerializer));
        public IObjectService ObjectService => LateInitializer.GetValue(() => new TheOneObjectService(CommandRunner, ServerConnectionData, JsonSerializer));
        public IQueryService QueryService => LateInitializer.GetValue(() => new TheOneQueryService(CommandRunner, this.CurrentUserService.CurrentUser?.sessionToken, JsonSerializer, ObjectService));
        public ISessionService<TUser> SessionService => LateInitializer.GetValue(() => new TheOneSessionService<TUser>(CommandRunner, JsonSerializer));
        public ICloudFunctionService CloudFunctionService => LateInitializer.GetValue(() => new TheOneCloudFunctionService(CommandRunner, ServerConnectionData, JsonSerializer));
        public IFileService FileService => LateInitializer.GetValue(() => new TheOneFileService(CommandRunner, JsonSerializer));
        

        public bool Reset() => LateInitializer.Used && LateInitializer.Reset();

        public T Create<T>(object[] parameters) where T : TheOneObject
        {
            T thing;
            
            if (parameters is { } && parameters.Length > 0)
                thing = (T)Activator.CreateInstance(typeof(T), parameters);
            else
                thing = (T)Activator.CreateInstance(typeof(T));

            thing.sessionToken = this.CurrentUserService.CurrentUser?.sessionToken;
            thing.ObjectService = this.ObjectService;

            return thing;
        }
        #endregion

        public static ServiceHub<TUser> GetInstance() {
            if (!(instance is { }))
                instance = new ServiceHub<TUser>();

            return instance;
        }

        public static ServiceHub<TUser> GetInstance(IServerConnectionData connectionData, HttpClient httpClient = null, IJsonSerializer jsonSerializer = null)
        {
            if (!(instance is { }))
                instance = new ServiceHub<TUser>(connectionData, jsonSerializer, httpClient);

            return instance;
        }
    }

}

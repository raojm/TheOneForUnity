﻿using System;
using TheOneUnity.Platform.Abstractions;
using TheOneUnity.Platform.Objects;
using TheOneUnity.Platform.Services.ClientServices;
using TheOneUnity.Platform.Services.Infrastructure;

namespace TheOneUnity.Platform.Services
{
    /// <summary>
    /// A service hub that is mutable.
    /// </summary>
    /// <remarks>This class is not thread safe; the mutability is allowed for the purposes of overriding values before it is used, as opposed to modifying it while it is in use.</remarks>
    public class MutableServiceHub<TUser> : IMutableServiceHub<TUser> where TUser : TheOneUser
    {
        /// <summary>
        /// Included so that this can be set prior to initialization for systems
        /// (Unity, Xamarin, etc.) that may not have Assembly Attributes available.
        /// </summary>
        public static HostManifestData ManifestData { get; set; }

        public MutableServiceHub() { AlwaysSave = true; }

        public bool AlwaysSave { get; set; }

        public IServerConnectionData ServerConnectionData { get; set; }

        public IMetadataService MetadataService { get; set; }

        public IJsonSerializer JsonSerializer { get; set; }

        public IWebClient WebClient { get; set; }
        public ICacheService CacheService { get; set; }

        public IInstallationService InstallationService { get; set; }

        public ITheOneCommandRunner CommandRunner { get; set; }

        public ICloudFunctionService CloudFunctionService { get; set; }
        //public IParseConfigurationController ConfigurationController { get; set; }
        public IFileService FileService { get; set; }
        public IObjectService ObjectService { get; set; }
        public IQueryService QueryService { get; set; }
        public ISessionService<TUser> SessionService { get; set; }

        public IUserService<TUser> UserService { get; set; }

        public ICurrentUserService<TUser> CurrentUserService { get; set; }

        public T Create<T>(object[] parameters) where T : TheOneObject
        {
            T thing;

            if (parameters is { } && parameters.Length > 0)
                thing = (T)Activator.CreateInstance(typeof(T), parameters);
            else
                thing = (T)Activator.CreateInstance(typeof(T));

            thing.sessionToken = this.CurrentUserService.CurrentUser.sessionToken;
            thing.ObjectService = this.ObjectService;

            return thing;
        }

        public MutableServiceHub<TUser> SetDefaults(IServerConnectionData connectionData = default)
        {
            ServerConnectionData ??= connectionData;
            MetadataService ??= new MetadataService
            {
                EnvironmentData = EnvironmentData.Inferred,
                HostManifestData = ManifestData ?? HostManifestData.Inferred
            };


            WebClient ??= new UniversalWebClient { };
            CacheService ??= new TheOneCacheService<TUser> (TheOneCacheService<TUser>.DefineRelativeFilePath("TheOne\\theone.cachefile"));

            InstallationService ??= new InstallationService(CacheService);
            CommandRunner ??= new TheOneCommandRunner<TUser>(WebClient, InstallationService, MetadataService, ServerConnectionData, new Lazy<IUserService<TUser>>(() => UserService));

            CloudFunctionService ??= new TheOneCloudFunctionService(CommandRunner, ServerConnectionData, JsonSerializer);
            //ConfigurationController ??= new ParseConfigurationController(CommandRunner, CacheController, Decoder);
            FileService ??= new TheOneFileService(CommandRunner, JsonSerializer);
            ObjectService ??= new TheOneObjectService(CommandRunner, ServerConnectionData, JsonSerializer);
            QueryService ??= new TheOneQueryService(CommandRunner, this.CurrentUserService.CurrentUser.sessionToken, JsonSerializer, ObjectService);
            SessionService ??= new TheOneSessionService<TUser>(CommandRunner, JsonSerializer);
            UserService ??= new TheOneUserService<TUser>(CommandRunner, ObjectService, JsonSerializer);
            CurrentUserService ??= new TheOneCurrentUserService<TUser>(CacheService, JsonSerializer);

            //AnalyticsController ??= new ParseAnalyticsController(CommandRunner);

            //InstallationCoder ??= new ParseInstallationCoder(Decoder, ClassController);

            //PushController ??= new ParsePushController(CommandRunner, CurrentUserController);
            //CurrentInstallationController ??= new ParseCurrentInstallationController(InstallationController, CacheController, InstallationCoder, ClassController);
            //PushChannelsController ??= new ParsePushChannelsController(CurrentInstallationController);
            //InstallationDataFinalizer ??= new ParseInstallationDataFinalizer { };

            return this;
        }
    }

}

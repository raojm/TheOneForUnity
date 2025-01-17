#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using TheOneUnity.Platform.Objects;

namespace TheOneUnity.Platform.Abstractions
{
    /// <summary>
    /// The dependency injection container for all internal .NET TheOne SDK services.
    /// </summary>
    public interface IServiceHub <TUser> where TUser : TheOneUser
    {
        /// <summary>
        /// If true when an object is requested to be saved it is always saved regardless of if it was
        /// updated or not. If false, the object is only saved if the IsDirty flag has been set.
        /// </summary>
        bool AlwaysSave { get; set; }

        /// <summary>
        /// The current server connection data that the the TheOne SDK has been initialized with.
        /// </summary>
        IServerConnectionData ServerConnectionData { get; }
        IMetadataService MetadataService { get; }

        /// <summary>
        /// Provides Serialization / Deserialization services.
        /// </summary>
        IJsonSerializer JsonSerializer { get; }

        IWebClient WebClient { get; }
        ICacheService CacheService { get; }

        IInstallationService InstallationService{ get; }
        ITheOneCommandRunner CommandRunner { get; }

        ICloudFunctionService CloudFunctionService { get; }

        IFileService FileService { get; }
        IObjectService ObjectService { get; }
        IQueryService QueryService { get; }
        ISessionService<TUser> SessionService { get; }
        IUserService<TUser> UserService { get; }
        ICurrentUserService<TUser> CurrentUserService { get; }

        T Create<T>(object[] parameters) where T : TheOneObject;

    }
}

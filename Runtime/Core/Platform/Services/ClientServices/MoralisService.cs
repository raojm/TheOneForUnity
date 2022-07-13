using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TheOneUnity.Platform.Abstractions;
using TheOneUnity.Platform.Objects;
using TheOneUnity.Platform.Utilities;

namespace TheOneUnity.Platform.Services.ClientServices
{
    public class TheOneService<TUser> : CustomServiceHub<TUser>, IServiceHubComposer<TUser> where TUser : TheOneUser
    {


        /// <summary>
        /// Contains, in order, the official ISO date and time format strings, and two modified versions that account for the possibility that the server-side string processing mechanism removed trailing zeroes.
        /// </summary>
        internal static string[] DateFormatStrings { get; } =
        {
            "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'",
            "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'ff'Z'",
            "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'f'Z'",
        };

        /// <summary>
        /// Gets whether or not the assembly using the TheOne SDK was compiled by IL2CPP.
        /// </summary>
        public static bool IL2CPPCompiled { get; set; } = AppDomain.CurrentDomain?.FriendlyName?.Equals("IL2CPP Root Domain") == true;

        /// <summary>
        /// The configured default instance of <see cref="TheOneClient"/> to use.
        /// </summary>
        public static TheOneService<TUser> Instance { get; private set; }

        internal static string Version => typeof(TheOneService<TUser>)?.Assembly?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? typeof(TheOneService<TUser>)?.Assembly?.GetName()?.Version?.ToString();

        /// <summary>
        /// Services that provide essential functionality.
        /// </summary>
        public override IServiceHub<TUser> Services { get; internal set; }

        /// <summary>
        /// Creates a new <see cref="TheOneClient"/> and authenticates it as belonging to your application. This class is a hub for interacting with the SDK. The recommended way to use this class on client applications is to instantiate it, then call <see cref="Publicize"/> on it in your application entry point. This allows you to access <see cref="Instance"/>.
        /// </summary>
        /// <param name="applicationID">The Application ID provided in the TheOne dashboard.</param>
        /// <param name="serverURL">The server URL provided in the TheOne dashboard.</param>
        /// <param name="key">The .NET Key provided in the TheOne dashboard.</param>
        /// <param name="serviceHub">A service hub to override internal services and thereby make the TheOne SDK operate in a custom manner.</param>
        /// <param name="configurators">A set of <see cref="IServiceHubMutator"/> implementation instances to tweak the behaviour of the SDK.</param>
        public TheOneService(string applicationID, string serverURL, string key, IJsonSerializer jsonSerializer, IServiceHub<TUser> serviceHub = default, params IServiceHubMutator[] configurators) : this(new ServerConnectionData { ApplicationID = applicationID, ServerURI = serverURL, Key = key }, jsonSerializer, serviceHub, configurators) { }

        /// <summary>
        /// Creates a new <see cref="TheOneClient"/> and authenticates it as belonging to your application. This class is a hub for interacting with the SDK. The recommended way to use this class on client applications is to instantiate it, then call <see cref="Publicize"/> on it in your application entry point. This allows you to access <see cref="Instance"/>.
        /// </summary>
        /// <param name="configuration">The configuration to initialize TheOne with.</param>
        /// <param name="serviceHub">A service hub to override internal services and thereby make the TheOne SDK operate in a custom manner.</param>
        /// <param name="configurators">A set of <see cref="IServiceHubMutator"/> implementation instances to tweak the behaviour of the SDK.</param>
        public TheOneService(IServerConnectionData configuration, IJsonSerializer jsonSerializer, IServiceHub<TUser> serviceHub = default, params IServiceHubMutator[] configurators)
        {
            Services = serviceHub is { } ? new OrchestrationServiceHub<TUser> { Custom = serviceHub, Default = new ServiceHub<TUser> { ServerConnectionData = GenerateServerConnectionData(), JsonSerializer = jsonSerializer } } : new ServiceHub<TUser> { ServerConnectionData = GenerateServerConnectionData(), JsonSerializer = jsonSerializer } as IServiceHub<TUser>;

            IServerConnectionData GenerateServerConnectionData() => configuration switch
            {
                null => throw new ArgumentNullException(nameof(configuration)),
                ServerConnectionData { Test: true, ServerURI: { } } data => data,
                ServerConnectionData { Test: true } data => new ServerConnectionData
                {
                    ApplicationID = data.ApplicationID,
                    Headers = data.Headers,
                    MasterKey = data.MasterKey,
                    Test = data.Test,
                    Key = data.Key,
                    ServerURI = "https://api.TheOne.com/1/"
                },
                { ServerURI: "https://api.TheOne.com/1/" } => throw new InvalidOperationException("Since the official TheOne server has shut down, you must specify a URI that points to a hosted instance."),
                { ApplicationID: { }, ServerURI: { }, Key: { } } data => data,
                _ => throw new InvalidOperationException("The IServerConnectionData implementation instance provided to the TheOneClient constructor must be populated with the information needed to connect to a TheOne server instance.")
            };

            // If a WS/WSS URI is not supplied create it from the server URL.
            if (String.IsNullOrWhiteSpace(configuration.LiveQueryServerURI))
            {
                configuration.LiveQueryServerURI = Conversion.WebUriToWsURi(configuration.ServerURI);
            }

            if (configurators is { Length: int length } && length > 0)
            {
                Services = serviceHub switch
                {
                    IMutableServiceHub<TUser> { } mutableServiceHub => BuildHub((Hub: mutableServiceHub, mutableServiceHub.ServerConnectionData = serviceHub.ServerConnectionData != null ? serviceHub.ServerConnectionData : Services.ServerConnectionData).Hub, Services, configurators),
                    { } => BuildHub(default, Services, configurators)
                };
            }

            Cloud = new TheOneCloud<TUser>((IServiceHub<TUser>)this.Services);
            
            //Services.ClassController.AddIntrinsic();
        }

        /// <summary>
        /// Initializes a <see cref="TheOneClient"/> instance using the <see cref="IServiceHub.Cloner"/> set on the <see cref="Instance"/>'s <see cref="Services"/> <see cref="IServiceHub"/> implementation instance.
        /// </summary>
        //public TheOneClient() => Services = (Instance ?? throw new InvalidOperationException("A TheOneClient instance with an initializer service must first be publicized in order for the default constructor to be used.")).Services.Cloner.BuildHub(Instance.Services, this);

        /// <summary>
        /// Sets this <see cref="TheOneClient"/> instance as the template to create new instances from.
        /// </summary>
        ///// <param name="publicize">Declares that the current <see cref="TheOneClient"/> instance should be the publicly-accesible <see cref="Instance"/>.</param>
        public void Publicize()
        {
            lock (Mutex)
            {
                Instance = this;
            }
        }

        public TheOneCloud<TUser> Cloud { get; private set; }


        static object Mutex { get; } = new object { };

        internal static string BuildQueryString(IDictionary<string, object> parameters) => String.Join("&", (from pair in parameters let valueString = pair.Value as string select $"{Uri.EscapeDataString(pair.Key)}={Uri.EscapeDataString(String.IsNullOrEmpty(valueString) ? JsonUtilities.Encode(pair.Value) : valueString)}").ToArray());

        internal static IDictionary<string, string> DecodeQueryString(string queryString)
        {
            Dictionary<string, string> query = new Dictionary<string, string> { };

            foreach (string pair in queryString.Split('&'))
            {
                string[] parts = pair.Split(new char[] { '=' }, 2);
                query[parts[0]] = parts.Length == 2 ? Uri.UnescapeDataString(parts[1].Replace("+", " ")) : null;
            }

            return query;
        }

        //internal static IDictionary<string, object> DeserializeJsonString(string jsonData) => JsonUtilities.Parse(jsonData) as IDictionary<string, object>;

        //internal static string SerializeJsonString(IDictionary<string, object> jsonData) => JsonUtilities.Encode(jsonData);

        public IServiceHub<TUser> BuildHub(IMutableServiceHub<TUser> target = default, IServiceHub<TUser> extension = default, params IServiceHubMutator[] configurators)
        {
            OrchestrationServiceHub<TUser> orchestrationServiceHub = new OrchestrationServiceHub<TUser> 
            { 
                Custom = target != null ? target : new MutableServiceHub<TUser> { }, 
                Default = extension != null ? extension : new ServiceHub<TUser> { } 
            };

            foreach (IServiceHubMutator mutator in configurators.Where(configurator => configurator.Valid))
            {
                mutator.Mutate(ref target, orchestrationServiceHub);
                orchestrationServiceHub.Custom = target;
            }

            return orchestrationServiceHub;
        }
    }
}

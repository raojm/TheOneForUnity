using System;
using TheOneUnity.Web3Api.Client;

namespace TheOneUnity.Web3Api
{
    /// <summary>
    /// Provides an easy to wrapper around the TheOne Web3Api REST services.
    /// </summary>
    public class TheOneClient
    {
        Web3ApiClient client = new Web3ApiClient();

        static TheOneClient instance = new TheOneClient();

        private TheOneClient() { }

        /// <summary>
        /// Initialize TheOne Web3API. Use this to initialize to your personal 
        /// TheOne server. Major advantage is api key is supplied 
        /// </summary>
        /// <param name="url"></param>
        public static void Initialize(string url) => instance.client.Initialize(url);

        /// <summary>
        /// Initialize TheOne Web3API. 
        /// </summary>
        /// <param name="useStandardServer">If true enforces use of the standard REST server</param>
        /// <param name="apiKey">Required if useStandardServer is true</param>
        /// <param name="url">Optional server url. If not provided default standard server Url is used.</param>
        public static void Initialize(bool useStandardServer, string apiKey = null, string url = null)
        {
            if (useStandardServer && !(apiKey is { })) throw new ArgumentException("API Key is required for Standard REST server.");

            if (apiKey is { }) Configuration.ApiKey["X-API-Key"] = apiKey;
            instance.client.Initialize(url);
        }

        /// <summary>
        /// Gets the Web3ApiClient instance. TheOne.Initialize must be called first.
        /// If TheOne is not initialized this will throw an ApiException.
        /// </summary>
        /// <exception cref="ApiException">Thrown when TheOne.Initialize has not been called.</exception>
        public static Web3ApiClient Web3Api
        {
            get => instance.client.IsInitialized ? instance.client : throw new ApiException(109, "TheOne must be initialized before use.");
        }
    }
}

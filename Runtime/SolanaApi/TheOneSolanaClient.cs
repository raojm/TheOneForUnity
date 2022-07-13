using System;
using TheOneUnity.SolanaApi.Client;

namespace TheOneUnity.SolanaApi
{
    public class TheOneSolanaClient
    {
        SolanaApiClient client = new SolanaApiClient();

        static TheOneSolanaClient instance = new TheOneSolanaClient();

        private TheOneSolanaClient() { }

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

            Configuration.ApiKey.Clear();

            if (apiKey is { }) Configuration.ApiKey["X-API-Key"] = apiKey;
            instance.client.Initialize(url);
        }

        /// <summary>
        /// Gets the Web3ApiClient instance. TheOne.Initialize must be called first.
        /// If TheOne is not initialized this will throw an ApiException.
        /// </summary>
        /// <exception cref="ApiException">Thrown when TheOne.Initialize has not been called.</exception>
        public static SolanaApiClient SolanaApi
        {
            get => instance.client.IsInitialized ? instance.client : throw new ApiException(109, "TheOne must be initialized before use.");
        }
    }
}

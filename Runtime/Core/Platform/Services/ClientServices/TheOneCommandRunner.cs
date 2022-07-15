using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Status = System.Net.HttpStatusCode;
using TheOneUnity.Platform.Utilities;
using Cysharp.Threading.Tasks;
using TheOneUnity.Platform.Abstractions;
using TheOneUnity.Platform.Exceptions;
using TheOneUnity.Platform.Objects;
using TheOneUnity.Platform.Services.Models;

namespace TheOneUnity.Platform.Services.ClientServices
{
    /// <summary>
    /// The command runner for all SDK operations that need to interact with the targeted deployment of TheOne Server.
    /// </summary>
    public class TheOneCommandRunner<TUser> : ITheOneCommandRunner where TUser : TheOneUser
    {
        IWebClient WebClient { get; }

        IInstallationService InstallationService { get; }

        IMetadataService MetadataService { get; }

        IServerConnectionData ServerConnectionData { get; }

        Lazy<IUserService<TUser>> UserService { get; }

        IWebClient GetWebClient() => WebClient;

        /// <summary>
        /// Creates a new TheOne SDK command runner.
        /// </summary>
        /// <param name="webClient">The <see cref="IWebClient"/> implementation instance to use.</param>
        /// <param name="installationController">The <see cref="IInstallationService"/> implementation instance to use.</param>
        public TheOneCommandRunner(IWebClient webClient, IInstallationService installationController, IMetadataService metadataController, IServerConnectionData serverConnectionData, Lazy<IUserService<TUser>> userController)
        {
            WebClient = webClient;
            InstallationService = installationController;
            MetadataService = metadataController;
            ServerConnectionData = serverConnectionData;
            UserService = userController;

            
        }

        /// <summary>
        /// Runs a specified <see cref="TheOneCommand"/>.
        /// </summary>
        /// <param name="command">The <see cref="TheOneCommand"/> to run.</param>
        /// <param name="uploadProgress">An <see cref="IProgress{TheOneUploadProgressEventArgs}"/> instance to push upload progress data to.</param>
        /// <param name="downloadProgress">An <see cref="IProgress{TheOneDownloadProgressEventArgs}"/> instance to push download progress data to.</param>
        /// <param name="cancellationToken">An asynchronous operation cancellation token that dictates if and when the operation should be cancelled.</param>
        /// <returns>Tuple<HttpStatusCode, string></returns>
        public async UniTask<Tuple<HttpStatusCode, string>> RunCommandAsync(TheOneCommand command, IProgress<IDataTransferLevel> uploadProgress = null, IProgress<IDataTransferLevel> downloadProgress = null, CancellationToken cancellationToken = default)
        {
            TheOneCommand cmd = await PrepareCommand(command);

            Tuple<Status, string> cmdResult = await GetWebClient().ExecuteAsync(cmd); //, uploadProgress, downloadProgress, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            string content = cmdResult.Item2;
            int responseCode = (int)cmdResult.Item1;

            if (responseCode >= 500)
            {
                throw new TheOneFailureException(TheOneFailureException.ErrorCode.InternalServerError, cmdResult.Item2);
            }

            // The TheOne server returns POCO saved dates as an object. Convert to ISO datetime string.
            string adjustedData = cmdResult.Item2.AdjustJsonForParseDate();

            // Remove Results wrapper.
            if (adjustedData.StartsWith("{\"results\":"))
            {
                adjustedData = adjustedData.Substring(11, adjustedData.Length - 12);
            }
            else if (adjustedData.StartsWith("{\"result\":"))
            {
                adjustedData = adjustedData.Substring(10, adjustedData.Length - 11);
            }

            Tuple<HttpStatusCode, string> newResponse = new Tuple<HttpStatusCode, string>(cmdResult.Item1, adjustedData);

            return newResponse;
        }

        async UniTask<TheOneCommand> PrepareCommand(TheOneCommand command)
        {
            TheOneCommand newCommand = new TheOneCommand(command)
            {
                Resource = ServerConnectionData.ServerURI
            };

            Guid? guid = await InstallationService.GetAsync();

            lock (newCommand.Headers)
            {
                newCommand.Headers.Add(new KeyValuePair<string, string>("X-Parse-Installation-Id", guid?.ToString()));
            }

            lock (newCommand.Headers)
            {
                KeyValuePair<string, string> appId = new KeyValuePair<string, string>("X-Parse-Application-Id", ServerConnectionData.ApplicationID);

                if (!newCommand.Headers.Contains(appId))
                {
                    newCommand.Headers.Add(appId);
                }

                KeyValuePair<string, string> clientVersion = new KeyValuePair<string, string>("X-Parse-Client-Version", TheOneService<TUser>.Version.ToString());

                if (!newCommand.Headers.Contains(clientVersion))
                {
                    newCommand.Headers.Add(clientVersion);
                }

                if (ServerConnectionData.Headers != null)
                {
                    foreach (KeyValuePair<string, string> header in ServerConnectionData.Headers)
                    {
                        newCommand.Headers.Add(header);
                    }
                }

                if (!String.IsNullOrEmpty(MetadataService.HostManifestData.Version))
                {
                    newCommand.Headers.Add(new KeyValuePair<string, string>("X-Parse-App-Build-Version", MetadataService.HostManifestData.Version));
                }

                if (!String.IsNullOrEmpty(MetadataService.HostManifestData.ShortVersion))
                {
                    newCommand.Headers.Add(new KeyValuePair<string, string>("X-Parse-App-Display-Version", MetadataService.HostManifestData.ShortVersion));
                }

                if (!String.IsNullOrEmpty(MetadataService.EnvironmentData.OSVersion))
                {
                    newCommand.Headers.Add(new KeyValuePair<string, string>("X-Parse-OS-Version", MetadataService.EnvironmentData.OSVersion));
                }

                if (!String.IsNullOrEmpty(ServerConnectionData.MasterKey))
                {
                    newCommand.Headers.Add(new KeyValuePair<string, string>("X-Parse-Master-Key", ServerConnectionData.MasterKey));
                }
                else
                {
                    newCommand.Headers.Add(new KeyValuePair<string, string>("X-Parse-Windows-Key", ServerConnectionData.Key));
                }

                if (UserService.Value.RevocableSessionEnabled)
                {
                    newCommand.Headers.Add(new KeyValuePair<string, string>("X-Parse-Revocable-Session", "1"));
                }
            }

            return newCommand;
        }
    }
}

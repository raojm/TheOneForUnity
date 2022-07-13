
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Cysharp.Threading.Tasks;
using TheOneUnity.Platform.Services.Models;

namespace TheOneUnity.Platform.Abstractions
{
    public interface ITheOneCommandRunner
    {
        /// <summary>
        /// Executes <see cref="TheOneCommand"/> and convert the result into Dictionary.
        /// </summary>
        /// <param name="command">The command to be run.</param>
        /// <param name="uploadProgress">Upload progress callback.</param>
        /// <param name="downloadProgress">Download progress callback.</param>
        /// <param name="cancellationToken">The cancellation token for the request.</param>
        /// <returns></returns>
        UniTask<Tuple<HttpStatusCode, string>> RunCommandAsync(TheOneCommand command, IProgress<IDataTransferLevel> uploadProgress = null, IProgress<IDataTransferLevel> downloadProgress = null, CancellationToken cancellationToken = default);
    }
}

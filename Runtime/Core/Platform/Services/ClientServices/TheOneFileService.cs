using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using TheOneUnity.Platform.Services.ClientServices;
using TheOneUnity.Platform.Utilities;
using Cysharp.Threading.Tasks;
using TheOneUnity.Platform.Abstractions;
using TheOneUnity.Platform.Exceptions;
using TheOneUnity.Platform.Objects;
using TheOneUnity.Platform.Services.Models;

namespace TheOneUnity.Platform.Services.ClientServices
{
    public class TheOneFileService : IFileService
    {
        ITheOneCommandRunner CommandRunner { get; }

        IJsonSerializer JsonSerializer { get; }

        public TheOneFileService(ITheOneCommandRunner commandRunner, IJsonSerializer jsonSerializer) => (CommandRunner, JsonSerializer) = (commandRunner, jsonSerializer);

        public async UniTask<TheOneFileState> SaveAsync(TheOneFileState state, Stream dataStream, string sessionToken, IProgress<IDataTransferLevel> progress, CancellationToken cancellationToken = default)
        {
            if (state.url != null)
                return state;

            if (cancellationToken.IsCancellationRequested)
                return await UniTask.FromCanceled<TheOneFileState>(cancellationToken);

            long oldPosition = dataStream.Position;

            Tuple<HttpStatusCode, string> cmdResult = await CommandRunner.RunCommandAsync(new TheOneCommand($"server/files/{state.name}", method: "POST", sessionToken: sessionToken, contentType: state.mediatype, stream: dataStream), uploadProgress: progress, cancellationToken: cancellationToken);
                
            cancellationToken.ThrowIfCancellationRequested();
            TheOneFileState fileState = default;

            if (cmdResult.Item2 is { })
            {
                fileState = JsonSerializer.Deserialize<TheOneFileState>(cmdResult.Item2);

                if (String.IsNullOrWhiteSpace(fileState.name) || !(fileState.url is { }))
                    throw new TheOneFailureException(TheOneFailureException.ErrorCode.ScriptFailed, "");

                fileState.mediatype = state.mediatype;
            }
            else
                throw new TheOneFailureException(TheOneFailureException.ErrorCode.ScriptFailed, "");
        
            // Rewind the stream on failure or cancellation (if possible).
            if (dataStream.CanSeek)
                dataStream.Seek(oldPosition, SeekOrigin.Begin);

            return fileState;
        }
    }
}

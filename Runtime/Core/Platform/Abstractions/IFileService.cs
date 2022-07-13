using System;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using TheOneUnity.Platform.Objects;

namespace TheOneUnity.Platform.Abstractions
{
    public interface IFileService
    {
        UniTask<TheOneFileState> SaveAsync(TheOneFileState state, Stream dataStream, string sessionToken, IProgress<IDataTransferLevel> progress, CancellationToken cancellationToken);
    }
}

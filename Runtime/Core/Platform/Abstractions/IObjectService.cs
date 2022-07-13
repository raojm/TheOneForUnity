using System;
using System.Collections.Generic;
using System.Threading;
using TheOneUnity.Platform.Abstractions;
using Cysharp.Threading.Tasks;
using TheOneUnity.Platform.Objects;

namespace TheOneUnity.Platform.Abstractions
{
    public interface IObjectService
    {
        UniTask<T> FetchAsync<T>(T item, string sessionToken, CancellationToken cancellationToken = default) where T : TheOneObject;

        UniTask<string> SaveAsync(TheOneObject item, IDictionary<string, ITheOneFieldOperation> operations, string sessionToken, CancellationToken cancellationToken = default);

        //IList<Task<T>> SaveAllAsync<T>(IList<IObjectState> states, IList<IDictionary<string, string sessionToken, IServiceHub<TheOneUser> serviceHub, CancellationToken cancellationToken = default) where T : TheOneObject;

        UniTask DeleteAsync(TheOneObject item, string sessionToken, CancellationToken cancellationToken = default);

        //IList<Task> DeleteAllAsync<T>(IList<T> items, string sessionToken, CancellationToken cancellationToken = default) where T : TheOneObject;
    }
}

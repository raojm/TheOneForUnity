using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using TheOneUnity.Platform.Objects;
using TheOneUnity.Platform.Queries;

namespace TheOneUnity.Platform.Abstractions
{
    public interface IQueryService
    {
        IObjectService ObjectService { get; }
        //Task<IEnumerable<T>> FindAsync<T>(TheOneQuery<T> query, TheOneUser user, CancellationToken cancellationToken = default) where T : TheOneObject;

        //Task<IEnumerable<T>> AggregateAsync<T>(TheOneQuery<T> query, TheOneUser user, CancellationToken cancellationToken = default) where T : TheOneObject;

        //Task<int> CountAsync<T>(TheOneQuery<T> query, TheOneUser user, CancellationToken cancellationToken = default) where T : TheOneObject;

        //Task<T> FirstAsync<T>(TheOneQuery<T> query, TheOneUser user, CancellationToken cancellationToken = default) where T : TheOneObject;

        //Task<IEnumerable<T>> DistinctAsync<T>(TheOneQuery<T> query, TheOneUser user, CancellationToken cancellationToken = default) where T : TheOneObject;
        UniTask<IEnumerable<T>> FindAsync<T>(TheOneQuery<T> query, string sessionToken, CancellationToken cancellationToken = default) where T : TheOneObject;

        UniTask<IEnumerable<T>> AggregateAsync<T>(TheOneQuery<T> query, string sessionToken, CancellationToken cancellationToken = default) where T : TheOneObject;

        UniTask<int> CountAsync<T>(TheOneQuery<T> query, string sessionToken, CancellationToken cancellationToken = default) where T : TheOneObject;

        UniTask<T> FirstAsync<T>(TheOneQuery<T> query, string sessionToken, CancellationToken cancellationToken = default) where T : TheOneObject;

        UniTask<IEnumerable<T>> DistinctAsync<T>(TheOneQuery<T> query, string sessionToken, CancellationToken cancellationToken = default) where T : TheOneObject;


    }
}

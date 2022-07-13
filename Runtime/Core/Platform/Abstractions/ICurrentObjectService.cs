using System.Threading;
using Cysharp.Threading.Tasks;
using TheOneUnity.Platform.Objects;

namespace TheOneUnity.Platform.Abstractions
{
    public interface ICurrentObjectService <T, TUser> where T : TheOneObject where TUser : TheOneUser
    {
        /// <summary>
        /// Persists current <see cref="TheOneObject"/>.
        /// </summary>
        /// <param name="obj"><see cref="TheOneObject"/> to be persisted.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        UniTask SetAsync(T obj, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the persisted current <see cref="TheOneObject"/>.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        UniTask<T> GetAsync(IServiceHub<TUser> serviceHub, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns a <see cref="Task"/> that resolves to <code>true</code> if current
        /// <see cref="TheOneObject"/> exists.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        UniTask<bool> ExistsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns <code>true</code> if the given <see cref="TheOneObject"/> is the persisted current
        /// <see cref="TheOneObject"/>.
        /// </summary>
        /// <param name="obj">The object to check.</param>
        /// <returns>true if <code>obj</code> is the current persisted <see cref="TheOneObject"/>.</returns>
        bool IsCurrent(T obj);

        /// <summary>
        /// Nullifies the current <see cref="TheOneObject"/> from memory.
        /// </summary>
        void ClearFromMemory();

        /// <summary>
        /// Clears current <see cref="TheOneObject"/> from disk.
        /// </summary>
        UniTask ClearFromDiskAsync();
    }
}

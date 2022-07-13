using TheOneUnity.Platform.Objects;

namespace TheOneUnity.Platform.Abstractions
{
    public interface IServiceHubComposer<TUser> where TUser : TheOneUser
    {
        public IServiceHub<TUser> BuildHub(IMutableServiceHub<TUser> serviceHub = default, IServiceHub<TUser> extension = default, params IServiceHubMutator[] configurators) ;
    }
}

using TheOneUnity.Platform.Objects;

namespace TheOneUnity.Platform.Abstractions
{
    public interface ICustomServiceHub<TUser> : IServiceHub<TUser> where TUser : TheOneUser
    {
        IServiceHub<TUser> Services { get; }
    }
}

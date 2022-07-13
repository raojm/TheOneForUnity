using Cysharp.Threading.Tasks;
using TheOneUnity.SolanaApi.Models;

namespace TheOneUnity.SolanaApi.Interfaces
{
    public interface INftApi
    {
        UniTask<NftMetadata> GetNFTMetadata(NetworkTypes network, string address);
    }
}

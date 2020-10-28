using EuNet.Rpc;
using System.Threading.Tasks;

namespace StarterCommon
{
    public interface IShopRpc : IRpc
    {
        Task<int> PurchaseItem(string itemId);
        // Please add user function here
    }
}

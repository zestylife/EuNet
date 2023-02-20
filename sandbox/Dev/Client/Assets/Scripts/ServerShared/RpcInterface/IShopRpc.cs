using EuNet.Rpc;
using System.Threading.Tasks;

namespace Common
{
    public interface IShopRpc : IRpc
    {
        Task<int> PurchaseItem(string itemId);
        // Please add user function here
    }
}

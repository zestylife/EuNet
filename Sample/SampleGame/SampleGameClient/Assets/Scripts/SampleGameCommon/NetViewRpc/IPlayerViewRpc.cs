using EuNet.Rpc;
using System.Threading.Tasks;

namespace SampleGameCommon
{
    public interface IPlayerViewRpc : IViewRpc
    {
        Task OnAttack();
        Task OnSkill(byte index);
    }
}

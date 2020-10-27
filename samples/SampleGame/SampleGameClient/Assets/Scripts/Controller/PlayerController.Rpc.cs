using SampleGameCommon;
using System.Threading.Tasks;

public partial class PlayerController : IPlayerViewRpc
{
    private IPlayerViewRpc_NoReply _playerRpc;

    public Task OnAttack()
    {
        _animator.SetTrigger("Attack");
        return Task.CompletedTask;
    }

    public Task OnSkill(byte index)
    {
        throw new System.NotImplementedException();
    }
}

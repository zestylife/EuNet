using Common;
using Common.Resolvers;
using EuNet.Core;
using EuNet.Unity;
using System;
using System.Threading.Tasks;

public class GameClient : Singleton<GameClient>
{
    private NetP2pUnity _client;

    // Rpcs
    public LoginRpc LoginRpc { get; private set; }
    public ShopRpc ShopRpc { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        _client = NetP2pUnity.Instance;
        _client.Client.OnConnected = OnConnected;
        _client.Client.OnClosed = OnClosed;
        _client.Client.OnReceived = OnReceive;
        _client.Client.OnP2pGroupLeaved = OnP2pGroupLeave;

        // 릴레이 테스트를 위한 옵션
        //_client.Client.ClientOption.IsForceRelay = true;

        // 자동으로 생성된 Rpc 서비스를 사용하기 위해 등록함
        _client.AddRpcService(new ActorViewRpcServiceView());

        CustomResolver.Register(GeneratedResolver.Instance);
    }

    private void OnConnected()
    {
        LoginRpc = new LoginRpc(_client.Client, null, TimeSpan.FromSeconds(10));
        ShopRpc = new ShopRpc(_client.Client, null, TimeSpan.FromSeconds(10));
    }

    private void OnClosed()
    {
        LoginRpc = null;
        ShopRpc = null;
    }

    private Task OnReceive(NetDataReader reader)
    {
        return Task.CompletedTask;
    }

    private void OnP2pGroupLeave(ushort sessionId, bool isMine)
    {
        // 다른유저가 그룹에서 떠나면
        if (isMine == false)
        {
            // 해당 유저의 액터를 삭제한다
            ActorManager.Instance.ActorList.ForEach((Actor actor) =>
            {
                if (actor.View.OwnerSessionId == sessionId)
                {
                    NetP2pUnity.Instance.Destroy(actor.View.ViewId);
                }
            });
        }
    }
}

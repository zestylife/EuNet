using Common;
using EuNet.Core;
using EuNet.Unity;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ActorScale : MonoBehaviour , IActorScaleRpc
{
    private NetView _view;
    public NetView View => _view;
    private ActorScaleRpc _actorScaleRpc;

    private void Awake()
    {
        _view = GetComponent<NetView>();
        _actorScaleRpc = new ActorScaleRpc(_view);
    }

    private void Update()
    {
        if(_view.IsMine())
        {
            if(Input.GetKeyDown(KeyCode.Space))
            {
                var scale = new Vector3(2, 2, 2);
                _actorScaleRpc.ToOthers(DeliveryMethod.ReliableOrdered).OnSetScale(scale);
                OnSetScale(scale);
            }
        }
    }

    public Task OnSetScale(Vector3 scale)
    {
        transform.localScale = scale;
        return Task.CompletedTask;
    }
}

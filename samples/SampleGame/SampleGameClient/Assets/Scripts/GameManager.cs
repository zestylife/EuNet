using Cysharp.Threading.Tasks;
using EuNet.Core;
using EuNet.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecutionOrder(-9)]
public class GameManager : SceneSingleton<GameManager> , INetViewHandler
{
    private NetView _view;

    protected override void Awake()
    {
        base.Awake();

        _view = GetComponent<NetView>();
    }

    private void Start()
    {
        if(NetP2pUnity.Instance.MasterIsMine())
        {
            var playerObj = NetP2pUnity.Instance.Instantiate("Player", new Vector3(-28, 0, -7), Quaternion.Euler(0, 436, 0));
            ActorManager.Instance.ControlActor = playerObj.GetComponent<Actor>();
        }
        else
        {
            RecoveryAsync().Forget();
        }
    }

    public async UniTaskVoid RecoveryAsync()
    {
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(1));
            await NetP2pUnity.Instance.RequestRecovery();

            var playerObj = NetP2pUnity.Instance.Instantiate("Player", new Vector3(-28, 0, -7), Quaternion.Euler(0, 436, 0));
            ActorManager.Instance.ControlActor = playerObj.GetComponent<Actor>();
        }
        catch(System.Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    public void OnViewInstantiate(NetDataReader reader)
    {
        throw new System.NotImplementedException();
    }

    public void OnViewDestroy(NetDataReader reader)
    {
        throw new System.NotImplementedException();
    }

    public void OnViewMessage(NetDataReader reader)
    {
        
    }
}

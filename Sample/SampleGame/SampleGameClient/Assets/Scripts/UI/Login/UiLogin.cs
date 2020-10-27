using Cysharp.Threading.Tasks;
using EuNet.Core;
using EuNet.Unity;
using SampleGameCommon;
using System;
using UnityEngine;

public class UiLogin : MonoBehaviour
{
    private void Awake()
    {
        NetP2pUnity.Instance.AddRpcService(new PlayerViewRpcServiceView());
    }

    public void OnClickConnect()
    {
        ConnectAsync().Forget();
    }

    private async UniTaskVoid ConnectAsync()
    {
        var result = await NetP2pUnity.Instance.ConnectAsync(TimeSpan.FromSeconds(10));

        if(result == true)
        {
            LoginRpc loginRpc = new LoginRpc(NetP2pUnity.Instance.Client);
            var loginResult = await loginRpc.Login("AuthedId", null);

            Debug.Log($"Login Result : {loginResult}");
            if (loginResult != 0)
                return;

            var userInfo = await loginRpc.GetUserInfo();
            Debug.Log($"UserName : {userInfo.Name}");

            var writer = NetPool.DataWriterPool.Alloc();
            try
            {
                writer.Write("join");
                NetP2pUnity.Instance.SendAsync(writer, DeliveryMethod.Tcp);
            }
            finally
            {
                NetPool.DataWriterPool.Free(writer);
            }
        }
        else
        {
            Debug.LogError("Fail to connect server");
        }
    }
}

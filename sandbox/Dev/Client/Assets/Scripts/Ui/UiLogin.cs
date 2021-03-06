﻿using Common;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using EuNet.Unity;
using System;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UiLogin : MonoBehaviour
{
    [SerializeField] private Button _loginButton;

    private void Awake()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    private void Start()
    {
        _loginButton.OnClickAsAsyncEnumerable().Subscribe(OnLoginAsync);
            
    }

    private async UniTaskVoid OnLoginAsync(AsyncUnit asyncUnit)
    {
        try
        {
            _loginButton.interactable = false;
            var result = await NetClientGlobal.Instance.Client.ConnectAsync(TimeSpan.FromSeconds(10));

            if (result == true)
            {
                LoginRpc loginRpc = new LoginRpc(NetClientGlobal.Instance.Client);
                var loginResult = await loginRpc.Login(SystemInfo.deviceUniqueIdentifier);

                Debug.Log($"Login Result : {loginResult}");
                if (loginResult != 0)
                    return;

                var userInfo = await loginRpc.GetUserInfo();
                Debug.Log($"UserName : {userInfo.Name}");

                var joinResult = await loginRpc.Join();
                Debug.Log($"Join : {joinResult}");

                var purchaseResult = await GameClient.Instance.ShopRpc.PurchaseItem("TestItem");
                Debug.Log($"purchaseResult : {purchaseResult}");
                if (purchaseResult != 1)
                    throw new Exception("Fail to PurchaseItem");

                // Game Scene 으로 이동
                await SceneManager.LoadSceneAsync("Game");
            }
            else
            {
                Debug.LogError("Fail to connect server");
            }
        }
        catch(Exception ex)
        {
            Debug.LogException(ex);
        }
        finally
        {
            if(_loginButton != null)
                _loginButton.interactable = true;
        }
    }
}

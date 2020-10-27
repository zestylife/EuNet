using EuNet.Core;
using EuNet.Unity;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Network : MonoBehaviour
{
    private NetP2pUnity _client;

    private void Awake()
    {
        _client = NetP2pUnity.Instance;
        _client.Client.OnReceived += OnReceive;
    }

    private Task OnReceive(NetDataReader reader)
    {
        var type = reader.ReadString();

        switch(type)
        {
            case "join":
                {
                    SceneManager.LoadScene("Game");
                }
                break;
        }
        return Task.CompletedTask;
    }
}

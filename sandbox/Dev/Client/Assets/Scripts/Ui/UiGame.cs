using EuNet.Unity;
using System;
using System.Text;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

public class UiGame : MonoBehaviour
{
    [SerializeField] private Text _playerCountText;
    
    private void Start()
    {
        gameObject
            .AddComponent<ObservableUpdateTrigger>()
            .UpdateAsObservable()
            .Sample(TimeSpan.FromMilliseconds(500))
            .Subscribe(OnUpdateUI);
    }

    private void OnUpdateUI(Unit unit)
    {
        StringBuilder builder = new StringBuilder();

        builder.AppendLine($"Player : {NetClientGlobal.Instance.Client.P2pGroup.MemberList.Count}");
        foreach (var member in NetClientGlobal.Instance.Client.P2pGroup.MemberList)
        {
            var udpChannel = member.Session.UdpChannel;
            if(member.SessionId == NetClientGlobal.Instance.Client.SessionId)
                builder.Append($"[Myself] ");
            else builder.Append($"[{member.State}] ");

            builder.AppendLine($"Ping [{member.Session.UdpChannel.Ping}ms] Mtu[{member.Session.UdpChannel.Mtu}] L[{udpChannel.LocalEndPoint}] R[{udpChannel.RemoteEndPoint}] T[{udpChannel.TempEndPoint}] P[{udpChannel.PunchedEndPoint}]");
        }

        _playerCountText.text = builder.ToString();
    }
}

using EuNet.Unity;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class UiGame : MonoBehaviour
{
    [SerializeField] private Text _playerCountText;
    
    private void Start()
    {
        
    }

    private void Update()
    {
        StringBuilder builder = new StringBuilder();

        builder.AppendLine($"Player : {NetClientGlobal.Instance.Client.P2pGroup.MemberList.Count}");
        foreach (var member in NetClientGlobal.Instance.Client.P2pGroup.MemberList)
        {
            var udpChannel = member.Session.UdpChannel;
            if(member.SessionId == NetClientGlobal.Instance.Client.SessionId)
                builder.AppendLine($"[Myself] Local[{udpChannel.LocalEndPoint}] Remote[{udpChannel.RemoteEndPoint}] Temp[{udpChannel.TempEndPoint}]");
            else builder.AppendLine($"[{member.State}] Local[{udpChannel.LocalEndPoint}] Remote[{udpChannel.RemoteEndPoint}] Temp[{udpChannel.TempEndPoint}]");
        }

        _playerCountText.text = builder.ToString();
    }
}

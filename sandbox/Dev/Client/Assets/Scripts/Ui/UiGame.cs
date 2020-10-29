using EuNet.Unity;
using System.Collections;
using System.Collections.Generic;
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
        _playerCountText.text = $"Player : {NetP2pUnity.Instance.Client.P2pGroup.MemberList.Count}";
    }
}

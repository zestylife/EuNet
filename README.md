# EuNet C# (.NET, .NET Core, Unity)

Easy Unity Network(EuNet)은 멀티플레이 게임을 위한 네트워크 솔루션입니다.

TCP, UDP, RUDP 프로토콜을 이용하여 Server-Client, Peer to Peer 통신을 지원합니다.

P2P(Peer to Peer)의 경우에는 홀펀칭(Hole Punching)을 지원하여 최대한 직접 통신을 시도하며, 불가능할 경우 자동으로 서버를 통해 Relay 되어 잘 무사히 전달됩니다.

Action MORPG, MOBA, Channel Based MMORPG, Casual Multiplayer Game (예를 들어 League of Legends, Among Us, Kart Rider, Diablo 등) 을 개발하는데 적합합니다.

.Net Standard 2.0 을 기반으로 제작되었으므로 멀티플랫폼(Windows, Linux, Android, iOS 등)이 지원되며, .Net Core 기반의 서버와 Unity3D 기반 클라이언트에 최적화되어 개발되었습니다.

## Table of Contents

- [EuNet C# (.NET, .NET Core, Unity)](#eunet-c-net-net-core-unity)
  - [Table of Contents](#table-of-contents)
  - [Features](#features)
  - [Channels](#channels)
  - [Installation](#installation)
    - [NuGet packages](#nuget-packages)
    - [Unity](#unity)
  - [Quick Start](#quick-start)
    - [.Net Core Server using RPC](#net-core-server-using-rpc)
    - [Unity Client using RPC](#unity-client-using-rpc)

## Features
  
* 가볍고 빠른 네트워크 기능
  * 멀티스레드를 이용한 고속 통신
  * 풀링 버퍼를 통한 고속 할당
* 지원되는 채널
  * TCP
  * Unreliable UDP
    * 가장 빠르고 가볍지만 소실되며 중복이 되거나 순서가 보장되지 않는  채널
  * Reliable Ordered UDP
    * 신뢰성있고 중복되지 않으며 순서가 보장되는 채널
  * Reliable Unordered UDP
    * 신뢰성있고 중복되지 않으며 순서가 보장되지 않는 채널
  * Reliable Sequenced UDP
    * 마지막 패킷만 소실되지 않고 순서가 보장되며 중복되지 않는 채널
  * Sequenced UDP
    * 소실되지만 순서가 보장되며 중복되지 않는 채널
* 지원되는 통신
  * Client to Server
  * Peer to Peer
    * Hole Punching
    * Relay (Auto Switching)
* RPC (Remote Procedure Call)
  * 간편하게 원격지의 함수를 호출하는 기능 
* 고속 패킷 시리얼라이저 (MessagePack for C#)
* 고속 Serialize 및 RPC 를 위한 컴파일러 내장
* 자동으로 MTU 체크
* MTU보다 큰 UDP 패킷의 자동 분해 및 조립
* 작은 UDP 패킷을 모아서 부하를 줄임
* Unity3D 지원 (.Net Standard 2.0 기반)

## Channels

|          채널          |             전송보장              |      중복안됨      |      순서보장      |
| :--------------------: | :-------------------------------: | :----------------: | :----------------: |
|          TCP           |        :heavy_check_mark:         | :heavy_check_mark: | :heavy_check_mark: |
|     Unreliable UDP     |                :x:                |        :x:         |        :x:         |
|  Reliable Ordered UDP  |        :heavy_check_mark:         | :heavy_check_mark: | :heavy_check_mark: |
| Reliable Unordered UDP |        :heavy_check_mark:         | :heavy_check_mark: |        :x:         |
| Reliable Sequenced UDP | :heavy_check_mark:(마지막 패킷만) | :heavy_check_mark: | :heavy_check_mark: |
|     Sequenced UDP      |                :x:                | :heavy_check_mark: | :heavy_check_mark: |

.

## Installation

### NuGet packages

### Unity

## Quick Start

### .Net Core Server using RPC
```csharp
// 유저세션 클래스에서 Rpc Interface (ILoginRpc)를 상속
public partial class UserSession : ILoginRpc
{
    private UserInfo _userInfo = new UserInfo();
    
    public Task<int> Login(string id, ISession session)
    {
        if (id == "AuthedId")
            return Task<int>.FromResult(0);

        return Task<int>.FromResult(1);
    }

    public Task<UserInfo> GetUserInfo()
    {
        // 유저정보를 입력 (추후 DB를 통해서 가져오면 됩니다)
        _userInfo.Name = "abc";

        return Task<UserInfo>.FromResult(_userInfo);
    }
}
```

### Unity Client using RPC
```csharp
private async UniTaskVoid ConnectAsync()
{
    // 접속을 시도함. 타임아웃은 10초
    var result = await NetP2pUnity.Instance.ConnectAsync(TimeSpan.FromSeconds(10));

    if(result == true)
    {
        // 로그인 Rpc 객체를 생성
        LoginRpc loginRpc = new LoginRpc(NetP2pUnity.Instance.Client);

        // 서버의 로그인 함수를 호출함
        var loginResult = await loginRpc.Login("AuthedId", null);

        Debug.Log($"Login Result : {loginResult}");
        if (loginResult != 0)
            return;
        
        // 서버의 유저정보를 가져옴
        var userInfo = await loginRpc.GetUserInfo();
        Debug.Log($"UserName : {userInfo.Name}");
    }
    else
    {
        // 접속 실패
        Debug.LogError("Fail to connect server");
    }
}
```
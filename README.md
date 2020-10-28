# EuNet C# (.NET, .NET Core, Unity)

Easy Unity Network (EuNet) is a network solution for multiplayer games.

Supports Server-Client, Peer to Peer communication using TCP, UDP, and RUDP protocols.

In the case of P2P (Peer to Peer), supports hole punching and tries to communicate directly as much as possible, and if it is impossible, automatically relayed through the server.

Great for developing Action MORPG, MOBA, Channel Based MMORPG, Casual Multiplayer Game (e.g. League of Legends, Among Us, Kart Rider, Diablo, etc.).

Produced based on .Net Standard 2.0, multiplatform supported(Windows, Linux, Android, iOS, etc.), and is optimized for .Net Core-based servers and Unity3D-based clients.

## Table of Contents

- [EuNet C# (.NET, .NET Core, Unity)](#eunet-c-net-net-core-unity)
  - [Table of Contents](#table-of-contents)
  - [Features](#features)
  - [Channels](#channels)
  - [Installation](#installation)
    - [Common project](#common-project)
    - [Server project (.net core)](#server-project-net-core)
    - [Client project (Unity3D)](#client-project-unity3d)
  - [Rpc Sample](#rpc-sample)
    - [Common project](#common-project-1)
    - [Server project (.Net Core)](#server-project-net-core-1)
    - [Client project (Unity3D)](#client-project-unity3d-1)
  - [Quick Start](#quick-start)
  - [Unity3D](#unity3d)
  - [IL2CPP issue (AOT)](#il2cpp-issue-aot)

## Features
  
* Fast network communication
  * High speed communication using multi-thread
  * Fast allocation using pooling buffer
* Supported channels
  * TCP
  * Unreliable UDP
  * Reliable Ordered UDP
  * Reliable Unordered UDP
  * Reliable Sequenced UDP
  * Sequenced UDP
* Supported communication
  * Client to Server
  * Peer to Peer
    * Hole Punching
    * Relay (Auto Switching)
* RPC (Remote Procedure Call)
* Fast packet serializer (Partial using MessagePack for C#)
* Custom Compiler(EuNetCodeGenerator) for fast serializing and RPC
* Automatic MTU detection
* Automatic fragmentation of large UDP packets
* Automatic merging small packets 
* Unity3D support
* Supported platforms
  * Windows / Mac / Linux (.Net Core)
  * Android (Unity)
  * iOS (Unity)

## Channels

|        Channels        |     Transmission guarantee     |   Not duplicate    |  Order guarantee   |
| :--------------------: | :----------------------------: | :----------------: | :----------------: |
|          TCP           |       :heavy_check_mark:       | :heavy_check_mark: | :heavy_check_mark: |
|     Unreliable UDP     |              :x:               |        :x:         |        :x:         |
|  Reliable Ordered UDP  |       :heavy_check_mark:       | :heavy_check_mark: | :heavy_check_mark: |
| Reliable Unordered UDP |       :heavy_check_mark:       | :heavy_check_mark: |        :x:         |
| Reliable Sequenced UDP | :heavy_check_mark:(Last order) | :heavy_check_mark: | :heavy_check_mark: |
|     Sequenced UDP      |              :x:               | :heavy_check_mark: | :heavy_check_mark: |

***

## Installation

We need three projects
* Common project (.Net Standard 2.0)
  * Server, Client common use
  * Generate code using EuNetCodeGenerator
* Server project (.Net Core)
* Client project (Unity3D)

See [EuNet-Starter](https://github.com/zestylife/EuNet-Starter) for an example

### Common project

* Create .Net Standard 2.0 based project.
* Install nuget package.

```
PM> Install-Package EuNet.CodeGenerator.Templates
```
* Rebuild project.
* If you look at the project, `CodeGen/EuNet.Rpc.CodeGen.cs` file was created.

### Server project (.net core)

* First install the nuget package.
```
PM> Install-Package EuNet
```
* Add common project to reference
```
Solution Explorer -> [User Project] -> References -> Add Reference -> [Add Common project]
```
* Write server code. [Server Code Sample]()
* Write session code. [Session Code Sample]()

### Client project (Unity3D)

* First install the unity-package. [Download here]()

## Rpc Sample

### Common project
```csharp
// Declaring login rpc interface
public interface ILoginRpc : IRpc
{
    Task<int> Login(string id, ISession session);
    Task<UserInfo> GetUserInfo();
}
// Generate Rpc code using EuNetCodeGenerator and use it in server and client
```

### Server project (.Net Core)
```csharp
// User session class inherits Rpc Interface (ILoginRpc)
public partial class UserSession : ILoginRpc
{
    private UserInfo _userInfo = new UserInfo();
    
    // Implement Rpc Method that client calls
    public Task<int> Login(string id, ISession session)
    {
        if (id == "AuthedId")
            return Task<int>.FromResult(0);

        return Task<int>.FromResult(1);
    }

    // Implement Rpc Method that client calls
    public Task<UserInfo> GetUserInfo()
    {
        // Set user information
        _userInfo.Name = "abc";

        return Task<UserInfo>.FromResult(_userInfo);
    }
}
```

### Client project (Unity3D)
```csharp
private async UniTaskVoid ConnectAsync()
{
    // Trying to connect. Timeout is 10 seconds.
    var result = await NetP2pUnity.Instance.ConnectAsync(TimeSpan.FromSeconds(10));

    if(result == true)
    {
        // Create an object for calling login Rpc
        LoginRpc loginRpc = new LoginRpc(NetP2pUnity.Instance.Client);

        // Call the server's login function (UserSession.Login)
        var loginResult = await loginRpc.Login("AuthedId", null);

        Debug.Log($"Login Result : {loginResult}");
        if (loginResult != 0)
            return;
        
        // Call the server's get user information function (UserSession.GetUserInfo)
        var userInfo = await loginRpc.GetUserInfo();
        Debug.Log($"UserName : {userInfo.Name}");
        // UserName : abc
    }
    else
    {
        // Fail to connect
        Debug.LogError("Fail to connect server");
    }
}
```

## Quick Start

## Unity3D

## IL2CPP issue (AOT)
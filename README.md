EuNet C# (.NET, .NET Core, Unity)
===
[![GitHub Actions](https://github.com/zestylife/EuNet/workflows/Build-Debug/badge.svg)](https://github.com/zestylife/EuNet/actions) [![GitHub Actions](https://github.com/zestylife/EuNet/workflows/Build-Release/badge.svg)](https://github.com/zestylife/EuNet/actions) [![nuget](https://img.shields.io/nuget/dt/EuNet.svg)](https://www.nuget.org/packages/EuNet/) [![Releases](https://img.shields.io/github/v/release/zestylife/EuNet)](https://github.com/zestylife/EuNet/releases)

Easy Unity Network (EuNet) is a network solution for multiplayer games.

Supports Server-Client, Peer to Peer communication using TCP, UDP, and RUDP protocols.

In the case of P2P (Peer to Peer), supports hole punching and tries to communicate directly as much as possible, and if it is impossible, automatically relayed through the server.

Great for developing Action MORPG, MOBA, Channel Based MMORPG, Casual Multiplayer Game (e.g. League of Legends, Among Us, Kart Rider, Diablo, etc.).

Produced based on .Net Standard 2.0, multiplatform supported(Windows, Linux, Android, iOS, etc.), and is optimized for .Net Core-based servers and Unity3D-based clients.

RPC(Remote procedure call) can be used to call remote functions and receive return values.  
There is no overhead as it serializes at high speed and calls remote functions.  
Work efficiency increases as there is no work to create a message every time.  

## See [EuNet-Starter](https://github.com/zestylife/EuNet-Starter) for an example

![image](https://github.com/zestylife/EuNet/blob/main/doc/images/Network.png?raw=true)

## Table of Contents

- [EuNet C# (.NET, .NET Core, Unity)](#eunet-c-net-net-core-unity)
  - [See EuNet-Starter for an example](#see-eunet-starter-for-an-example)
  - [Table of Contents](#table-of-contents)
  - [Features](#features)
  - [Channels](#channels)
  - [Installation](#installation)
    - [Common project](#common-project)
    - [Server project (.net core)](#server-project-net-core)
    - [Client project (Unity3D)](#client-project-unity3d)
    - [Install via git URL](#install-via-git-url)
    - [Install via package file](#install-via-package-file)
  - [RPC (Remote procedure call)](#rpc-remote-procedure-call)
    - [Common project](#common-project-1)
    - [Server project (.Net Core)](#server-project-net-core-1)
    - [Client project (Unity3D)](#client-project-unity3d-1)
  - [Quick Start](#quick-start)
  - [Serialize](#serialize)
    - [Using Auto-Generated formmater](#using-auto-generated-formmater)
    - [Manualy serialize](#manualy-serialize)
  - [Unity3D](#unity3d)
    - [Settings](#settings)
    - [How to use Rpc](#how-to-use-rpc)
    - [How to use ViewRpc](#how-to-use-viewrpc)
  - [IL2CPP issue (AOT)](#il2cpp-issue-aot)
    - [Serialization](#serialization)

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

### Install via git URL

After Unity 2019.3.4f1, Unity 2020.1a21, that support path query parameter of git package.
You can add package from UPM (Unity Package Manager)
```
https://github.com/zestylife/EuNet.git?path=src/EuNet.Unity/Assets/Plugins/EuNet
``` 
If you want to add a specific release version, add `#version` after the url. ex) version 1.1.13
```
https://github.com/zestylife/EuNet.git?path=src/EuNet.Unity/Assets/Plugins/EuNet#1.1.13
``` 

![image](https://github.com/zestylife/EuNet/blob/main/doc/images/AddPackageFromUPM.png?raw=true)

### Install via package file
* Install the unity-package. [Download here](https://github.com/zestylife/EuNet/releases)

## RPC (Remote procedure call)

RPC(Remote procedure call) can be used to call remote functions and receive return values.  
There is no overhead as it serializes at high speed and calls remote functions.  
Work efficiency increases as there is no work to create a message every time.  

![image](https://github.com/zestylife/EuNet/blob/main/doc/images/Rpc.png?raw=true)

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
    var client = NetClientGlobal.Instance.Client;

    // Trying to connect. Timeout is 10 seconds.
    var result = await client.ConnectAsync(TimeSpan.FromSeconds(10));

    if(result == true)
    {
        // Create an object for calling login Rpc
        LoginRpc loginRpc = new LoginRpc(client);

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

## Serialize

Object serialization is required to use Rpc.
There are two ways to serialize objects.

### Using Auto-Generated formmater

Declaring the `NetDataObject` Attribute makes the class serializable.
All public objects are serialized.
Declaring the `[IgnoreMember]` Attribute does not serialize it.

```csharp
[NetDataObject]
public class DataClass
{
    // Serializable
    public int Int;

    // Serializable
    public int Property { get; set; }

    // Ignore
    public int PropertyOnlyGet { get; }

    // Ignore
    private int IntPrivate;

    // Ignore
    protected int IntProtected;
    
    // Ignore
    [IgnoreMember]
    public int IgnoreInt;

    // Ignore
    [IgnoreMember]
    public int IgnoreProperty { get; set; }
}
```

### Manualy serialize

Implement serialization manually by inheriting `INetSerializable`.
You have to code, but it's the fastest and most flexible.

```csharp
public class InterfaceSerializeClass : INetSerializable
{
    public int Value;
    public string Name;

    public void Serialize(NetDataWriter writer)
    {
        writer.Write(Value);
        writer.Write(Name);
    }

    public void Deserialize(NetDataReader reader)
    {
        Value = reader.ReadInt32();
        Name = reader.ReadString();
    }
}
```

## Unity3D

* Special object NetView is supported, and synchronization and Rpc communication between NetViews are possible.   
(NetClientP2pBehaviour required. Peer to Peer only)
* Supported global settings with NetClientGlobal object. (Singleton)
* Supported for communication to the server (NetClientBehaviour, NetClientP2pBehaviour)
* Support NetClientP2pBehaviour only one.

### Settings

* Add an empty GameObject to the first run Scene and add a NetClientGlobal component.   
Only one should be made globally.

![image](https://github.com/zestylife/EuNet/blob/main/doc/images/NetClientGlobal.png?raw=true)

* Add NetClientP2pBehaviour component for communicate to one server (including P2p).  
Modify the options as needed.

![image](https://github.com/zestylife/EuNet/blob/main/doc/images/NetClientP2pBehaviour.png?raw=true)

* Add user component to receive and process events.

![image](https://github.com/zestylife/EuNet/blob/main/doc/images/GameClient.png?raw=true)

* `GameClient.cs` file
```csharp
using Common.Resolvers;
using EuNet.Core;
using EuNet.Unity;
using System.Threading.Tasks;

public class GameClient : Singleton<GameClient>
{
    private NetClientP2pBehaviour _client;

    public NetClientP2p Client => _client.ClientP2p;

    protected override void Awake()
    {
        base.Awake();

        _client = GetComponent<NetClientP2pBehaviour>();

        Client.OnConnected = OnConnected;
        Client.OnClosed = OnClosed;
        Client.OnReceived = OnReceive;
        
        // Register automatically generated resolver.
        //CustomResolver.Register(GeneratedResolver.Instance);

        // If you generated RpcService, register it.
        //Client.AddRpcService(new GameScRpcService());
    }

    public Task<bool> ConnectAsync()
    {
        // Try to connect server. All functions can be accessed with Client instance
        return Client.ConnectAsync(TimeSpan.FromSeconds(10));
    }

    private void OnConnected()
    {
        // Connected
    }

    private void OnClosed()
    {
        // Disconnected

    private Task OnReceive(NetDataReader reader)
    {
        // Received data. No need to use when using RPC
        return Task.CompletedTask;
    }
}
```

### How to use Rpc

Rpc is service that can call remote procedures.   
EuNet's Rpc is a function call service between the server and the client.   
When you declare an interface that inherits the IRpc interface, calls and service codes are automatically generated.

* Create Rpc Interface in `Common` project.
* Build project.
  
```csharp
using EuNet.Rpc;
using System.Threading.Tasks;

namespace Common
{
    // Inherit IRpc for Rpc
    public interface IGameCsRpc : IRpc
    {
        // Login Rpc
        Task<int> Login(string id);
    }
}
```

* In the `Server` project, register RpcService when creating a server .

```csharp
_server.AddRpcService(new GameCsRpcServiceSession());
```

* In the `Server` project, `UserSession` class inherits from `IGameCsRpc`.
```csharp
public partial class UserSession : IGameCsRpc
{
    public Task<int> Login(string id)
    {
        return Task.FromResult(0);
    }
}
```

* In the `Client` project, call Rpc.

```csharp
// Rpc callable object
var rpc = new GameCsRpc(_client.Client, null, TimeSpan.FromSeconds(10));

// Call Rpc Login
var loginResult = await rpc.Login("MyId");
Debug.Log(loginResult);
```


### How to use ViewRpc

ViewRpc is a technology that makes peer-to-peer communication between NetView Components as Rpc.   
By adding a NetView Component to the GameObject, you can call functions of the same NetView Component (same ViewId) that exist on different clients.   
For example, if you shoot a cannon from a red tank, the other user's red tank will also fire.   
1:1 or 1:N call is possible, and in case of 1:N, return value can not be received.

## IL2CPP issue (AOT)

Some platforms do not allow runtime code generation. Therefore, any managed code which depends upon just-in-time (JIT) compilation on the target device will fail. Instead, you need to compile all of the managed code ahead-of-time (AOT). Often, this distinction doesn’t matter, but in a few specific cases, AOT platforms require additional consideration.

See more   
https://docs.unity3d.com/2019.4/Documentation/Manual/ScriptingRestrictions.html

### Serialization

There is a problem when serializing generic objects as AOT cannot generate code
So, you need to provide a hint so that AOT can generate the code.

* Class for serialize (In `Common` project)
```csharp
[NetDataObject]
public class DataClass
{
    public Tuple<int,string> TupleData;
    public Dictionary<int,string> DictionaryData;
}
```

* Hint function (In `Client` unity project)
```csharp
private void UsedOnlyForAOTCodeGeneration()
{
    // Hints for using <int,string> in TupleFormatter<T,T>
    new TupleFormatter<int, string>();

    // Hints for using <int,string> in DictionaryFormatter<T,T>
    new DictionaryFormatter<int, string>();

    // Exception!
    throw new InvalidOperationException("This method is used for AOT code generation only. Do not call it at runtime.");
}
```
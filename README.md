# SocketLib

This is a net socket library for game network, it is based on c#. In this project, there is a project which names RXNet, this is the core of the socket library.

## Classes

| Clases    |
| --------- |
| RXMsg     |
| RXSocket  |
| RXSession |
| RXTool    |
| RXPkg     |

There are 5 classes in this project, `RXMsg` `RXMsg` `RXPkg` `RXSocket` and `RxTool` . `RXSocket` is the entry of this lib, it is a generic class, you can use methods to start a server or a client. In `RXSesion` . In `RXSession` , there are some receive method, all off them are `async` method, and they support receiving message separately. In addition, there are some sending method. `RXPkg` is a data structure for message. it contains the. part of body and head. `RxTool` gives some tools, such as `SerializeData` and `DeSerializeData` , and it also has a log tool for developer.

## RXSocket

| Method                                          | Detail                                                       |
| ----------------------------------------------- | :----------------------------------------------------------- |
| StartASClient(string ip, int port)              | Build a client instance, it has two parameter, ip and port number. |
| StartAsServer(string ip, int port, int backLog) | Build a server instance, it needs ip, port and a time for listening. |
| ReturnSession()                                 | Return all of sessions which the server has.                 |
| CloseClient()                                   | Close Client, and release all resource for client.           |
| CloseServer()                                   | Close Server, and release all resource for server .          |

It can be use just like :

```c#
RXSocket<ClientSession, NetMsg> client = new RXSocket<ClientSession, NetMsg>();
```

Note that: 

```c#
RXSocket<T, k> where T : RXSession<k>, new() where k : RXMsg
```

## RXSession

| Method                                       | Detail                                                       |
| -------------------------------------------- | ------------------------------------------------------------ |
| StartRcvData(Socket socket, Action callback) | Receiving data from client or server, you can define callback in this method. Callback will be implemented in this step. |
| ASyncHeadRcv(IAsyncResult result)            | Receive head of message.                                     |
| ASyncBodyRcv(IAsyncResult result)            | Receive body of message.                                     |
| SendMsg<K>(K msg)                            | Sending a message, the parameter is an object.               |
| SendMsg(byte[] msg)                          | Sending a message, the parameter is a byte stream.           |
| CloseSession()                               | Close a session.                                             |
| Virtual OnConnectSuccess()                   | You can override this function, it is processed in StartRcvData. |
| Virtual OnConnectClose()                     | You can override this function, it is processed in CloseSession. |
| OnHandleMessage(RXMsg msg)                   | You can override this function, it is processed in ASyncBodyRcv. |


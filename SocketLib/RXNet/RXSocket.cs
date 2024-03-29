﻿using System.Net;
using System.Net.Sockets;

namespace RXNet;

public class RXSocket<T, k> where T : RXSession<k>, new() where k : RXMsg
{
    private Socket socket;
    public T session;
    private List<T> sessionList;

    public RXSocket()
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    }

    #region AsClient

    public void StartASClient(string ip, int port)
    {
        try
        {
            RXTool.Log("Client Start....", LogEnum.None);
            socket.BeginConnect(new IPEndPoint(IPAddress.Parse(ip), port), ConnectionHandle, socket);
        }
        catch (Exception e)
        {
            RXTool.Log(e.Message, LogEnum.Error);
        }

    }

    public void ConnectionHandle(IAsyncResult result)
    {
        try
        {
            Socket skt = (Socket)result.AsyncState;
            skt.EndConnect(result);
            RXTool.Log("Connect Success! Current Thread is: " + Thread.CurrentThread.ManagedThreadId, LogEnum.None);
            session = new T();
            session.StartRcvData(skt, null);
        }
        catch (Exception e)
        {
            RXTool.Log(e.Message, LogEnum.Error);
        }
    }
    #endregion

    #region AsServer
    public void StartAsServer(string ip, int port, int backLog)
    {
        try
        {
            RXTool.Log("Server Start...", LogEnum.None);
            socket.Bind(new IPEndPoint(IPAddress.Parse(ip), port));
            socket.Listen(backLog);
            sessionList = new List<T>();
            socket.BeginAccept(ServerConnectionHandle, socket);
        }
        catch (Exception e)
        {
            RXTool.Log(e.Message, LogEnum.Error);
        }
    }

    public void ServerConnectionHandle(IAsyncResult result)
    {
        Socket skt = (Socket)result.AsyncState;
        try
        {
            RXTool.Log("Connect Success! Current Thread is: " + Thread.CurrentThread.ManagedThreadId, LogEnum.None);
            Socket clientSkt = skt.EndAccept(result);
            session = new T();
            sessionList.Add(session);
            session.StartRcvData(clientSkt, () =>
            {
                if (sessionList.Contains(session))
                    sessionList.Remove(session);
            });
            skt.BeginAccept(ServerConnectionHandle, socket);
        }
        catch (Exception e)
        {
            RXTool.Log(e.Message, LogEnum.Error);
        }
    }

    public List<T> ReturnSession()
    {
        return sessionList;
    }

    public void CloseClient()
    {
        if (session != null)
        {
            session.CloseSession();
            session = null;
        }
        if (socket != null)
        {
            socket = null;
        }
    }

    public void CloseServer()
    {
        for (int i = 0; i < sessionList.Count; i++)
        {
            sessionList[i].CloseSession();
        }

        sessionList = null;
        if (socket != null)
        {
            socket.Close();
            socket = null;
        }
    }
    #endregion
}

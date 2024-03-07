using System.Net;
using System.Net.Sockets;

namespace RXNet;

public class RXSocket<T,k> where T : RXSession<k>, new() where k : RXMsg
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
            Console.WriteLine("Client Start....");
            socket.BeginConnect(new IPEndPoint(IPAddress.Parse(ip), port), ConnectionHandle, socket);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }

    }

    public void ConnectionHandle(IAsyncResult result)
    {
        try
        {
            Socket skt = (Socket)result.AsyncState;
            skt.EndConnect(result);
            Console.WriteLine("Connect Success! Current Thread is: " + Thread.CurrentThread.ManagedThreadId);
            session = new T();
            session.StartRcvData(skt, null);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }
    #endregion

    #region AsServer
    public void StartAsServer(string ip, int port, int backLog)
    {
        try
        {
            Console.WriteLine("Server Start...");
            socket.Bind(new IPEndPoint(IPAddress.Parse(ip), port));
            socket.Listen(backLog);
            sessionList = new List<T>();
            socket.BeginAccept(ServerConnectionHandle, socket);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

    public void ServerConnectionHandle(IAsyncResult result)
    {
        Socket skt = (Socket)result.AsyncState;
        try
        {
            Console.WriteLine("Connect Success! Current Thread is: " + Thread.CurrentThread.ManagedThreadId);
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
            Console.WriteLine(e.Message);
        }
    }

    public List<T> ReturnSession(){
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

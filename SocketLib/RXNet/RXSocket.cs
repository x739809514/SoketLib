using System.Net;
using System.Net.Sockets;

namespace RXNet;

public class RXSocket
{
    private Socket socket;
    private RXSession session;
    private List<RXSession> sessionList;

    public RXSocket()
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    }

    #region AsClient

    public void StartASClient(string ip, int port)
    {
        try
        {
            socket.BeginConnect(new IPEndPoint(IPAddress.Parse(ip), port), ConnectionHandle, socket);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }

    }

    private void ConnectionHandle(IAsyncResult result)
    {
        try
        {
            Socket skt = (Socket)result.AsyncState;
            skt.EndConnect(result);
            Console.WriteLine("Connect Success");
            session = new RXSession();
            session.StartRcvData(skt);
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
            socket.Bind(new IPEndPoint(IPAddress.Parse(ip), port));
            socket.Listen(backLog);
            sessionList = new List<RXSession>();
            socket.BeginAccept(ServerConnectionHandle, socket);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

    private void ServerConnectionHandle(IAsyncResult result)
    {
        Socket skt = (Socket)result.AsyncState;
        try{
            Socket clientSkt = skt.EndAccept(result);
            session = new RXSession();
            sessionList.Add(session);
            session.StartRcvData(clientSkt);
            skt.BeginAccept(ServerConnectionHandle,socket);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }
    #endregion
}

// See https://aka.ms/new-console-template for more information

using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;

class SocketObj
{
    public Socket skt;
    public string str;
}

class ClientSocketObj
{
    public Socket skt;
    public byte[] data;
}

class Server
{
    static void Main(String[] args)
    {
        Console.WriteLine("The main thread ID: " + Thread.CurrentThread.ManagedThreadId.ToString());
        CreateASyncReceive();
        //CreateBasicServer();
        Console.Read();
    }

    #region ASynclear
    static void CreateASyncReceive()
    {
        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPAddress ipAddress = new IPAddress(new byte[] { 127, 0, 0, 1 });
        EndPoint pt = new IPEndPoint(ipAddress, 12701);
        try
        {
            socket.Bind(pt);
            socket.Listen(100);
            Console.WriteLine("Server Start...");
            socket.BeginAccept(new AsyncCallback(ASyncAccept), new SocketObj() { skt = socket, str = "default" });
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    static void ASyncAccept(IAsyncResult result)
    {
        try
        {
            if (result.AsyncState is SocketObj args)
            {
                Socket clientSocket = args.skt.EndAccept(result);
                Console.WriteLine("The Receive thread ID: " + Thread.CurrentThread.ManagedThreadId.ToString());

                string msg = "Connect Successful";
                byte[] bytes = Encoding.UTF8.GetBytes(msg);
                clientSocket.Send(bytes);

                byte[] dataRcv = new byte[1024];
                clientSocket.BeginReceive(dataRcv, 0, 1024, SocketFlags.None, new AsyncCallback(ASyncDataRcv), new ClientSocketObj() { skt = clientSocket, data = dataRcv });

                // since a child thread is closed, so we need to start a new one
                args.skt.BeginAccept(new AsyncCallback(ASyncAccept), new SocketObj() { skt = args.skt, str = "default" });
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    static void ASyncDataRcv(IAsyncResult result)
    {
        try
        {
            if (result.AsyncState is ClientSocketObj args)
            {
                // receive data buffer
                byte[] dataRcv = args.data;
                int lenRcv = args.skt.EndReceive(result);
                if(lenRcv==0){
                    Console.WriteLine("Client is offline");
                    args.skt.Shutdown(SocketShutdown.Both);
                    args.skt.Close();
                    return;
                }
                Console.WriteLine("The Child thread ID: " + Thread.CurrentThread.ManagedThreadId.ToString());

                string msgRcv = Encoding.UTF8.GetString(dataRcv, 0, lenRcv);
                Console.WriteLine("Rcv Client Msg: " + msgRcv);
                // send what its receive
                byte[] rcv = Encoding.UTF8.GetBytes(msgRcv);
                args.skt.Send(rcv);

                args.skt.BeginReceive(dataRcv, 0, 1024, SocketFlags.None, new AsyncCallback(ASyncDataRcv), new ClientSocketObj() { skt = args.skt, data = dataRcv });
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }
    #endregion

    #region SyncReceive
    static void CreateBasicServer()
    {
        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPAddress ipAddress = new IPAddress(new byte[] { 127, 0, 0, 1 });
        EndPoint pt = new IPEndPoint(ipAddress, 12700);
        socket.Bind(pt);
        socket.Listen(100);
        Console.WriteLine("Server Start...");

        while (true)
        {
            Socket skt = socket.Accept();

            Thread thread = new Thread(ReveiveHandle);
            thread.Start(skt);
        }
    }

    private static void ReveiveHandle(object obj)
    {
        Socket skt = (Socket)obj;
        string msg = "Connect Successful";
        byte[] bytes = Encoding.UTF8.GetBytes(msg);
        skt.Send(bytes);

        while (true)
        {
            try
            {
                // receive data buffer
                byte[] dataRcv = new byte[1024];
                int lenRcv = skt.Receive(dataRcv);
                if (lenRcv == 0)
                {
                    Console.WriteLine("Client is quit");
                    break;
                }
                string msgRcv = Encoding.UTF8.GetString(dataRcv, 0, lenRcv);
                Console.WriteLine("Rcv Client Msg: " + msgRcv);
                // send what its receive
                byte[] rcv = Encoding.UTF8.GetBytes(msgRcv);
                skt.Send(rcv);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                break;
            }
        }
    }
    #endregion
}
// See https://aka.ms/new-console-template for more information

using System.Net;
using System.Net.Sockets;
using System.Reflection.Metadata.Ecma335;
using System.Text;

class SendObj
{
    public Socket skt;
    public EndPoint pt;
}

class ReceiveData
{
    public Socket skt;
    public byte[] dataBytes;
}

class Client
{
    static bool isCanceled = false;
    static void Main(String[] args)
    {
        ASyncConnection();
        Console.Read();
    }

    #region ASync
    static void ASyncConnection()
    {
        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPAddress ipAddress = new IPAddress(new byte[] { 127, 0, 0, 1 });
        EndPoint pt = new IPEndPoint(ipAddress, 12701);
        Console.WriteLine("Client Start...");
        try
        {
            // connection is only once
            socket.BeginConnect(pt, new AsyncCallback(ConnectionHandle), new SendObj() { skt = socket, pt = pt });
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }

        // send data
        while (true)
        {
            string msgSend = Console.ReadLine();
            if (msgSend.Length > 0)
            {
                if (msgSend.Equals("quit"))
                {
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                    isCanceled = true;
                    break;
                }
                else
                {
                    socket.Send(Encoding.UTF8.GetBytes(msgSend));
                }
            }
        }
    }

    static void ConnectionHandle(IAsyncResult result)
    {
        try
        {
            if (result.AsyncState is SendObj args)
            {
                args.skt.EndConnect(result);

                byte[] dataRcv = new byte[1024];
                args.skt.BeginReceive(dataRcv, 0, 1024, SocketFlags.None, new AsyncCallback(ReceiveDataHandle), new ReceiveData() { skt = args.skt, dataBytes = dataRcv });
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    static void ReceiveDataHandle(IAsyncResult result)
    {
        if(isCanceled) return;
        try
        {
            if (result.AsyncState is ReceiveData args)
            {
                int len = args.skt.EndReceive(result);
                if (len == 0)
                {
                    Console.WriteLine("Server is offline");
                    args.skt.Shutdown(SocketShutdown.Both);
                    args.skt.Close();
                    return;
                }
                string rcvMsg = Encoding.UTF8.GetString(args.dataBytes, 0, len);
                Console.WriteLine("Rcv Server Data: " + rcvMsg);

                args.skt.BeginReceive(args.dataBytes, 0, 1024, SocketFlags.None, new AsyncCallback(ReceiveDataHandle), new ReceiveData() { skt = args.skt, dataBytes = args.dataBytes });
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }
    #endregion

    #region Sync
    static void SyncSendData()
    {
        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPAddress ipAddress = new IPAddress(new byte[] { 127, 0, 0, 1 });
        EndPoint pt = new IPEndPoint(ipAddress, 12700);

        socket.Connect(pt);
        Console.WriteLine("Client Start...");

        while (true)
        {
            byte[] dataRcv = new byte[1024];
            int len = socket.Receive(dataRcv);

            string rcvMsg = Encoding.UTF8.GetString(dataRcv, 0, len);
            Console.WriteLine("Rcv Server Data: " + rcvMsg);

            while (true)
            {
                string msgSend = Console.ReadLine();
                if (msgSend.Length > 0)
                {
                    if (msgSend.Equals("quit"))
                    {
                        socket.Shutdown(SocketShutdown.Both);
                        socket.Close();
                    }
                    else
                    {
                        socket.Send(Encoding.UTF8.GetBytes(msgSend));
                        break;
                    }
                }
            }
        }
    }
    #endregion
}
// See https://aka.ms/new-console-template for more information

using System.Net;
using System.Net.Sockets;
using System.Text;

class SendObj{
    public Socket skt;
    public EndPoint pt;
}

class ReceiveData{
    public Socket skt;
    public byte[] dataBytes;
}

class Client
{
    static void Main(String[] args)
    {
        ASyncSendData();
        Console.Read();
    }

#region ASync
    static void ASyncSendData()
    {
        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPAddress ipAddress = new IPAddress(new byte[] { 127, 0, 0, 1 });
        EndPoint pt = new IPEndPoint(ipAddress, 12700);
        Console.WriteLine("Client Start...");
        // connection is only once
        socket.BeginConnect(pt, new AsyncCallback(SendDataHandle), new SendObj(){skt = socket, pt = pt});

    }

    static void SendDataHandle(IAsyncResult result)
    {
        if(result.AsyncState is SendObj args)
        {
            args.skt.EndConnect(result);

            byte[] dataRcv = new byte[1024];
            args.skt.BeginReceive(dataRcv,0,1024,SocketFlags.None,new AsyncCallback(ReceiveDataHandle), new ReceiveData(){skt = args.skt,dataBytes = dataRcv});
        }
    }

    static void ReceiveDataHandle(IAsyncResult result){
        if(result.AsyncState is ReceiveData args)
        {
            int len = args.skt.EndReceive(result);

            string rcvMsg = Encoding.UTF8.GetString(args.dataBytes, 0, len);
            Console.WriteLine("Rcv Server Data: " + rcvMsg);

            args.skt.BeginReceive(args.dataBytes,0,1024,SocketFlags.None,new AsyncCallback(ReceiveDataHandle), new ReceiveData(){skt = args.skt,dataBytes = args.dataBytes});
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
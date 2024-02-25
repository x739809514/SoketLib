// See https://aka.ms/new-console-template for more information

using System.Net;
using System.Net.Sockets;
using System.Text;

class Server
{
    static void Main(String[] args)
    {
        CreateBasicServer();
    }

    static void CreateBasicServer()
    {
        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPAddress ipAddress = new IPAddress(new byte[] { 127, 0, 0, 1 });
        EndPoint pt = new IPEndPoint(ipAddress,12700);
        socket.Bind(pt);
        socket.Listen(100);
        Console.WriteLine("Server Start...");
        Socket skt = socket.Accept();

        string msg = "Connect Successful";
        byte[] bytes = Encoding.UTF8.GetBytes(msg);
        skt.Send(bytes);
        
        //receive data buffer
        byte[] dataRcv = new byte[1024];
        int lenRcv = skt.Receive(dataRcv);

        string msgRcv = Encoding.UTF8.GetString(dataRcv, 0, lenRcv);
        Console.WriteLine("Rcv Client Msg: " + msgRcv);

        Console.ReadKey();
    }
}